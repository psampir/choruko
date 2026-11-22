package com.psampir.choruko

import java.net.HttpURLConnection
import java.net.URL

/**
 * Checks that an address really serves Choruko before the app stores it. The SPA is
 * a static page, so its <title> identifies the instance; asserting a status code or
 * an API route would accept any server with an API (or a captive portal).
 */
object ChorukoProbe {
    private const val TIMEOUT_MS = 8_000
    private const val MAX_CHARS = 64 * 1024

    enum class Result { OK, NOT_CHORUKO, UNREACHABLE }

    fun check(serverUrl: String): Result {
        return try {
            val connection = (URL(serverUrl).openConnection() as HttpURLConnection).apply {
                connectTimeout = TIMEOUT_MS
                readTimeout = TIMEOUT_MS
                instanceFollowRedirects = true
                requestMethod = "GET"
                setRequestProperty("Accept", "text/html")
            }

            try {
                if (connection.responseCode !in 200..299) {
                    return Result.NOT_CHORUKO
                }

                val body = connection.inputStream.bufferedReader().use { reader ->
                    val buffer = CharArray(MAX_CHARS)
                    val read = reader.read(buffer)
                    if (read <= 0) "" else String(buffer, 0, read)
                }

                if (body.contains("<title>choruko</title>", ignoreCase = true)) {
                    Result.OK
                } else {
                    Result.NOT_CHORUKO
                }
            } finally {
                connection.disconnect()
            }
        } catch (_: Exception) {
            Result.UNREACHABLE
        }
    }
}
