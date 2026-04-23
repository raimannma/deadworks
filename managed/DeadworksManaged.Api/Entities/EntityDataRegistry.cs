namespace DeadworksManaged.Api;

/// <summary>Global registry of all active <see cref="EntityData{T}"/> stores. Notifies them when an entity is deleted to purge stale entries.</summary>
public static class EntityDataRegistry {
	private static readonly List<WeakReference<IEntityData>> _stores = new();

	internal static void Register(IEntityData store) {
		lock (_stores) _stores.Add(new WeakReference<IEntityData>(store));
	}

	internal static void OnEntityDeleted(uint handle) {
		lock (_stores) {
			for (int i = _stores.Count - 1; i >= 0; i--) {
				if (_stores[i].TryGetTarget(out var store))
					store.Remove(handle);
				else
					_stores.RemoveAt(i);
			}
		}
	}
}
