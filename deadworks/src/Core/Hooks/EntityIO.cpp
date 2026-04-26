#include "EntityIO.hpp"

#include "../Deadworks.hpp"

namespace deadworks {
namespace hooks {

void __fastcall Hook_CEntityInstance_AcceptInput(CEntityInstance *thisptr, const char *inputName,
                                                  void *activator, void *caller, const char *value) {
    g_Deadworks.OnEntityAcceptInput(thisptr, activator, caller, inputName, value);

    g_CEntityInstance_AcceptInput.thiscall<void>(thisptr, inputName, activator, caller, value);
}

} // namespace hooks
} // namespace deadworks
