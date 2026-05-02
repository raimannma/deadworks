#pragma once

#include <safetyhook.hpp>

namespace deadworks {
namespace hooks {

inline safetyhook::InlineHook g_InitializeHeroOnPawn;
__int64 __fastcall Hook_InitializeHeroOnPawn(void *pPawn, char bWipeItems);

} // namespace hooks
} // namespace deadworks
