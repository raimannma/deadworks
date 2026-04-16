import Titlebar from "@/components/Titlebar";
import ServersPage from "@/components/ServersPage";
import UpdateManager from "@/components/UpdateManager";
import { useSettings } from "@/hooks/use-settings";
import styles from "./App.module.css";

export default function App() {
  const settings = useSettings();

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
