/* Choruko promo site — no dependencies, no build step. */
(function () {
  "use strict";

  var root = document.documentElement;
  var THEME_KEY = "choruko-site-theme";
  var systemQuery = window.matchMedia("(prefers-color-scheme: dark)");

  var screens = {
    tasks: {
      title: "Chores with real deadlines",
      desc: "Overdue chores glow red, the rest count down to their deadline.",
      alt: "Choruko tasks screen showing overtime, to-do and completed chores",
    },
    leaderboard: {
      title: "A leaderboard that settles arguments",
      desc: "Monthly, yearly and all-time points for every roommate.",
      alt: "Choruko leaderboard ranking four roommates by points",
    },
    settlements: {
      title: "Shared costs, clearly split",
      desc: "See what is due, what is paid, and who still owes what.",
      alt: "Choruko settlements screen showing amounts due and amounts paid",
    },
    household: {
      title: "Admin without the spreadsheet",
      desc: "Household settings, members and admin actions in one place.",
      alt: "Choruko manage household screen with settings and participants",
    },
    account: {
      title: "Notifications that just work",
      desc: "Copy your private ntfy topic once — alerts arrive on your phone.",
      alt: "Choruko manage account screen with the private ntfy topic",
    },
  };

  var heroShot = document.getElementById("hero-shot");
  var heroTitle = document.getElementById("hero-title");
  var heroDesc = document.getElementById("hero-desc");
  var themeToggle = document.getElementById("theme-toggle");
  var galleryToggle = document.getElementById("gallery-theme");
  var lightbox = document.getElementById("lightbox");
  var lightboxImage = document.getElementById("lightbox-image");
  var lightboxCaption = document.getElementById("lightbox-caption");
  var lightboxClose = lightbox ? lightbox.querySelector(".lightbox-close") : null;
  var lightboxShot = null;
  var lastFocused = null;

  function shotSrc(shot, theme) {
    return "assets/screenshots/" + shot + "-" + theme + ".webp";
  }

  function currentTheme() {
    return root.getAttribute("data-theme") === "dark" ? "dark" : "light";
  }

  function storedTheme() {
    try {
      var value = localStorage.getItem(THEME_KEY);
      return value === "light" || value === "dark" ? value : null;
    } catch (e) {
      return null;
    }
  }

  function syncThemeUi(theme) {
    if (themeToggle) {
      themeToggle.setAttribute(
        "aria-label",
        theme === "dark" ? "Switch to light theme" : "Switch to dark theme"
      );
      themeToggle.setAttribute("title", theme === "dark" ? "Light mode" : "Dark mode");
    }
    if (galleryToggle) {
      var choices = galleryToggle.querySelectorAll("[data-theme-choice]");
      for (var i = 0; i < choices.length; i++) {
        choices[i].setAttribute(
          "aria-pressed",
          choices[i].getAttribute("data-theme-choice") === theme ? "true" : "false"
        );
      }
    }
  }

  function applyTheme(theme, persist) {
    if (theme !== "light" && theme !== "dark") {
      theme = systemQuery.matches ? "dark" : "light";
    }
    root.setAttribute("data-theme", theme);
    if (persist) {
      try {
        localStorage.setItem(THEME_KEY, theme);
      } catch (e) { /* private mode */ }
    }

    var shots = document.querySelectorAll("img[data-shot]");
    for (var i = 0; i < shots.length; i++) {
      var shot = shots[i].getAttribute("data-shot");
      var next = shotSrc(shot, theme);
      if (shots[i].getAttribute("src") !== next) {
        shots[i].setAttribute("src", next);
      }
    }

    if (lightboxShot && lightboxImage) {
      lightboxImage.setAttribute("src", shotSrc(lightboxShot, theme));
    }

    syncThemeUi(theme);
  }

  // Theme buttons
  if (themeToggle) {
    themeToggle.addEventListener("click", function () {
      applyTheme(currentTheme() === "dark" ? "light" : "dark", true);
    });
  }

  if (galleryToggle) {
    galleryToggle.addEventListener("click", function (event) {
      var button = event.target.closest("[data-theme-choice]");
      if (!button) return;
      applyTheme(button.getAttribute("data-theme-choice"), true);
    });
  }

  // Follow the OS only until the visitor picks a theme themselves.
  var onSystemChange = function () {
    if (!storedTheme()) applyTheme(systemQuery.matches ? "dark" : "light", false);
  };
  if (systemQuery.addEventListener) {
    systemQuery.addEventListener("change", onSystemChange);
  } else if (systemQuery.addListener) {
    systemQuery.addListener(onSystemChange);
  }

  applyTheme(currentTheme(), false);

  // Hero screen switcher
  var tabs = document.querySelectorAll(".phone-tab");
  function selectScreen(shot) {
    if (!screens[shot]) return;
    for (var i = 0; i < tabs.length; i++) {
      var active = tabs[i].getAttribute("data-screen") === shot;
      tabs[i].classList.toggle("is-active", active);
      tabs[i].setAttribute("aria-selected", active ? "true" : "false");
    }
    if (heroShot) {
      heroShot.setAttribute("data-shot", shot);
      heroShot.setAttribute("src", shotSrc(shot, currentTheme()));
      heroShot.setAttribute("alt", screens[shot].alt);
    }
    if (heroTitle) heroTitle.textContent = screens[shot].title;
    if (heroDesc) heroDesc.textContent = screens[shot].desc;
  }

  for (var t = 0; t < tabs.length; t++) {
    tabs[t].addEventListener("click", function () {
      selectScreen(this.getAttribute("data-screen"));
    });
  }

  // Lightbox
  function openLightbox(shot, title) {
    if (!lightbox || !lightboxImage) return;
    lightboxShot = shot;
    lastFocused = document.activeElement;
    lightboxImage.setAttribute("src", shotSrc(shot, currentTheme()));
    lightboxImage.setAttribute("alt", screens[shot] ? screens[shot].alt : title);
    if (lightboxCaption) lightboxCaption.textContent = title;
    lightbox.classList.add("is-open");
    lightbox.setAttribute("aria-hidden", "false");
    document.body.classList.add("lightbox-open");
    if (lightboxClose) lightboxClose.focus();
  }

  function closeLightbox() {
    if (!lightbox || !lightbox.classList.contains("is-open")) return;
    lightbox.classList.remove("is-open");
    lightbox.setAttribute("aria-hidden", "true");
    document.body.classList.remove("lightbox-open");
    lightboxShot = null;
    if (lastFocused && lastFocused.focus) lastFocused.focus();
    lastFocused = null;
  }

  var shotButtons = document.querySelectorAll("[data-lightbox]");
  for (var b = 0; b < shotButtons.length; b++) {
    shotButtons[b].addEventListener("click", function () {
      openLightbox(
        this.getAttribute("data-lightbox"),
        this.getAttribute("data-title") || "Choruko screenshot"
      );
    });
  }

  if (lightboxClose) lightboxClose.addEventListener("click", closeLightbox);
  if (lightbox) {
    lightbox.addEventListener("click", function (event) {
      if (event.target === lightbox) closeLightbox();
    });
  }

  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
      closeLightbox();
      closeMenu();
    }
  });

  // Copy buttons
  function fallbackCopy(text) {
    var area = document.createElement("textarea");
    area.value = text;
    area.setAttribute("readonly", "");
    area.style.position = "fixed";
    area.style.opacity = "0";
    document.body.appendChild(area);
    area.select();
    var ok = false;
    try {
      ok = document.execCommand("copy");
    } catch (e) {
      ok = false;
    }
    document.body.removeChild(area);
    return ok;
  }

  function copyText(text) {
    if (navigator.clipboard && window.isSecureContext) {
      return navigator.clipboard.writeText(text);
    }
    return new Promise(function (resolve, reject) {
      fallbackCopy(text) ? resolve() : reject(new Error("copy failed"));
    });
  }

  var copyButtons = document.querySelectorAll("[data-copy]");
  for (var c = 0; c < copyButtons.length; c++) {
    copyButtons[c].addEventListener("click", function () {
      var button = this;
      var target = document.getElementById(button.getAttribute("data-copy"));
      if (!target) return;
      var label = button.querySelector("span");
      var original = label ? label.textContent : "Copy";

      copyText(target.innerText.replace(/\u00a0/g, " "))
        .then(function () {
          button.classList.add("is-copied");
          if (label) label.textContent = "Copied!";
          window.setTimeout(function () {
            button.classList.remove("is-copied");
            if (label) label.textContent = original;
          }, 2000);
        })
        .catch(function () {
          if (label) label.textContent = "Select & copy";
          window.setTimeout(function () {
            if (label) label.textContent = original;
          }, 2200);
        });
    });
  }

  // Mobile navigation
  var burger = document.getElementById("nav-burger");
  var navLinks = document.getElementById("nav-links");

  function closeMenu() {
    if (!burger || !navLinks) return;
    navLinks.classList.remove("is-open");
    burger.setAttribute("aria-expanded", "false");
  }

  if (burger && navLinks) {
    burger.addEventListener("click", function () {
      var open = navLinks.classList.toggle("is-open");
      burger.setAttribute("aria-expanded", open ? "true" : "false");
    });

    navLinks.addEventListener("click", function (event) {
      if (event.target.closest("a")) closeMenu();
    });

    document.addEventListener("click", function (event) {
      if (!navLinks.classList.contains("is-open")) return;
      if (event.target.closest(".site-nav")) return;
      closeMenu();
    });
  }

  // Scrollspy for the primary nav
  var navAnchors = Array.prototype.slice.call(
    document.querySelectorAll('.nav-links a[href^="#"]')
  );
  var spySections = navAnchors
    .map(function (link) {
      var id = link.getAttribute("href").slice(1);
      var section = document.getElementById(id);
      return section ? { link: link, section: section } : null;
    })
    .filter(Boolean);

  var spyQueued = false;
  function updateSpy() {
    spyQueued = false;
    var y = window.scrollY + 130;
    var active = null;
    for (var i = 0; i < spySections.length; i++) {
      if (spySections[i].section.offsetTop <= y) active = spySections[i];
    }
    for (var j = 0; j < spySections.length; j++) {
      spySections[j].link.classList.toggle(
        "is-active",
        !!active && spySections[j] === active
      );
    }
  }

  function queueSpy() {
    if (spyQueued) return;
    spyQueued = true;
    window.requestAnimationFrame(updateSpy);
  }

  window.addEventListener("scroll", queueSpy, { passive: true });
  window.addEventListener("resize", queueSpy);
  updateSpy();

  // Reveal on scroll
  var revealItems = document.querySelectorAll(".reveal");
  root.setAttribute("data-reveal-ready", "1");

  if ("IntersectionObserver" in window) {
    var revealObserver = new IntersectionObserver(
      function (entries) {
        for (var i = 0; i < entries.length; i++) {
          if (!entries[i].isIntersecting) continue;
          entries[i].target.classList.add("is-visible");
          revealObserver.unobserve(entries[i].target);
        }
      },
      { rootMargin: "0px 0px -8% 0px", threshold: 0.05 }
    );
    for (var r = 0; r < revealItems.length; r++) {
      revealObserver.observe(revealItems[r]);
    }
  } else {
    for (var q = 0; q < revealItems.length; q++) {
      revealItems[q].classList.add("is-visible");
    }
  }
})();
