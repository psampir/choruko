package com.psampir.choruko

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.TextView

/**
 * First launch (or a "change server" request): asks for the address of the Choruko
 * instance, verifies that it really serves Choruko, and remembers it. The APK
 * therefore works against any self-hosted installation without a rebuild.
 */
class MainActivity : Activity() {

    companion object {
        const val EXTRA_FORCE_SETUP = "com.psampir.choruko.EXTRA_FORCE_SETUP"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val saved = ServerStore.read(this)
        val forceSetup = intent.getBooleanExtra(EXTRA_FORCE_SETUP, false)
        if (saved != null && !forceSetup) {
            openWeb(saved)
            return
        }

        setContentView(R.layout.activity_setup)
        findViewById<View>(android.R.id.content).apply {
            applySystemBarInsets()
            requestApplyInsets()
        }

        val input = findViewById<EditText>(R.id.server_url)
        val error = findViewById<TextView>(R.id.server_error)
        val save = findViewById<Button>(R.id.server_save)

        if (forceSetup && saved != null) {
            input.setText(saved)
        }

        save.setOnClickListener {
            val normalized = ServerStore.normalize(input.text.toString())
            if (normalized == null) {
                showError(error, R.string.server_error_invalid)
                return@setOnClickListener
            }

            verifyAndOpen(normalized, save, error)
        }
    }

    /**
     * The address is only stored once it answers with the Choruko page, so a typo or
     * a random URL does not leave the app pointed at something that is not Choruko.
     */
    private fun verifyAndOpen(url: String, save: Button, error: TextView) {
        save.isEnabled = false
        save.text = getString(R.string.connect_action_progress)
        error.visibility = View.GONE

        Thread {
            val result = ChorukoProbe.check(url)

            runOnUiThread {
                save.isEnabled = true
                save.text = getString(R.string.connect_action)

                when (result) {
                    ChorukoProbe.Result.OK -> {
                        ServerStore.write(this, url)
                        openWeb(url)
                    }

                    ChorukoProbe.Result.NOT_CHORUKO ->
                        showError(error, R.string.server_error_not_choruko)

                    ChorukoProbe.Result.UNREACHABLE ->
                        showError(error, R.string.server_error_unreachable)
                }
            }
        }.start()
    }

    private fun showError(error: TextView, message: Int) {
        error.text = getString(message)
        error.visibility = View.VISIBLE
    }

    private fun openWeb(url: String) {
        startActivity(
            Intent(this, WebActivity::class.java).putExtra(WebActivity.EXTRA_URL, url)
        )
        finish()
    }
}
