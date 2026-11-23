import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.scss";
import App from "./App";
import { tryOpenInAppOnStart } from "./utils/openInApp";


createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>
);

// On Android, hand the page over to the installed shell app once per session.
setTimeout(tryOpenInAppOnStart, 0);
