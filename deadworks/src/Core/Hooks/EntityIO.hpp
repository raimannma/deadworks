#pragma once

#include <safetyhook.hpp>

class CEntityInstance;
class CEntityIdentity;

namespace deadworks {
namespace hooks {

inline safetyhook::InlineHook g_CEntityInstance_AcceptInput;
void __fastcall Hook_CEntityInstance_AcceptInput(CEntityInstance *thisptr, const char *inputName,
                                                  void *activator, void *caller, const char *value);

} // namespace hooks
} // namespace deadworks
