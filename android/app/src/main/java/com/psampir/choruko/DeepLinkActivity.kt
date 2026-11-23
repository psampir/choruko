package com.psampir.choruko

import android.app.Activity
import android.content.Intent
import android.os.Bundle

/**
 * Entry point for chorukoapp:// links from a browser. The site cannot know whether
 * the app is installed, so it fires such a link and the system routes it here when
 * it is. Everything ends either in the WebView (same server) or on the address
 * screen, pre-filled with the link's server, so another instance is never opened
 * without the user seeing it.
 */
class DeepLinkActivity : Activity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val data = intent.data
        val saved = ServerStore.read(this)
        val linkServer = data?.getQueryParameter("server")?.let(ServerStore::normalize)
        val path = normalizePath(data?.getQueryParameter("path"))

        when {
            data?.host == "change-server" -> startActivity(
                Intent(this, MainActivity::class.java)
                    .setAction(MainActivity.ACTION_CHANGE_SERVER)
            )

            linkServer != null && linkServer == saved -> startActivity(
                Intent(this, WebActivity::class.java)
                    .putExtra(WebActivity.EXTRA_URL, join(linkServer, path))
                    .addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP)
            )

            else -> startActivity(
                Intent(this, MainActivity::class.java)
                    .putExtra(MainActivity.EXTRA_PREFILL_SERVER, linkServer ?: saved)
                    .putExtra(MainActivity.EXTRA_OPEN_PATH, path)
            )
        }

        finish()
    }

    private fun join(server: String, path: String): String =
        server.trimEnd('/') + path.ifEmpty { "/" }

    private fun normalizePath(raw: String?): String {
        val value = raw?.trim().orEmpty()
        if (value.isEmpty()) {
            return ""
        }

        val path = if (value.startsWith("/")) value else "/$value"
        return if (path.startsWith("//")) "/" else path.replace("\\", "")
    }
}
