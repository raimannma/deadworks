namespace DeadworksManaged.Api;

/// <summary>Internal interface for entity-keyed data stores, allowing cleanup on entity deletion.</summary>
public interface IEntityData {
	void Remove(uint handle);
}
