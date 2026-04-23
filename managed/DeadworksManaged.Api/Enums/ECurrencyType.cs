namespace DeadworksManaged.Api;

/// <summary>Types of in-game currency (gold, ability points, etc.).</summary>
public enum ECurrencyType : uint {
	ECurrencyInvalid = 0xffffffff,
	EGold = 0x0,
	EAbilityPoints = 0x1,
	EAbilityUnlocks = 0x2,
	EDeathPenaltyGold = 0x3,
	EItemDraftRerolls = 0x4,
	EItemEnhancements = 0x5,
	ECurrencyCount = 0x6,
}
