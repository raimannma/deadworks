import { useState, useEffect, useCallback } from "react";
import { listen } from "@tauri-apps/api/event";
import { emitTo } from "@tauri-apps/api/event";
import { getStore, getApiUrl } from "@/lib/tauri";

export interface Settings {
  apiEndpoint: string;
  setApiEndpoint: (endpoint: string) => void;
  apiUrl: string;
}

interface SettingsPayload {
  apiEndpoint: string;
}

export function useSettings(): Settings {
  const [apiEndpoint, setApiEndpointState] = useState("prod");

  useEffect(() => {
    getStore().then(async (store) => {
      const endpoint = await store.get<string>("api_endpoint");
      if (endpoint) setApiEndpointState(endpoint);
    });
  }, []);

  useEffect(() => {
    const unlisten = listen<SettingsPayload>("settings-changed", (event) => {
      setApiEndpointState(event.payload.apiEndpoint);
    });
    return () => { unlisten.then((fn) => fn()); };
  }, []);

  const setApiEndpoint = useCallback(async (endpoint: string) => {
    setApiEndpointState(endpoint);
    const store = await getStore();
    await store.set("api_endpoint", endpoint);
    await store.save();
    const payload: SettingsPayload = { apiEndpoint: endpoint };
    emitTo("main", "settings-changed", payload);
  }, []);

  return {
    apiEndpoint,
    setApiEndpoint,
    apiUrl: getApiUrl(apiEndpoint),
  };
}
