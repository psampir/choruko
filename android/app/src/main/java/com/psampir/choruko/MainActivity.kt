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
 * instance and remembers it. The APK therefore works against any self-hosted
 * installation without a rebuild.
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

        val input = findViewById<EditText>(R.id.server_url)
        val error = findViewById<TextView>(R.id.server_error)
        val save = findViewById<Button>(R.id.server_save)

        if (forceSetup && saved != null) {
            input.setText(saved)
        }

        save.setOnClickListener {
            val normalized = ServerStore.normalize(input.text.toString())
            if (normalized == null) {
                error.text = getString(R.string.server_error_invalid)
                error.visibility = View.VISIBLE
                return@setOnClickListener
            }

            ServerStore.write(this, normalized)
            openWeb(normalized)
        }
    }

    private fun openWeb(url: String) {
        startActivity(
            Intent(this, WebActivity::class.java).putExtra(WebActivity.EXTRA_URL, url)
        )
        finish()
    }
}
