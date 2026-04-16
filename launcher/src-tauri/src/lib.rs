mod addons;
mod connect;
mod gameinfo;
mod ping;

/// Try to patch gameinfo.gi at startup. If the game is running the file is
/// locked, so we just log and move on — the user will be told at connect time.
fn patch_gameinfo_on_startup() {
    let cfg_dir = match connect::find_deadlock_cfg_dir() {
        Ok(d) => d,
        Err(_) => return, // Deadlock not installed, nothing to do
    };
    let game_dir = match cfg_dir.parent().and_then(|p| p.parent()) {
        Some(d) => d,
        None => return,
    };
    match gameinfo::ensure_addonroot(game_dir) {
        Ok(true) => println!("[startup] Patched gameinfo.gi with addonroot"),
        Ok(false) => {} // already present
        Err(e) => println!("[startup] Could not patch gameinfo.gi (game may be running): {}", e),
    }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    // Try to patch gameinfo.gi early — will fail silently if game is running
    patch_gameinfo_on_startup();

    tauri::Builder::default()
        .plugin(tauri_plugin_updater::Builder::new().build())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_process::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_store::Builder::default().build())
        .plugin(tauri_plugin_autostart::init(
            tauri_plugin_autostart::MacosLauncher::LaunchAgent,
            None,
        ))
        .invoke_handler(tauri::generate_handler![
            connect::launch_deadlock,
            addons::prepare_and_connect,
            ping::ping_server,
        ])
        .setup(|app| {
            use tauri::menu::{Menu, MenuItem};
            use tauri::tray::{MouseButton, MouseButtonState, TrayIconBuilder, TrayIconEvent};
            use tauri::Manager;
            use tauri_plugin_autostart::ManagerExt;

            // Ensure autostart is always enabled
            let _ = app.autolaunch().enable();

            let show = MenuItem::with_id(app, "show", "Show", true, None::<&str>)?;
            let launch = MenuItem::with_id(app, "launch", "Launch Deadlock", true, None::<&str>)?;
            let quit = MenuItem::with_id(app, "quit", "Quit", true, None::<&str>)?;
            let menu = Menu::with_items(app, &[&show, &launch, &quit])?;

            let _tray = TrayIconBuilder::new()
                .icon(app.default_window_icon().unwrap().clone())
                .menu(&menu)
                .tooltip("Deadworks")
                .on_tray_icon_event(|tray, event| {
                    if let TrayIconEvent::Click {
                        button: MouseButton::Left,
                        button_state: MouseButtonState::Up,
                        ..
                    } = event
                    {
                        let app = tray.app_handle();
                        if let Some(window) = app.get_webview_window("main") {
                            let _ = window.show();
                            let _ = window.unminimize();
                            let _ = window.set_focus();
                        }
                    }
                })
                .on_menu_event(|app, event| match event.id.as_ref() {
                    "show" => {
                        if let Some(window) = app.get_webview_window("main") {
                            let _ = window.show();
                            let _ = window.unminimize();
                            let _ = window.set_focus();
                        }
                    }
                    "launch" => {
                        let _ = open::that(format!("steam://run/{}", "1422450"));
                    }
                    "quit" => {
                        app.exit(0);
                    }
                    _ => {}
                })
                .build(app)?;

            Ok(())
        })
        .on_window_event(|window, event| {
            if let tauri::WindowEvent::CloseRequested { api, .. } = event {
                // Only hide the main window to tray; let other windows close normally
                if window.label() == "main" {
                    api.prevent_close();
                    let _ = window.hide();
                }
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
