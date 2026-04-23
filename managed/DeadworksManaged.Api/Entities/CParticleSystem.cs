using System.Drawing;
using System.Numerics;
using System.Text;

namespace DeadworksManaged.Api;

/// <summary>Represents a live <c>info_particle_system</c> entity. Obtain instances via <see cref="Builder.Spawn"/>.</summary>
[NativeClass("CParticleSystem")]
public sealed unsafe class CParticleSystem : CBaseEntity {
	internal CParticleSystem(nint handle) : base(handle) { }

	// Schema accessors (lazy-resolved)
	private static readonly SchemaAccessor<uint> _clrTint = new("CParticleSystem"u8, "m_clrTint"u8);
	private static readonly SchemaAccessor<int> _nTintCP = new("CParticleSystem"u8, "m_nTintCP"u8);
	private static readonly SchemaAccessor<int> _nDataCP = new("CParticleSystem"u8, "m_nDataCP"u8);
	private static readonly SchemaAccessor<Vector3> _vecDataCPValue = new("CParticleSystem"u8, "m_vecDataCPValue"u8);
	private static readonly SchemaArrayAccessor<uint> _hControlPointEnts = new("CParticleSystem"u8, "m_hControlPointEnts"u8);

	/// <summary>Stops the particle effect without destroying the entity.</summary>
	public void Stop() => AcceptInput("Stop");

	/// <summary>Starts or restarts the particle effect.</summary>
	public void Start() => AcceptInput("Start");

	/// <summary>Removes this particle system entity from the world.</summary>
	public void Destroy() => Remove();

	/// <summary>Attaches this particle system to <paramref name="parent"/>, inheriting its transform.</summary>
	/// <param name="parent">The entity to attach to.</param>
	public void AttachTo(CBaseEntity parent) => SetParent(parent);

	/// <summary>Detaches this particle system from its current parent.</summary>
	public void Detach() => ClearParent();

	/// <summary>Begins building a new particle system entity.</summary>
	/// <param name="effectName">The resource path of the particle effect (e.g. <c>particles/foo.vpcf</c>).</param>
	public static Builder Create(string effectName) => new(effectName);

	/// <summary>Fluent builder for spawning a <see cref="CParticleSystem"/> entity.</summary>
	public sealed class Builder {
		private readonly string _effectName;
		private Vector3? _position;
		private Vector3? _angles;
		private bool _startActive = true;
		private Color? _tintColor;
		private int _tintCP;
		private int? _dataCP;
		private Vector3? _dataCPValue;
		private readonly Dictionary<int, CBaseEntity> _controlPoints = new();
		private CBaseEntity? _parent;

		internal Builder(string effectName) => _effectName = effectName;

		/// <summary>Sets the world-space spawn position.</summary>
		public Builder AtPosition(Vector3 position) { _position = position; return this; }

		/// <summary>Sets the world-space spawn position from individual components.</summary>
		public Builder AtPosition(float x, float y, float z) { _position = new Vector3(x, y, z); return this; }

		/// <summary>Sets the world-space spawn angles (pitch, yaw, roll in degrees).</summary>
		public Builder WithAngles(Vector3 angles) { _angles = angles; return this; }

		/// <summary>Controls whether the effect starts playing immediately on spawn. Defaults to <see langword="true"/>.</summary>
		public Builder StartActive(bool active) { _startActive = active; return this; }

		/// <summary>Applies an RGBA tint color to the effect via the specified control point.</summary>
		/// <param name="color">The tint color.</param>
		/// <param name="tintCP">The control point index to receive the tint (default 0).</param>
		public Builder WithTint(Color color, int tintCP = 0) {
			_tintColor = color;
			_tintCP = tintCP;
			return this;
		}

		/// <summary>Sets a data control point value on the effect.</summary>
		/// <param name="cp">The control point index.</param>
		/// <param name="value">The 3D value to assign to the control point.</param>
		public Builder WithDataCP(int cp, Vector3 value) {
			_dataCP = cp;
			_dataCPValue = value;
			return this;
		}

		/// <summary>Binds an entity to a numbered control point so the effect can follow it.</summary>
		/// <param name="index">The control point index.</param>
		/// <param name="entity">The entity to bind.</param>
		public Builder WithControlPoint(int index, CBaseEntity entity) {
			_controlPoints[index] = entity;
			return this;
		}

		/// <summary>Parents the spawned particle system to another entity (applied after spawn).</summary>
		/// <param name="parent">The entity to parent to.</param>
		public Builder AttachedTo(CBaseEntity parent) { _parent = parent; return this; }

		/// <summary>
		/// Creates the info_particle_system entity via CEntityKeyValues so the
		/// engine properly resolves the particle resource handle (m_iEffectIndex).
		/// </summary>
		/// <returns>The spawned <see cref="CParticleSystem"/>, or <see langword="null"/> if entity creation failed.</returns>
		public CParticleSystem? Spawn() {
			// 1. Create entity
			var baseEntity = CBaseEntity.CreateByName("info_particle_system");
			if (baseEntity == null) return null;

			var particle = new CParticleSystem(baseEntity.Handle);

			// 2. Build CEntityKeyValues - effect_name MUST go through KV so the
			//    engine resolves it to m_iEffectIndex (resource strong handle).
			//    Raw schema writes to m_iszEffectName bypass resource resolution.
			var ekv = new CEntityKeyValues();
			ekv.SetString("effect_name", _effectName);
			ekv.SetBool("start_active", _startActive);

			// 3. Set optional schema fields (simple value types, no resource resolution needed)
			if (_tintColor.HasValue) {
				var c = _tintColor.Value;
				uint rgba = (uint)(c.R | (c.G << 8) | (c.B << 16) | (c.A << 24));
				_clrTint.Set(particle.Handle, rgba);
				_nTintCP.Set(particle.Handle, _tintCP);
			}

			if (_dataCP.HasValue) {
				_nDataCP.Set(particle.Handle, _dataCP.Value);
				if (_dataCPValue.HasValue)
					_vecDataCPValue.Set(particle.Handle, _dataCPValue.Value);
			}

			foreach (var (index, entity) in _controlPoints) {
				_hControlPointEnts.Set(particle.Handle, index, entity.EntityHandle);
			}

			// 4. Teleport
			if (_position.HasValue || _angles.HasValue)
				particle.Teleport(_position, _angles);

			// 5. DispatchSpawn with keyvalues - engine resolves effect_name → m_iEffectIndex
			particle.Spawn(ekv);

			// 6. Explicitly fire Start input to ensure activation
			if (_startActive)
				particle.Start();

			// 7. SetParent (must be after spawn)
			if (_parent != null)
				particle.SetParent(_parent);

			return particle;
		}
	}
}
