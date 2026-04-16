use std::fs;
use std::path::PathBuf;

const DEADLOCK_APP_ID: &str = "1422450";

/// Find Steam install path from the Windows registry.
#[cfg(windows)]
fn find_steam_path() -> Result<PathBuf, String> {
    use winreg::enums::HKEY_LOCAL_MACHINE;
    use winreg::RegKey;

    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let steam_key = hklm
        .open_subkey("SOFTWARE\\WOW6432Node\\Valve\\Steam")
        .map_err(|e| format!("Failed to open Steam registry key: {}", e))?;
    let install_path: String = steam_key
        .get_value("InstallPath")
        .map_err(|e| format!("Failed to read Steam InstallPath: {}", e))?;
    Ok(PathBuf::from(install_path))
}

/// Parse libraryfolders.vdf to find all Steam library paths.
fn find_library_folders(steam_path: &PathBuf) -> Result<Vec<PathBuf>, String> {
    let vdf_path = steam_path.join("steamapps").join("libraryfolders.vdf");
    let content = fs::read_to_string(&vdf_path)
        .map_err(|e| format!("Failed to read {}: {}", vdf_path.display(), e))?;

    let mut folders = Vec::new();
    for line in content.lines() {
        let trimmed = line.trim();
        if trimmed.starts_with("\"path\"") {
            if let Some(val) = extract_vdf_value(trimmed) {
                folders.push(PathBuf::from(val));
            }
        }
    }

    folders.push(steam_path.clone());
    Ok(folders)
}

fn extract_vdf_value(line: &str) -> Option<String> {
    let mut parts = line.splitn(2, "\"path\"");
    parts.next()?;
    let rest = parts.next()?.trim();
    let start = rest.find('"')? + 1;
    let end = rest[start..].find('"')? + start;
    Some(rest[start..end].replace("\\\\", "\\"))
}

/// Find Deadlock's cfg directory across all Steam libraries.
#[cfg(windows)]
pub(crate) fn find_deadlock_cfg_dir() -> Result<PathBuf, String> {
    let steam_path = find_steam_path()?;
    let libraries = find_library_folders(&steam_path)?;

    for lib in &libraries {
        let cfg_dir = lib
            .join("steamapps")
            .join("common")
            .join("Deadlock")
            .join("game")
            .join("citadel")
            .join("cfg");
        if cfg_dir.exists() {
            return Ok(cfg_dir);
        }
    }

    Err("Deadlock installation not found in any Steam library".into())
}

#[cfg(not(windows))]
pub(crate) fn find_deadlock_cfg_dir() -> Result<PathBuf, String> {
    Err("Deadlock detection is only supported on Windows".into())
}

#[derive(serde::Serialize)]
pub struct ConnectResult {
    success: bool,
    method: String,
    message: String,
}

/// Open `steam://connect/<addr>` which tells Steam to launch/join the server.
/// `addr` should be a raw `ip:port` pair from the API.
pub(crate) fn connect_to_server_inner(addr: &str) -> Result<ConnectResult, String> {
    let steam_url = format!("steam://connect/{}", addr);
    open::that(&steam_url).map_err(|e| format!("Failed to open Steam: {}", e))?;
    Ok(ConnectResult {
        success: true,
        method: "steam_connect".into(),
        message: format!("Opening steam://connect/{}", addr),
    })
}

#[tauri::command]
pub fn launch_deadlock() -> Result<(), String> {
    let steam_url = format!("steam://run/{}", DEADLOCK_APP_ID);
    open::that(&steam_url).map_err(|e| format!("Failed to launch Deadlock: {}", e))
}
