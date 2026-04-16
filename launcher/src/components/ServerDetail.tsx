import type { Server } from "@/lib/types";
import styles from "./ServerDetail.module.css";

interface ServerDetailProps {
  server: Server;
  onConnect: () => void;
}

export default function ServerDetail({ server, onConnect }: ServerDetailProps) {
  const isDev = import.meta.env.DEV;

  return (
    <div className={styles.detail}>
      <div className={styles.header}>
        <h3 className={styles.name}>{server.name}</h3>
        <button
          onClick={onConnect}
          disabled={!server.online}
          className={styles.connectBtn}
        >
          CONNECT
        </button>
      </div>

      <div className={styles.body}>
        <div className={styles.row}>
          <span className={styles.label}>Address</span>
          <span className={styles.value}>{server.address}</span>
        </div>
        <div className={styles.row}>
          <span className={styles.label}>Map</span>
          <span className={styles.value}>{server.map || "\u2014"}</span>
        </div>
        <div className={styles.row}>
          <span className={styles.label}>Players</span>
          <span className={styles.value}>{server.player_count} / {server.max_players}</span>
        </div>
        {isDev && (
          <div className={styles.row}>
            <span className={styles.label}>Mods</span>
            <span className={styles.value}>
              {server.mods?.length > 0
                ? server.mods.map((m) => `${m.name} v${m.version}`).join(", ")
                : "None"}
            </span>
          </div>
        )}
        {isDev && (
          <div className={styles.row}>
            <span className={styles.label}>Content Addons</span>
            <span className={styles.value}>
              {server.content_addons?.length > 0 ? server.content_addons.join(", ") : "None"}
            </span>
          </div>
        )}
        <div className={styles.row}>
          <span className={styles.label}>Custom Maps</span>
          <span className={styles.value}>
            {server.extra_maps?.length > 0 ? server.extra_maps.join(", ") : "None"}
          </span>
        </div>
      </div>

      {server.players.length > 0 && (
        <div className={styles.players}>
          <h4 className={styles.playersTitle}>Players ({server.players.length})</h4>
          <table className={styles.playerTable}>
            <thead>
              <tr>
                <th>Name</th>
                <th>Hero</th>
                <th>K</th>
                <th>D</th>
                <th>A</th>
              </tr>
            </thead>
            <tbody>
              {server.players.map((p, i) => (
                <tr key={i}>
                  <td>{p.name}</td>
                  <td>{p.hero}</td>
                  <td>{p.kills}</td>
                  <td>{p.deaths}</td>
                  <td>{p.assists}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
