using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Entities.LivingThings;

public class Hand
{
    private enum HandState
    {
        Idle,
        Extending,
        Retracting
    }

    private const float SideDistance = 35f;
    private const float ForwardDistance = 20f;

    private const float AttackDistance = 45f;
    private const float AttackDuration = 0.12f;

    private int HitboxSize;

    private HandState state = HandState.Idle;

    private float attackTimer = 0f;

    private Vector2 ownerPosition;
    private Vector2 direction = Vector2.UnitX;

    public Vector2 Position { get; private set; }
    private Sprite sprite;

    public float Angle { get; private set; }

    public bool IsAttacking => state != HandState.Idle;

    public bool CanAttack => state == HandState.Idle;

    public Rectangle Hitbox
    {
        get
        {
            return new Rectangle(
                (int)(Position.X - HitboxSize / 2f),
                (int)(Position.Y - HitboxSize / 2f),
                HitboxSize,
                HitboxSize
            );
        }
    }

    public Hand(Texture2D texture, int hitboxSize)
    {
        sprite = new Sprite(texture, 0.8f);
        HitboxSize = hitboxSize;
    }

    public void Update(Vector2 ownerPosition, Vector2 direction, bool isLeft, GameTime gameTime)
    {
        this.ownerPosition = ownerPosition;

        if (direction != Vector2.Zero)
        {
            this.direction = Vector2.Normalize(direction);
        }

        Angle = MathF.Atan2(this.direction.Y, this.direction.X);
        sprite.Update(Angle, Position);


        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateAttack(deltaTime);
        UpdatePosition(isLeft);
    }

    public bool Attack()
    {
        if (!CanAttack) return false;

        state = HandState.Extending;
        attackTimer = 0f;

        return true;
    }

    private void UpdateAttack(float deltaTime)
    {
        if (state == HandState.Idle) return;

        attackTimer += deltaTime;

        if (attackTimer >= AttackDuration)
        {
            attackTimer = 0f;

            if (state == HandState.Extending)
            {
                state = HandState.Retracting;
            }

            else if (state == HandState.Retracting)
            {
                state = HandState.Idle;
            }
        }
    }

    private void UpdatePosition(bool isLeft)
    {
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        float sideOffset = isLeft ? -SideDistance : SideDistance;

        Vector2 basePosition = ownerPosition + direction * ForwardDistance + perpendicular * sideOffset;

        Vector2 attackOffset = Vector2.Zero;

        if (state == HandState.Extending)
        {
            float progress = attackTimer / AttackDuration;

            float easedProgress = EaseOut(progress);

            attackOffset = direction * AttackDistance * easedProgress;
        }
        else if (state == HandState.Retracting)
        {
            float progress = attackTimer / AttackDuration;

            float easedProgress = EaseOut(progress);

            attackOffset = direction * AttackDistance * (1f - easedProgress);
        }

        Position = basePosition + attackOffset;
    }

    private float EaseOut(float value)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        return 1f - MathF.Pow(1f - value, 3f);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        sprite.Draw(spriteBatch, Color.White);
    }

}