import { useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";
import Titlebar from "@/components/Titlebar";
import ServersPage from "@/components/ServersPage";
import UpdateManager from "@/components/UpdateManager";
import { useSettings } from "@/hooks/use-settings";
import { getStore } from "@/lib/tauri";
import styles from "./App.module.css";

export default function App() {
  const settings = useSettings();

  // On first launch, enable autostart by default
  useEffect(() => {
    getStore().then(async (store) => {
      const hasBeenSet = await store.get<boolean>("autostart_set");
      if (!hasBeenSet) {
        await invoke("plugin:autostart|enable").catch(() => {});
        await store.set("autostart_set", true);
        await store.save();
      }
    });
  }, []);

  return (
    <>
      <Titlebar />
      <main className={styles.main}>
        <ServersPage apiUrl={settings.apiUrl} />
      </main>
      <UpdateManager />
    </>
  );
}
