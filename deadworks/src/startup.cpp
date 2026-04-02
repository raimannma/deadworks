#include "Lib/Module.hpp"
#include "Memory/MemoryDataLoader.hpp"
#include "Memory/Scanner.hpp"
#include "Logging/ConsoleLogger.hpp"
#include "Core/Hooks/CoreHooks.hpp"

using namespace std::literals;

using Source2MainFn = int (*)(void *hInstance, void *hPrevInstance, const char *pszCmdLine, int nShowCmd, const char *pszBaseDir, const char *pszGame);

int main(int argc, char **argv) {
    auto log = deadworks::ConsoleLogger{"bootstrap"};

    // Resolve paths from executable location, not cwd
    auto exePath = std::filesystem::path(argv[0]).parent_path();
    if (exePath.empty()) exePath = std::filesystem::current_path();
    else exePath = std::filesystem::absolute(exePath);

    auto serverModule = deadworks::Module((exePath / "../../citadel/bin/win64/server.dll").string());

    auto engineModule = deadworks::Module{"engine2.dll"};
    if (!engineModule.IsValid()) {
        log.Critical("Failed to load engine2");
        return 1;
    }

    auto Source2Main = engineModule.GetSymbol<Source2MainFn>("Source2Main");
    if (!Source2Main) {
        log.Critical("Failed to get Source2Main");
        return 1;
    }

    auto &data = deadworks::MemoryDataLoader::Get();
    auto loadResult = data.Load((exePath / "../../citadel/cfg/deadworks_mem.jsonc").string());
    if (!loadResult.has_value()) {
        log.Critical("Failed to load data: {}", loadResult.error());
        return 1;
    }

    std::array requiredSignatures = {
        "UTIL_Remove",
        "CMaterialSystem2AppSystemDict::OnAppSystemLoaded",
        "CServerSideClientBase::FilterMessage",
        "GetVDataInstanceByName",
        "CModifierProperty::AddModifier"};

    for (const auto &signature : requiredSignatures) {
        if (!data.GetOffset(signature).has_value()) {
            log.Critical("Failed to get signature {}", signature);
            return 1;
        }
    }

    auto onAppSystemLoaded = data.GetOffset("CMaterialSystem2AppSystemDict::OnAppSystemLoaded");
    if (!onAppSystemLoaded.has_value()) {
        log.Critical("Failed to get OnAppSystemLoaded");
        return 1;
    }

    // todo abstract
    deadworks::hooks::g_OnAppSystemLoaded = safetyhook::create_inline(onAppSystemLoaded.value(), deadworks::hooks::Hook_OnAppSystemLoaded);

    constexpr auto DEFAULT_CMD_LINE = "-dedicated -console -dev -insecure -allow_no_lobby_connect +tv_citadel_auto_record 0 +spec_replay_enable 0 +tv_enable 0 +citadel_upload_replay_enabled 0 +hostport 27015 +map dl_midtown"sv;

    std::string cmdLine;
    if (argc > 1) {
        for (int i = 1; i < argc; i++) {
            if (i > 1) cmdLine += ' ';
            cmdLine += argv[i];
        }
    } else {
        cmdLine = DEFAULT_CMD_LINE;
    }

    log.Info("handoff to Source2Main. have fun!!");
    int res = Source2Main(nullptr, nullptr, cmdLine.c_str(), 0, exePath.string().c_str(), "citadel");
    return res;
}
