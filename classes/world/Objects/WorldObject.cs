namespace EXILION.World;

public class WorldObject
{
    private readonly System.Action<WorldObjectState> _saveState;
    private readonly System.Action<WorldObject> _onDestroyed;

    public WorldObjectId Id { get; }
    public ObjectType Type { get; }
    public float TileX { get; } // Su posición horizontal en el mapa; puede quedar entre dos tiles.
    public float TileY { get; }
    public ObjectDefinition Definition => ObjectProperties.Get(Type);
    public int? CurrentHealth { get; private set; }
    public bool IsDestroyed { get; private set; }
    public bool IsCollected { get; private set; }

    internal WorldObject(WorldObjectId id, ObjectType type, float tileX, float tileY,
        WorldObjectState savedState, System.Action<WorldObjectState> saveState,
        System.Action<WorldObject> onDestroyed = null)
    {
        Id = id;
        Type = type;
        TileX = tileX;
        TileY = tileY;
        _saveState = saveState;
        _onDestroyed = onDestroyed;
        CurrentHealth = savedState?.CurrentHealth ?? Definition.MaxHealth;
        IsDestroyed = savedState?.IsDestroyed ?? false;
        IsCollected = savedState?.IsCollected ?? false;
    }

    public bool ApplyDamage(int damage, bool allowDrops = true)
    {
        if (damage <= 0 || CurrentHealth == null || IsDestroyed || IsCollected)
            return false;

        CurrentHealth = System.Math.Max(0, CurrentHealth.Value - damage);
        if (CurrentHealth == 0)
        {
            IsDestroyed = true;
            if (allowDrops)
                _onDestroyed?.Invoke(this);
        }

        SaveState();
        return true;
    }

    public bool Destroy()
    {
        if (IsDestroyed || IsCollected)
            return false;

        IsDestroyed = true;
        _onDestroyed?.Invoke(this);
        SaveState();
        return true;
    }

    public bool Collect()
    {
        if (!Definition.CanBeCollected || IsDestroyed || IsCollected)
            return false;

        IsCollected = true;
        SaveState();
        return true;
    }

    private void SaveState()
    {
        _saveState(new WorldObjectState(Id, CurrentHealth, IsDestroyed, IsCollected));
    }
}
