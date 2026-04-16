use std::net::UdpSocket;
use std::time::{Duration, Instant};

// A2S_INFO request: header (0xFFFFFFFF) + 'T' + "Source Engine Query\0"
const A2S_INFO: &[u8] = b"\xFF\xFF\xFF\xFF\x54Source Engine Query\x00";

#[tauri::command]
pub async fn ping_server(addr: String) -> i32 {
    tokio::task::spawn_blocking(move || {
        let addr_with_port = if addr.contains(':') {
            addr.clone()
        } else {
            format!("{}:27015", addr)
        };

        let socket = match UdpSocket::bind("0.0.0.0:0") {
            Ok(s) => s,
            Err(_) => return -1,
        };
        socket.set_read_timeout(Some(Duration::from_secs(3))).ok();

        if socket.send_to(A2S_INFO, &addr_with_port).is_err() {
            return -1;
        }

        let start = Instant::now();
        let mut buf = [0u8; 1400];

        // Server may respond with S2C_CHALLENGE — resend with the challenge token appended
        match socket.recv_from(&mut buf) {
            Ok((len, _)) if len >= 5 && buf[4] == 0x41 => {
                let elapsed = start.elapsed();
                // 0x41 = challenge response, resend with challenge
                let mut retry = Vec::with_capacity(A2S_INFO.len() + 4);
                retry.extend_from_slice(A2S_INFO);
                retry.extend_from_slice(&buf[5..9]);
                if socket.send_to(&retry, &addr_with_port).is_err() {
                    return -1;
                }
                match socket.recv_from(&mut buf) {
                    Ok(_) => elapsed.as_millis() as i32,
                    Err(_) => -1,
                }
            }
            Ok(_) => start.elapsed().as_millis() as i32,
            Err(_) => -1,
        }
    })
    .await
    .unwrap_or(-1)
}
