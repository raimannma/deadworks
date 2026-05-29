# Deadworks

A server-side modding framework for [Deadlock](https://store.steampowered.com/app/1422450/Deadlock/).

> **Early development** — APIs are not finalized and will change without notice. We are not distributing prebuilt binaries at this time. Early users and contributors should build from source.

## Prerequisites

### Visual Studio

Install [Visual Studio 2026](https://visualstudio.microsoft.com/) with the following workloads:

- **Desktop development with C++**
- **.NET desktop development**

### .NET 10 SDK

Download the latest .NET 10.x.x SDK from [dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

After installing, locate the `nethost` static library. Note down this path for `local.props` later.

The path will look like: `C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\10.0.5\runtimes\win-x64\native`

### protobuf 3.21.8

The native layer statically links `libprotobuf.lib`. You need protobuf **3.21.8** headers and a Release build of the static library.

1. Clone the protobuf 3.21.8 source:
   ```
   git clone --branch v3.21.8 --depth 1 https://github.com/protocolbuffers/protobuf.git protobuf-3.21.8
   ```
2. Configure with CMake:
   ```
   cmake -B build -DCMAKE_BUILD_TYPE=Release -Dprotobuf_BUILD_TESTS=OFF -Dprotobuf_MSVC_STATIC_RUNTIME=ON
   ```
3. Build:
   ```
   cmake --build build --config Release
   ```
4. After a successful build, you should have `libprotobuf.lib` in `build/Release/`. Note the paths to:
   - `src/` - headers (used for `ProtobufIncludeDir` in `local.props`)
   - `build/Release/` - static library (used for `ProtobufLibDir` in `local.props`)

### Deadlock

Install [Deadlock](https://store.steampowered.com/app/1422450/Deadlock/) via Steam. You'll need the path to `game\bin\win64\` for deployment.

The path will look like: `C:\Program Files (x86)\Steam\steamapps\common\Deadlock\game\bin\win64`

## Building

1. Clone with submodules:
   ```
   git clone --recurse-submodules https://github.com/Deadworks-net/deadworks.git
   ```

2. Copy `local.props.example` to `local.props` and fill in your paths:
   - `ProtobufIncludeDir` — protobuf `src/` directory (e.g. `C:\protobuf-3.21.8\src`)
   - `ProtobufLibDir` — protobuf build output (e.g. `C:\protobuf-3.21.8\build\Release`)
   - `NetHostDir` — .NET `nethost` native directory (see above)
   - `DeadlockDir` — Deadlock `game\bin\win64` (optional, enables automatic post-build deploy)

3. Open `deadworks.slnx` in Visual Studio and build (x64 Release).

4. Run the built `deadworks.exe` from `<Deadlock>/game/bin/win64/` to start the Deadworks server.

5. Open your game and connect via console `connect localhost:27067`

## Hosting with Docker (Linux)

You can run a Deadworks server on Linux using the pre-built Docker image — no Windows machine or build toolchain required.

### Requirements

- Docker and Docker Compose
- A Steam account that owns [Deadlock](https://store.steampowered.com/app/1422450/Deadlock/) (used to download server files on first start)

### Quick start

1. Create a directory for your server and add a `docker-compose.yaml`:
   ```yaml
   services:
     deadworks:
       image: ghcr.io/deadworks-net/deadworks:latest
       ports:
         - "27015:27015/udp"
         - "27015:27015/tcp"
       env_file: .env
       volumes:
         - /etc/machine-id:/etc/machine-id:ro
         - proton:/opt/proton
         - gamedata:/home/steam/server
         - compatdata:/home/steam/.steam/steam/steamapps/compatdata
         - dotnet-cache:/opt/dotnet-cache
       restart: unless-stopped

   volumes:
     proton:
     gamedata:
     compatdata:
     dotnet-cache:
   ```

2. Create a `.env` file with your configuration:
   ```
   STEAM_LOGIN=your_username
   STEAM_PASSWORD=your_password

   SERVER_PORT=27015
   SERVER_MAP=dl_midtown
   SERVER_PASSWORD=
   RCON_PASSWORD=
   PROTON_VERSION=GE-Proton10-33
   DOTNET_VERSION=10.0.0
   DEADWORKS_ARGS=
   ```

   `STEAM_LOGIN` and `STEAM_PASSWORD` are required. The rest have sensible defaults.

3. Start the server:
   ```
   docker compose up -d
   ```

   On first launch the container will download GE-Proton, the Deadlock game files via SteamCMD, and the .NET Windows runtime. These are cached in named volumes so subsequent starts are fast.

4. View logs:
   ```
   docker compose logs -f
   ```

5. Connect from your game: `connect <your-server-ip>:27015`

### Including custom plugins

To include plugins built from source outside the repo, use `additional_contexts` in your compose file and build locally:

```yaml
services:
  deadworks:
    build:
      context: .
      dockerfile: docker/Dockerfile
      additional_contexts:
        extra-plugins: ../my-deadworks-plugins
```

Each subdirectory under the extra-plugins path should contain a `.csproj` that references `DeadworksManaged.Api`.

### Plugin hot-reload (Linux dev workflow)

For iterating on plugins without rebuilding the image or restarting the server, run the
server in dev mode and build your plugins on the host. The server's `PluginLoader` watches
its plugins directory and live-reloads any DLL that changes (load / reload / unload).

**Requirements:** Docker + Docker Compose, and the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
on the host (`dotnet --version` should report `10.x`).

1. Start the server with the dev override. This bind-mounts `./plugins-live` into the
   container as the live plugins directory and enables hot-reload:

   ```
   docker compose -f docker-compose.yaml -f docker-compose.dev.yaml up --build
   ```

2. In a second terminal, build a plugin into `./plugins-live` and watch for changes:

   ```
   ./scripts/dev-plugins.sh examples/plugins/RollTheDicePlugin
   ```

   You can point the script at any plugin project, including your own outside the repo
   (it just needs a `.csproj` that references `DeadworksManaged.Api`):

   ```
   ./scripts/dev-plugins.sh ../my-plugins/CoolPlugin
   ```

   Edit and save a `.cs` file — the script rebuilds the DLL into `./plugins-live` and the
   running server reloads it within ~1s. Delete a DLL to unload its plugin. To build once
   without watching, set `ONESHOT=1`.

Notes:

- In dev mode the live folder is the **only** source of plugins — the example plugins baked
  into the image are not loaded. Build whichever plugins you want into `./plugins-live`
  (you can build several into the same folder; each lands as its own DLL).
- Plugins are platform-agnostic managed assemblies, so they build on Linux and run under the
  Windows .NET runtime inside Proton. `DeadworksManaged.Api` is referenced with
  `<Private>false</Private>`, so the host supplies it at runtime — keep the repo you build
  plugins from in sync with the image (rebuild the image when you change the framework/API).
