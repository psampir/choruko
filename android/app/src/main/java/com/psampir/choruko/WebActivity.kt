package com.psampir.choruko

import android.app.Activity
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.Color
import android.net.Uri
import android.os.Bundle
import android.view.View
import android.webkit.CookieManager
import android.webkit.ValueCallback
import android.webkit.WebChromeClient
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.webkit.WebViewClient
import android.widget.Button
import android.widget.TextView

/**
 * Full-screen WebView for the Choruko SPA. The URL comes from [ServerStore], so the
 * same APK works with any instance. Links to other hosts open in the browser, file
 * inputs (the profile picture) go to the system picker, and an unreachable server
 * shows a native screen with Retry and Change server instead of a browser error page.
 */
class WebActivity : Activity() {

    companion object {
        const val EXTRA_URL = "com.psampir.choruko.EXTRA_URL"

        /** Navigating to this URL from the SPA opens the server address screen. */
        private const val APP_SCHEME = "chorukoapp"
        private const val CHANGE_SERVER_HOST = "change-server"

        private const val REQUEST_FILE_CHOOSER = 1001
    }

    private lateinit var webView: WebView
    private lateinit var errorView: View
    private lateinit var serverUrl: String
    private var filePathCallback: ValueCallback<Array<Uri>>? = null
    private var mainFrameFailed = false
    private var failedUrl: String? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val url = intent.getStringExtra(EXTRA_URL) ?: ServerStore.read(this)
        if (url == null) {
            startActivity(Intent(this, MainActivity::class.java))
            finish()
            return
        }
        serverUrl = url

        setContentView(R.layout.activity_web)
        // Insets are dispatched to the content frame, not to the views inside it,
        // which consumes them when it comes from a Material theme.
        findViewById<View>(android.R.id.content).apply {
            applySystemBarInsets()
            requestApplyInsets()
        }

        webView = findViewById(R.id.web_view)
        errorView = findViewById(R.id.connection_error)

        findViewById<TextView>(R.id.connection_error_message).text =
            getString(R.string.connection_error_message, serverUrl)
        findViewById<Button>(R.id.connection_retry).setOnClickListener { retry() }
        findViewById<Button>(R.id.connection_change_server).setOnClickListener { openServerSetup() }

        webView.setBackgroundColor(Color.BLACK)

        CookieManager.getInstance().setAcceptCookie(true)

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
            mediaPlaybackRequiresUserGesture = false
            userAgentString = "$userAgentString ChorukoApp/${BuildConfig.VERSION_NAME}"
        }

        webView.webViewClient = object : WebViewClient() {
            override fun shouldOverrideUrlLoading(
                view: WebView,
                request: WebResourceRequest,
            ): Boolean {
                val target = request.url

                if (target.scheme == APP_SCHEME) {
                    if (target.host == CHANGE_SERVER_HOST) {
                        openServerSetup()
                    }
                    return true
                }

                if (target.host.equals(Uri.parse(serverUrl).host, ignoreCase = true)) {
                    return false
                }

                openExternally(target)
                return true
            }

            override fun onPageStarted(view: WebView, url: String?, favicon: Bitmap?) {
                mainFrameFailed = false
            }

            override fun onPageFinished(view: WebView, url: String?) {
                if (!mainFrameFailed) {
                    errorView.visibility = View.GONE
                }
            }

            override fun onReceivedError(
                view: WebView,
                request: WebResourceRequest,
                error: WebResourceError,
            ) {
                if (request.isForMainFrame) {
                    mainFrameFailed = true
                    failedUrl = request.url.toString()
                    errorView.visibility = View.VISIBLE
                }
            }
        }

        webView.webChromeClient = object : WebChromeClient() {
            override fun onShowFileChooser(
                view: WebView,
                callback: ValueCallback<Array<Uri>>,
                params: FileChooserParams,
            ): Boolean {
                filePathCallback?.onReceiveValue(null)
                filePathCallback = callback

                return try {
                    startActivityForResult(params.createIntent(), REQUEST_FILE_CHOOSER)
                    true
                } catch (error: Exception) {
                    filePathCallback = null
                    false
                }
            }
        }

        webView.loadUrl(serverUrl)
    }

    private fun retry() {
        errorView.visibility = View.GONE
        webView.loadUrl(failedUrl ?: serverUrl)
    }

    /**
     * Opens the address screen with the current server filled in. The stored address
     * only changes once a new one passes the Choruko check, so backing out keeps it.
     */
    private fun openServerSetup() {
        startActivity(
            Intent(this, MainActivity::class.java)
                .setAction(MainActivity.ACTION_CHANGE_SERVER)
        )
    }

    private fun openExternally(uri: Uri) {
        try {
            startActivity(Intent(Intent.ACTION_VIEW, uri))
        } catch (_: Exception) {
            // No browser installed: stay in the app instead of crashing.
        }
    }

    @Deprecated("Framework Activity without AndroidX")
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        if (requestCode == REQUEST_FILE_CHOOSER) {
            val callback = filePathCallback ?: return
            filePathCallback = null
            callback.onReceiveValue(parseFileChooserResult(resultCode, data))
            return
        }
        super.onActivityResult(requestCode, resultCode, data)
    }

    private fun parseFileChooserResult(resultCode: Int, data: Intent?): Array<Uri>? {
        if (resultCode != RESULT_OK) {
            return null
        }

        val clipData = data?.clipData
        if (clipData != null) {
            return Array(clipData.itemCount) { index -> clipData.getItemAt(index).uri }
        }

        return data?.data?.let { arrayOf(it) }
    }

    /**
     * Back walks the SPA history; at its start the app goes to the background like
     * any other Android app, keeping the session and the current page.
     */
    @Deprecated("Framework Activity without AndroidX")
    override fun onBackPressed() {
        if (errorView.visibility != View.VISIBLE && webView.canGoBack()) {
            webView.goBack()
            return
        }

        moveTaskToBack(true)
    }

    override fun onDestroy() {
        filePathCallback?.onReceiveValue(null)
        filePathCallback = null
        if (::webView.isInitialized) {
            webView.destroy()
        }
        super.onDestroy()
    }
}
