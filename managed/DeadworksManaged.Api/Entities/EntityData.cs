namespace DeadworksManaged.Api;

/// <summary>Dictionary-like store that associates arbitrary per-entity data with entities by their handle. Automatically removes entries when an entity is deleted.</summary>
public sealed class EntityData<T> : IEntityData {
	private readonly Dictionary<uint, T> _data = new();

	static EntityData() { }

	public EntityData() {
		EntityDataRegistry.Register(this);
	}

	public T? this[CBaseEntity entity] {
		get => TryGet(entity, out var val) ? val : default;
		set {
			if (value is null)
				_data.Remove(entity.EntityHandle);
			else
				_data[entity.EntityHandle] = value;
		}
	}

	public bool TryGet(CBaseEntity entity, out T value) => _data.TryGetValue(entity.EntityHandle, out value!);

	public T GetOrAdd(CBaseEntity entity, T defaultValue) {
		uint handle = entity.EntityHandle;
		if (!_data.TryGetValue(handle, out var value)) {
			value = defaultValue;
			_data[handle] = value;
		}
		return value;
	}

	public T GetOrAdd(CBaseEntity entity, Func<T> factory) {
		uint handle = entity.EntityHandle;
		if (!_data.TryGetValue(handle, out var value)) {
			value = factory();
			_data[handle] = value;
		}
		return value;
	}

	public bool Has(CBaseEntity entity) => _data.ContainsKey(entity.EntityHandle);

	public void Remove(CBaseEntity entity) => _data.Remove(entity.EntityHandle);

	void IEntityData.Remove(uint handle) => _data.Remove(handle);

	public void Clear() => _data.Clear();
}
