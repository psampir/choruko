import java.util.Properties

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

val versionNameFromEnv = System.getenv("CHORUKO_VERSION_NAME")?.takeIf { it.isNotBlank() } ?: "1.0.0"
val versionCodeFromEnv = System.getenv("CHORUKO_VERSION_CODE")?.toIntOrNull() ?: 1

/**
 * Release signing comes from the environment so the keystore never lives in the
 * repository: CHORUKO_KEYSTORE_PATH, CHORUKO_KEYSTORE_PASSWORD,
 * CHORUKO_KEY_ALIAS, CHORUKO_KEY_PASSWORD. For local builds the same values can sit
 * in android/keystore.properties, which is git-ignored:
 *
 *   storeFile=/home/you/choruko-release.jks
 *   storePassword=...
 *   keyAlias=choruko
 *   keyPassword=...
 *
 * Without either source the build produces an unsigned release APK, which is enough
 * to check that the project compiles.
 */
val keystoreProperties = Properties().apply {
    val file = rootProject.file("keystore.properties")
    if (file.exists()) {
        file.inputStream().use { load(it) }
    }
}

fun signingValue(env: String, property: String): String? =
    System.getenv(env)?.takeIf { it.isNotBlank() } ?: keystoreProperties.getProperty(property)

val keystorePath = signingValue("CHORUKO_KEYSTORE_PATH", "storeFile")
val hasSigningKey = !keystorePath.isNullOrBlank() && file(keystorePath).exists()

android {
    namespace = "com.psampir.choruko"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.psampir.choruko"
        minSdk = 26
        targetSdk = 35
        versionCode = versionCodeFromEnv
        versionName = versionNameFromEnv
    }

    buildFeatures {
        buildConfig = true
    }

    signingConfigs {
        if (hasSigningKey) {
            create("release") {
                storeFile = file(keystorePath!!)
                storePassword = signingValue("CHORUKO_KEYSTORE_PASSWORD", "storePassword")
                keyAlias = signingValue("CHORUKO_KEY_ALIAS", "keyAlias")
                keyPassword = signingValue("CHORUKO_KEY_PASSWORD", "keyPassword")
                    ?: signingValue("CHORUKO_KEYSTORE_PASSWORD", "storePassword")
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            isShrinkResources = false
            if (hasSigningKey) {
                signingConfig = signingConfigs.getByName("release")
            }
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro",
            )
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }

    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
}

dependencies {
    // Intentionally empty: the shell is a plain WebView with no native plugins.
}
