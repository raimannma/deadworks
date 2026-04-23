namespace DeadworksManaged.Api;

/// <summary>Individual content/interaction layers used to build <see cref="MaskTrace"/> bitmasks for trace queries.</summary>
public enum InteractionLayer : sbyte {
	// Base engine layers (0-30)
	ContentsSolid = 0,
	ContentsHitbox,
	ContentsTrigger,
	ContentsSky,
	FirstUser,
	ContentsPlayerClip = FirstUser,
	ContentsNpcClip,
	ContentsBlockLos,
	ContentsBlockLight,
	ContentsLadder,
	ContentsPickup,
	ContentsBlockSound,
	ContentsNoDraw,
	ContentsWindow,
	ContentsPassBullets,
	ContentsWorldGeometry,
	ContentsWater,
	ContentsSlime,
	ContentsTouchAll,
	ContentsPlayer,
	ContentsNpc,
	ContentsDebris,
	ContentsPhysicsProp,
	ContentsNavIgnore,
	ContentsNavLocalIgnore,
	ContentsPostProcessingVolume,
	ContentsUnusedLayer3,
	ContentsCarriedObject,
	ContentsPushaway,
	ContentsServerEntityOnClient,
	ContentsCarriedWeapon,
	ContentsStaticLevel,
	// Deadlock layers (31-63)
	FirstModSpecific,
	CitadelTeamAmber = FirstModSpecific,  // 31
	CitadelTeamSapphire,                  // 32
	CitadelTeamNeutal,                    // 33
	CitadelAbility,                       // 34
	CitadelBullet,                        // 35
	CitadelProjectile,                    // 36
	CitadelUnitHero,                      // 37
	CitadelUnitTrooper,                   // 38
	CitadelUnitNeutral,                   // 39
	CitadelUnitBuilding,                  // 40
	CitadelUnitProp,                      // 41
	CitadelUnitMinion,                    // 42
	CitadelUnitBoss,                      // 43
	CitadelUnitGoldOrb,                   // 44
	CitadelUnitWorldProp,                 // 45
	CitadelUnitTrophy,                    // 46
	CitadelUnitZipline,                   // 47
	CitadelMantleHidden,                  // 48
	CitadelObscured,                      // 49
	CitadelTimeWarp,                      // 50
	CitadelFoliage,                       // 51
	CitadelTransparent,                   // 52
	CitadelBlockCamera,                   // 53
	CitadelMantleable,                    // 54
	CitadelWalkable,                      // 55
	CitadelTempMovementBlocker,           // 56
	CitadelBlockMantle,                   // 57
	CitadelSkyclip,                       // 58
	CitadelValidPingTarget,               // 59
	CitadelCameraCanPassThrough,          // 60
	CitadelAbilityTrigger,                // 61
	CitadelPortalTrigger,                 // 62
	CitadelPortalEnvironment,             // 63
	NotFound = -1,
}
