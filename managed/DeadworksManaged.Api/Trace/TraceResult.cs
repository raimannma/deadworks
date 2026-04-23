using System.Numerics;

namespace DeadworksManaged.Api;

/// <summary>Simplified trace result returned by <see cref="Trace.Ray"/>. Contains the hit position, fraction, and full <see cref="CGameTrace"/> data.</summary>
public struct TraceResult {
	public Vector3 HitPosition;
	public float Fraction;
	public bool DidHit;
	public CGameTrace Trace;
}
