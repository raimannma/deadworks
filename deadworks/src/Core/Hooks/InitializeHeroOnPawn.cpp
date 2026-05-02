#include "InitializeHeroOnPawn.hpp"

#include "../Deadworks.hpp"

namespace deadworks {
namespace hooks {

__int64 __fastcall Hook_InitializeHeroOnPawn(void *pPawn, char bWipeItems) {
    auto result = g_InitializeHeroOnPawn.thiscall<__int64>(pPawn, bWipeItems);
    if (pPawn)
        g_Deadworks.OnPost_InitializeHeroOnPawn(pPawn);
    return result;
}

} // namespace hooks
} // namespace deadworks
