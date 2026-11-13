package com.psampir.choruko

import android.content.Context
import android.net.Uri

/**
 * Stores the URL of the Choruko instance the app talks to. Nothing is baked into
 * the APK: the first launch asks for the address and every later launch reuses it.
 */
object ServerStore {
    private const val PREFS = "choruko"
    private const val KEY_SERVER_URL = "server_url"

    fun read(context: Context): String? =
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .getString(KEY_SERVER_URL, null)

    fun write(context: Context, url: String) {
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .putString(KEY_SERVER_URL, url)
            .apply()
    }

    fun clear(context: Context) {
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .remove(KEY_SERVER_URL)
            .apply()
    }

    /**
     * Accepts "host", "host:port" and full URLs, and returns a URL that ends with
     * a slash. Returns null when the value cannot be a server address.
     */
    fun normalize(raw: String): String? {
        var value = raw.trim()
        if (value.isEmpty()) {
            return null
        }

        if (!value.startsWith("http://") && !value.startsWith("https://")) {
            value = "https://$value"
        }

        val uri = Uri.parse(value)
        if (uri.host.isNullOrBlank()) {
            return null
        }
        if (uri.scheme != "http" && uri.scheme != "https") {
            return null
        }

        return value.trimEnd('/') + "/"
    }
}
