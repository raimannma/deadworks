import { useState, useEffect, useRef } from "react";
import { prepareAndConnect, listenDownloadProgress } from "@/lib/tauri";
import type { Server } from "@/lib/types";
import styles from "./ConnectDialog.module.css";
import { cn } from "@/lib/utils";

interface ConnectDialogProps {
  server: Server;
  apiUrl: string;
  onClose: () => void;
}

export default function ConnectDialog({ server, apiUrl, onClose }: ConnectDialogProps) {
  const [status, setStatus] = useState("Initializing...");
  const [progress, setProgress] = useState<number | null>(null);
  const startedRef = useRef(false);

  useEffect(() => {
    if (startedRef.current) return;
    startedRef.current = true;

    let unlisten: (() => void) | null = null;

    async function run() {
      unlisten = await listenDownloadProgress((p) => {
        if (p.status === "fetching") {
          setStatus("Checking server content...");
        } else if (p.status === "checking") {
          setStatus(`Verifying ${p.name}... (${p.item_index + 1}/${p.total_items})`);
          setProgress(null);
        } else if (p.status === "downloading") {
          const pct = p.total_bytes > 0 ? Math.round((p.bytes_downloaded / p.total_bytes) * 100) : 0;
          setStatus(`Downloading ${p.name}... ${pct}% (${p.item_index + 1}/${p.total_items})`);
          setProgress(pct);
        } else if (p.status === "decompressing") {
          const pct = p.total_bytes > 0 ? Math.round((p.bytes_downloaded / p.total_bytes) * 100) : 0;
          setStatus(`Decompressing ${p.name}... ${pct}% (${p.item_index + 1}/${p.total_items})`);
          setProgress(pct);
        } else if (p.status === "ready") {
          setStatus(`${p.name} verified (${p.item_index + 1}/${p.total_items})`);
          setProgress(100);
        } else if (p.status === "connecting") {
          setStatus("All content verified. Connecting...");
          setProgress(100);
        }
      });

      try {
        setStatus("Checking server content...");
        const result = await prepareAndConnect(server.id, server.raw_address, apiUrl);
        if (result.success) {
          setStatus(result.message);
          setTimeout(onClose, 2000);
        } else {
          setStatus(`Error: ${result.message}`);
        }
      } catch (e) {
        const msg = typeof e === "string" ? e : String(e);
        if (msg.includes("FILE_IN_USE")) {
          setStatus(
            "One of this server's files is currently loaded in Deadlock. " +
              "Please fully disconnect or quit the game, then try joining again."
          );
        } else {
          setStatus(`Error: ${msg}`);
        }
      }
    }

    run();

    return () => {
      unlisten?.();
    };
  }, [server, apiUrl, onClose]);

  return (
    <div className={styles.overlay} onClick={(e) => e.target === e.currentTarget && onClose()}>
      <div className={styles.box}>
        <h3 className={styles.title}>Connecting to server...</h3>
        <p className={styles.serverName}>{server.name}</p>
        <p className={styles.addr}>{server.address}</p>

        <div className={styles.progressTrack}>
          <div
            className={cn(styles.progressBar, progress == null && styles.progressIndeterminate)}
            style={progress != null ? { width: `${progress}%` } : undefined}
          />
        </div>

        <p className={styles.status}>{status}</p>
        <button onClick={onClose} className={styles.cancelBtn}>CANCEL</button>
      </div>
    </div>
  );
}
