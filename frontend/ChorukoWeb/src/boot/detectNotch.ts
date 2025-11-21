// (() => {
//   // Exit if not in a browser environment
//   if (typeof window === 'undefined' || typeof document === 'undefined') return;

//   // Create a probe element to detect the notch
//   const probe = document.createElement('div');
//   probe.style.cssText =
//     'position:fixed;top:0;height:env(safe-area-inset-top);pointer-events:none;opacity:0;';
//   document.documentElement.appendChild(probe);
//   const height = parseInt(getComputedStyle(probe).height || '0', 10) || 0;
//   probe.remove();

//   if (height > 0) document.documentElement.classList.add('is-notch');
// })();