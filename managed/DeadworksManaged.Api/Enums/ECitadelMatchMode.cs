namespace DeadworksManaged.Api;

public enum ECitadelMatchMode : uint {
	Invalid = 0x0,
	Unranked = 0x1,
	PrivateLobby = 0x2,
	CoopBot = 0x3,
	Ranked = 0x4,
	ServerTest = 0x5,
	Tutorial = 0x6,
	HeroLabs = 0x7,
	Calibration = 0x8,
}
