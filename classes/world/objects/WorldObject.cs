namespace EXILION.World;

public class WorldObject
{
    private const float HitAnimationDuration = 0.25f;
    private const float HitScaleAmount = 0.85f;
    private float _hitAnimationTimer;
    private readonly System.Action<WorldObjectState> _saveState;
    private readonly System.Action<WorldObject> _onDestroyed;

    public WorldObjectId Id { get; }
    public ObjectType Type { get; }
    public float TileX { get; } // Su posición horizontal en el mapa; puede quedar entre dos tiles.
    public float TileY { get; }
    public ObjectDefinition Definition => ObjectProperties.Get(Type);
    public int? CurrentHealth { get; private set; }
    public bool IsDestroyed { get; private set; }
    public float DrawScale
    {
        get
        {
            if (_hitAnimationTimer <= 0f) return 1f;

            float progress = 1f - _hitAnimationTimer / HitAnimationDuration;
            float easedProgress = progress * progress * (3f - 2f * progress);
            return HitScaleAmount + (1f - HitScaleAmount) * easedProgress;
        }
    }

    internal WorldObject(WorldObjectId id, ObjectType type, float tileX, float tileY,
        WorldObjectState savedState, System.Action<WorldObjectState> saveState, System.Action<WorldObject> onDestroyed)
    {
        Id = id;
        Type = type;
        TileX = tileX;
        TileY = tileY;
        _saveState = saveState;
        _onDestroyed = onDestroyed;
        CurrentHealth = savedState?.CurrentHealth ?? Definition.MaxHealth;
        IsDestroyed = savedState?.IsDestroyed ?? false;
    }

    public bool ApplyDamage(int damage, bool allowDrops = true)
    {
        if (damage <= 0 || CurrentHealth == null || IsDestroyed)
            return false;

        SFX.Play(Definition.HitSound);
        _hitAnimationTimer = HitAnimationDuration;
        CurrentHealth = System.Math.Max(0, CurrentHealth.Value - damage);
        if (CurrentHealth == 0)
        {
            IsDestroyed = true;
            if (allowDrops)
                _onDestroyed(this);
        }

        SaveState();
        return true;
    }

    internal void Update(float deltaTime)
    {
        if (_hitAnimationTimer > 0f) _hitAnimationTimer = System.Math.Max(0f, _hitAnimationTimer - deltaTime);
    }

    public bool Destroy()
    {
        if (IsDestroyed)
            return false;

        IsDestroyed = true;
        _onDestroyed(this);
        SaveState();
        return true;
    }

    private void SaveState()
    {
        _saveState(new WorldObjectState(Id, CurrentHealth, IsDestroyed));
    }
}
