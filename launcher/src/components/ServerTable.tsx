import { useState, useRef, useCallback } from "react";
import { cn } from "@/lib/utils";
import type { Server } from "@/lib/types";
import type { SortColumn, SortDirection } from "@/hooks/use-servers";
import styles from "./ServerTable.module.css";

interface ServerTableProps {
  servers: Server[];
  pings: Record<string, number>;
  selectedServerId: string | null;
  onSelect: (server: Server) => void;
  onDoubleClick: (server: Server) => void;
  onContextMenu: (server: Server, e: React.MouseEvent) => void;
  isLoading: boolean;
  error: string | null;
  sortColumn: SortColumn | null;
  sortDirection: SortDirection;
  onToggleSort: (column: SortColumn) => void;
}

const COLUMNS = ["name", "map", "players", "ping"] as const;
const DEFAULT_WIDTHS = [50, 20, 10, 8];

function PingCell({ ms }: { ms: number | undefined }) {
  if (ms == null) {
    return <td className={styles.colPing}>...</td>;
  }
  if (ms < 0) {
    return <td className={styles.colPing}>&mdash;</td>;
  }
  return (
    <td
      className={cn(
        styles.colPing,
        ms < 60 ? styles.pingGood : ms < 120 ? styles.pingOk : styles.pingBad
      )}
    >
      {ms}
    </td>
  );
}

function SortArrow({ column, sortColumn, sortDirection }: { column: SortColumn; sortColumn: SortColumn | null; sortDirection: SortDirection }) {
  if (column !== sortColumn || !sortDirection) return null;
  return <span className={styles.sortArrow}>{sortDirection === "asc" ? "\u25B2" : "\u25BC"}</span>;
}

export default function ServerTable({
  servers,
  pings,
  selectedServerId,
  onSelect,
  onDoubleClick,
  onContextMenu,
  isLoading,
  error,
  sortColumn,
  sortDirection,
  onToggleSort,
}: ServerTableProps) {
  const showEmpty = !isLoading && !error && servers.length === 0;
  const [colWidths, setColWidths] = useState(DEFAULT_WIDTHS);
  const dragRef = useRef<{ idx: number; startX: number; startLeft: number; startRight: number; tableWidth: number } | null>(null);

  const onMouseDown = useCallback((idx: number, e: React.MouseEvent) => {
    e.preventDefault();
    const table = (e.target as HTMLElement).closest("table");
    if (!table) return;
    const tableWidth = table.getBoundingClientRect().width;
    dragRef.current = {
      idx,
      startX: e.clientX,
      startLeft: colWidths[idx],
      startRight: colWidths[idx + 1],
      tableWidth,
    };

    const onMouseMove = (ev: MouseEvent) => {
      if (!dragRef.current) return;
      const { idx: i, startX, startLeft, startRight, tableWidth: tw } = dragRef.current;
      const deltaPercent = ((ev.clientX - startX) / tw) * 100;
      const newLeft = Math.max(5, startLeft + deltaPercent);
      const newRight = Math.max(5, startRight - deltaPercent);
      setColWidths((prev) => {
        const next = [...prev];
        next[i] = newLeft;
        next[i + 1] = newRight;
        return next;
      });
    };

    const onMouseUp = () => {
      dragRef.current = null;
      document.removeEventListener("mousemove", onMouseMove);
      document.removeEventListener("mouseup", onMouseUp);
    };

    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mouseup", onMouseUp);
  }, [colWidths]);

  return (
    <div className={styles.wrap}>
      <table className={styles.table}>
        <colgroup>
          {colWidths.map((w, i) => (
            <col key={i} style={{ width: `${w}%` }} />
          ))}
        </colgroup>
        <thead>
          <tr>
            <th className={cn(styles.sortable, sortColumn === "name" && styles.sortActive)} onClick={() => onToggleSort("name")}>
              Server Name
              <SortArrow column="name" sortColumn={sortColumn} sortDirection={sortDirection} />
              <span className={styles.resizeHandle} onMouseDown={(e) => { e.stopPropagation(); onMouseDown(0, e); }} />
            </th>
            <th className={cn(styles.sortable, sortColumn === "map" && styles.sortActive)} onClick={() => onToggleSort("map")}>
              Map
              <SortArrow column="map" sortColumn={sortColumn} sortDirection={sortDirection} />
              <span className={styles.resizeHandle} onMouseDown={(e) => { e.stopPropagation(); onMouseDown(1, e); }} />
            </th>
            <th className={cn(styles.thPlayers, styles.sortable, sortColumn === "players" && styles.sortActive)} onClick={() => onToggleSort("players")}>
              Players
              <SortArrow column="players" sortColumn={sortColumn} sortDirection={sortDirection} />
              <span className={styles.resizeHandle} onMouseDown={(e) => { e.stopPropagation(); onMouseDown(2, e); }} />
            </th>
            <th className={cn(styles.thPing, styles.sortable, sortColumn === "ping" && styles.sortActive)} onClick={() => onToggleSort("ping")}>
              Ping
              <SortArrow column="ping" sortColumn={sortColumn} sortDirection={sortDirection} />
            </th>
          </tr>
        </thead>
        <tbody>
          {error && (
            <tr className={styles.emptyRow}>
              <td colSpan={4} className={styles.emptyState}>{error}</td>
            </tr>
          )}
          {showEmpty && (
            <tr className={styles.emptyRow}>
              <td colSpan={4} className={styles.emptyState}>No servers found</td>
            </tr>
          )}
          {servers.map((server) => (
            <tr
              key={server.id}
              onClick={() => onSelect(server)}
              onDoubleClick={() => onDoubleClick(server)}
              onContextMenu={(e) => onContextMenu(server, e)}
              className={cn(
                selectedServerId === server.id && styles.selected,
                !server.online && styles.offline
              )}
            >
              <td className={styles.colName}>
                <img
                  className={styles.flag}
                  src={`/flags/${server.country.toLowerCase()}.png`}
                  alt={server.country}
                />
                <span
                  className={cn(
                    styles.statusDot,
                    server.online ? styles.dotOnline : styles.dotOffline
                  )}
                />
                {server.name}
              </td>
              <td className={styles.colMap}>{server.map || "\u2014"}</td>
              <td className={styles.colPlayers}>
                {server.player_count}/{server.max_players}
              </td>
              <PingCell ms={pings[server.id]} />
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
