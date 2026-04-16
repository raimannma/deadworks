import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import SettingsWindow from "./components/SettingsWindow";
import ServerInfoWindow from "./components/ServerInfoWindow";
import "./index.css";

const path = window.location.pathname;

function Root() {
  if (path === "/settings") return <SettingsWindow />;
  if (path === "/server-info") return <ServerInfoWindow />;
  return <App />;
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <Root />
  </React.StrictMode>
);
