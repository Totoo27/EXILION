using EXILION.Items;

namespace EXILION.Logic;

/// <summary>Gameplay state and lifetime for a dropped item.</summary>
public sealed class CatchableItemState
{
    public const float LifetimeSeconds = 30f;
    private float _age;

    public ItemStack Stack { get; }
    public bool Picked { get; private set; }

    public CatchableItemState(ItemStack stack) => Stack = stack;

    public void MarkPicked() => Picked = true;

    public void Update(float deltaTime)
    {
        if (Picked) return;
        _age += deltaTime;
        if (_age >= LifetimeSeconds) MarkPicked();
    }
}
