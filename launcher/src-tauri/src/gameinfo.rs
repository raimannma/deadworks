use std::path::Path;

const ADDONROOT_VALUE: &str = "citadel/deadworks_addons";

/// Check if an uncommented line contains `addonroot` with our specific value.
fn has_addonroot_line(content: &str) -> bool {
    content.lines().any(|line| {
        let trimmed = line.trim();
        // Skip comments
        if trimmed.starts_with("//") {
            return false;
        }
        trimmed.starts_with("addonroot") && trimmed.contains(ADDONROOT_VALUE)
    })
}

/// Check if gameinfo.gi already contains the addonroot entry.
pub fn has_addonroot(game_dir: &Path) -> Result<bool, String> {
    let gi_path = game_dir.join("citadel").join("gameinfo.gi");
    if !gi_path.exists() {
        return Err(format!("gameinfo.gi not found at {}", gi_path.display()));
    }
    let content = std::fs::read_to_string(&gi_path)
        .map_err(|e| format!("Failed to read gameinfo.gi: {}", e))?;
    Ok(has_addonroot_line(&content))
}

/// Ensure gameinfo.gi has `addonroot deadworks_mods` in its SearchPaths block.
/// Returns Ok(true) if patched, Ok(false) if already present.
pub fn ensure_addonroot(game_dir: &Path) -> Result<bool, String> {
    let gi_path = game_dir.join("citadel").join("gameinfo.gi");
    if !gi_path.exists() {
        return Err(format!("gameinfo.gi not found at {}", gi_path.display()));
    }

    let content = std::fs::read_to_string(&gi_path)
        .map_err(|e| format!("Failed to read gameinfo.gi: {}", e))?;

    // Already patched?
    if has_addonroot_line(&content) {
        return Ok(false);
    }

    let lines: Vec<&str> = content.lines().collect();
    let mut result: Vec<String> = Vec::with_capacity(lines.len() + 1);
    let mut insert_idx: Option<usize> = None;
    let mut in_search_paths = false;

    for (i, line) in lines.iter().enumerate() {
        result.push(line.to_string());
        let trimmed = line.trim();

        if trimmed.contains("SearchPaths") {
            in_search_paths = true;
            continue;
        }
        if in_search_paths && trimmed == "{" {
            insert_idx = Some(i + 1);
            in_search_paths = false;
        }
    }

    let idx = insert_idx
        .ok_or_else(|| "Could not find SearchPaths block in gameinfo.gi".to_string())?;

    // Detect indentation from the next line after the brace
    let indent = if idx < lines.len() {
        let next = lines[idx];
        let stripped = next.trim_start();
        &next[..next.len() - stripped.len()]
    } else {
        "\t\t"
    };

    result.insert(idx, format!("{}addonroot\t{}", indent, ADDONROOT_VALUE));

    // Preserve original line ending style
    let newline = if content.contains("\r\n") { "\r\n" } else { "\n" };
    let new_content = result.join(newline);

    std::fs::write(&gi_path, &new_content)
        .map_err(|e| format!("Failed to write gameinfo.gi: {}", e))?;

    Ok(true)
}
