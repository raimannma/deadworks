import { useState } from "react";
import { getCurrentWindow } from "@tauri-apps/api/window";
import { emitTo } from "@tauri-apps/api/event";
import { useSettings } from "@/hooks/use-settings";
import { cn } from "@/lib/utils";
import styles from "./SettingsWindow.module.css";

const NAV_ITEMS = [
  { id: "general", label: "General" },
  ...(import.meta.env.DEV ? [{ id: "developer", label: "Developer" }] : []),
];

function SettingRow({
  title,
  description,
  control,
}: {
  title: string;
  description: string;
  control: React.ReactNode;
}) {
  return (
    <div className={styles.settingRow}>
      <div className={styles.settingInfo}>
        <div className={styles.settingLabel}>{title}</div>
        <div className={styles.settingDesc}>{description}</div>
      </div>
      <div>{control}</div>
    </div>
  );
}

export default function SettingsWindow() {
  const { apiEndpoint, setApiEndpoint } = useSettings();
  const [activeSection, setActiveSection] = useState("general");
  const win = getCurrentWindow();

  return (
    <div className={styles.window}>
      {/* Titlebar */}
      <div className={styles.titlebar} data-tauri-drag-region>
        <div className={styles.titlebarLeft}>
          <span className={styles.titlebarTitle}>SETTINGS</span>
        </div>
        <div className={styles.windowControls}>
          <button onClick={() => win.minimize()} className={styles.winBtn} aria-label="Minimize">
            <svg width="10" height="1" viewBox="0 0 10 1">
              <rect width="10" height="1" fill="currentColor" />
            </svg>
          </button>
          <button onClick={() => win.toggleMaximize()} className={styles.winBtn} aria-label="Maximize">
            <svg width="10" height="10" viewBox="0 0 10 10">
              <rect width="10" height="10" fill="none" stroke="currentColor" strokeWidth="1" />
            </svg>
          </button>
          <button onClick={() => win.close()} className={cn(styles.winBtn, styles.winClose)} aria-label="Close">
            <svg width="10" height="10" viewBox="0 0 10 10">
              <line x1="0" y1="0" x2="10" y2="10" stroke="currentColor" strokeWidth="1.2" />
              <line x1="10" y1="0" x2="0" y2="10" stroke="currentColor" strokeWidth="1.2" />
            </svg>
          </button>
        </div>
      </div>

      {/* Body */}
      <div className={styles.body}>
        {/* Left sidebar */}
        <div className={styles.sidebar}>
          <div className={styles.navList}>
            {NAV_ITEMS.map((item) => (
              <button
                key={item.id}
                onClick={() => setActiveSection(item.id)}
                className={cn(
                  styles.navItem,
                  activeSection === item.id && styles.navItemActive
                )}
              >
                {item.label}
              </button>
            ))}
          </div>
        </div>

        {/* Right content */}
        <div className={styles.content}>
          {activeSection === "general" && (
            <>
              <h2 className={styles.sectionTitle}>General</h2>
              <div className={styles.sectionSubtitle}>Updates</div>

              <SettingRow
                title="Check for Updates"
                description="Check if a new version of the launcher is available"
                control={
                  <button
                    className={styles.devBtn}
                    onClick={() => emitTo("main", "check-for-updates")}
                  >
                    Check Now
                  </button>
                }
              />
            </>
          )}

          {activeSection === "developer" && (
            <>
              <h2 className={styles.sectionTitle}>Developer</h2>
              <div className={styles.sectionSubtitle}>Developer Tools</div>

              <SettingRow
                title="API Endpoint"
                description="Switch between production and local API for testing"
                control={
                  <select
                    value={apiEndpoint}
                    onChange={(e) => setApiEndpoint(e.target.value)}
                    className={styles.select}
                  >
                    <option value="prod">Production (api.deadworks.net)</option>
                    <option value="local">Local (localhost:8787)</option>
                  </select>
                }
              />

              <SettingRow
                title="Test Update UI"
                description="Simulate an available update to preview the update manager"
                control={
                  <button
                    className={styles.devBtn}
                    onClick={() => emitTo("main", "test-update-ui")}
                  >
                    Trigger
                  </button>
                }
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
}
