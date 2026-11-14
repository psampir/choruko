package com.psampir.choruko

import android.app.Activity
import android.app.AlertDialog
import android.content.Intent
import android.graphics.Color
import android.net.Uri
import android.os.Bundle
import android.webkit.CookieManager
import android.webkit.ValueCallback
import android.webkit.WebChromeClient
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.webkit.WebViewClient

/**
 * Full-screen WebView for the Choruko SPA. The URL comes from [ServerStore], so the
 * same APK works with any instance. Links to other hosts open in the browser, and
 * file inputs (the profile picture) are handed to the system picker.
 */
class WebActivity : Activity() {

    companion object {
        const val EXTRA_URL = "com.psampir.choruko.EXTRA_URL"
        private const val REQUEST_FILE_CHOOSER = 1001
    }

    private lateinit var webView: WebView
    private lateinit var serverUrl: String
    private var filePathCallback: ValueCallback<Array<Uri>>? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val url = intent.getStringExtra(EXTRA_URL) ?: ServerStore.read(this)
        if (url == null) {
            startActivity(Intent(this, MainActivity::class.java))
            finish()
            return
        }
        serverUrl = url

        webView = WebView(this)
        setContentView(webView)

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
                if (target.host.equals(Uri.parse(serverUrl).host, ignoreCase = true)) {
                    return false
                }
                openExternally(target)
                return true
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

    @Deprecated("Framework Activity without AndroidX")
    override fun onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack()
            return
        }

        AlertDialog.Builder(this)
            .setTitle(R.string.exit_title)
            .setItems(
                arrayOf(
                    getString(R.string.exit_action),
                    getString(R.string.change_server_action),
                ),
            ) { _, which ->
                when (which) {
                    0 -> finish()
                    1 -> {
                        ServerStore.clear(this)
                        startActivity(
                            Intent(this, MainActivity::class.java)
                                .putExtra(MainActivity.EXTRA_FORCE_SETUP, true)
                        )
                        finish()
                    }
                }
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    override fun onDestroy() {
        filePathCallback?.onReceiveValue(null)
        filePathCallback = null
        webView.destroy()
        super.onDestroy()
    }
}
