package com.psampir.choruko

import android.os.Build
import android.view.View
import android.view.WindowInsets

/**
 * Android 15 enforces edge-to-edge for apps targeting SDK 35, so nothing reserves
 * space for the status and navigation bars. Pad the view by the system bar and
 * cutout insets instead of opting out of edge-to-edge (that flag stops working in
 * Android 16), which keeps the page clear of the clock and the gesture bar on every
 * Android version the app supports.
 *
 * The keyboard counts too: with edge-to-edge the window is not resized for the IME,
 * so the bottom inset becomes the taller of the navigation bar and the keyboard.
 */
fun View.applySystemBarInsets() {
    setOnApplyWindowInsetsListener { view, insets ->
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            val bars = insets.getInsets(
                WindowInsets.Type.systemBars() or WindowInsets.Type.displayCutout()
            )
            val ime = insets.getInsets(WindowInsets.Type.ime())
            view.setPadding(
                bars.left,
                bars.top,
                bars.right,
                maxOf(bars.bottom, ime.bottom),
            )
        } else {
            @Suppress("DEPRECATION")
            view.setPadding(
                insets.systemWindowInsetLeft,
                insets.systemWindowInsetTop,
                insets.systemWindowInsetRight,
                insets.systemWindowInsetBottom,
            )
        }
        insets
    }
}
