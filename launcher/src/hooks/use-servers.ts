import { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { fetchServers, pingServer } from "@/lib/tauri";
import type { Server } from "@/lib/types";

export type SortColumn = "name" | "map" | "players" | "ping";
export type SortDirection = "asc" | "desc" | null;

export interface UseServersResult {
  servers: Server[];
  pings: Record<string, number>;
  selectedServer: Server | null;
  selectServer: (server: Server) => void;
  searchQuery: string;
  setSearchQuery: (q: string) => void;
  filteredServers: Server[];
  refreshServers: () => void;
  isLoading: boolean;
  error: string | null;
  serverCount: string;
  sortColumn: SortColumn | null;
  sortDirection: SortDirection;
  toggleSort: (column: SortColumn) => void;
}

export function useServers(apiUrl: string): UseServersResult {
  const [servers, setServers] = useState<Server[]>([]);
  const [pings, setPings] = useState<Record<string, number>>({});
  const [selectedServer, setSelectedServer] = useState<Server | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sortColumn, setSortColumn] = useState<SortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>(null);
  const apiUrlRef = useRef(apiUrl);
  apiUrlRef.current = apiUrl;

  // The natural first-click direction for each column
  const defaultDirection: Record<SortColumn, "asc" | "desc"> = {
    name: "asc",      // A → Z
    map: "asc",       // A → Z
    players: "asc",   // high → low (inverted comparison)
    ping: "asc",      // low → high
  };

  const loadServers = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    const minDelay = new Promise((r) => setTimeout(r, 800));
    try {
      const data = await fetchServers(apiUrlRef.current);
      setServers(data);
      // Kick off pings for all servers
      for (const server of data) {
        pingServer(server.raw_address).then((ms) => {
          setPings((prev) => ({ ...prev, [server.id]: ms }));
        });
      }
    } catch (e) {
      console.error("Failed to fetch servers:", e);
      setServers([]);
      setPings({});
      setSelectedServer(null);
      setError("Could not reach server. Check your connection.");
    } finally {
      await minDelay;
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadServers();
  }, [loadServers, apiUrl]);

  const filteredServers = useMemo(() => {
    const lower = searchQuery.toLowerCase();
    const filtered = servers.filter(
      (s) =>
        s.name.toLowerCase().includes(lower) ||
        s.map.toLowerCase().includes(lower)
    );
    filtered.sort((a, b) => {
      if (a.online !== b.online) return a.online ? -1 : 1;

      const aDefault = a.name === "Deadlock";
      const bDefault = b.name === "Deadlock";
      if (aDefault !== bDefault) return aDefault ? 1 : -1;

      // No sort active — default: player count high → low, ping low → high
      if (!sortColumn || !sortDirection) {
        if (a.player_count !== b.player_count) return b.player_count - a.player_count;
        const aPing = pings[a.id] ?? Infinity;
        const bPing = pings[b.id] ?? Infinity;
        return aPing - bPing;
      }

      const dir = sortDirection === "asc" ? 1 : -1;
      let cmp = 0;
      switch (sortColumn) {
        case "name":
          cmp = a.name.localeCompare(b.name);
          break;
        case "map":
          cmp = (a.map || "").localeCompare(b.map || "");
          break;
        case "players":
          cmp = b.player_count - a.player_count;
          break;
        case "ping": {
          const pa = pings[a.id];
          const pb = pings[b.id];
          const aValid = pa != null && pa >= 0;
          const bValid = pb != null && pb >= 0;
          if (aValid !== bValid) return aValid ? -1 : 1; // no ping always last
          cmp = (pa ?? 0) - (pb ?? 0);
          break;
        }
      }
      return cmp * dir;
    });
    return filtered;
  }, [servers, searchQuery, sortColumn, sortDirection, pings]);

  const totalPlayers = useMemo(
    () => filteredServers.reduce((sum, s) => sum + s.player_count, 0),
    [filteredServers]
  );

  const serverCount = `${filteredServers.length} servers \u2014 ${totalPlayers} players`;

  const selectServer = useCallback((server: Server) => {
    setSelectedServer(server);
  }, []);

  // Click 1: natural direction for that column
  // Click 2: opposite direction
  // Click 3: clear sort
  function toggleSort(column: SortColumn) {
    if (sortColumn !== column) {
      // Different column — start at its natural direction
      setSortColumn(column);
      setSortDirection(defaultDirection[column]);
      return;
    }
    // Same column — cycle: natural → opposite → clear
    const natural = defaultDirection[column];
    const opposite = natural === "asc" ? "desc" : "asc";
    if (sortDirection === natural) {
      setSortDirection(opposite);
    } else {
      setSortColumn(null);
      setSortDirection(null);
    }
  }

  return {
    servers,
    pings,
    selectedServer,
    selectServer,
    searchQuery,
    setSearchQuery,
    filteredServers,
    refreshServers: loadServers,
    isLoading,
    error,
    serverCount,
    sortColumn,
    sortDirection,
    toggleSort,
  };
}
