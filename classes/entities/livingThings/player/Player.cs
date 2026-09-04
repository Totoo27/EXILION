using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Items;
using EXILION.Entities.CatchableItems;

namespace EXILION.Entities.LivingThings;


public class Player : LivingThing
{

    public int maxHunger { get; private set; } = 100;
    public int hunger { get; private set; }
    private Inventory inventory;
    public Inventory Inventory => inventory;

    private float hungerTimer = 0f;

    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, (float) gameContext.ScaleXY(3), gameContext)
    {
        this.hunger = maxHunger;
        this.inventory = new Inventory(); 
    }

    public async void Update(Vector2 mousePosition, InputManager input, GameTime gameTime)
    {

        updateHunger(gameTime);

        float currentSpeed = this.speed;

        if(input.IsKeyHeld(Keys.LeftShift))
        {
            currentSpeed *= 2;
        }

        if (input.IsKeyHeld(Keys.Left))
        {
            this.position.X -= currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Right))
        {
            this.position.X += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Down))
        {
            this.position.Y += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Up))
        {
            this.position.Y -= currentSpeed;
        }

        if (input.IsKeyPressed(Keys.H))
        {
            gameContext.showHitboxes = !gameContext.showHitboxes;
        }

        base.Update(mousePosition);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        base.Draw(spriteBatch, pixel);
    }

    public void updateHunger(GameTime gameTime)
    {

        hungerTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (hungerTimer >= 1f)
        {
            hungerTimer -= 1f;
            hunger--;

            Console.WriteLine($"Hunger: {hunger}");

            if (hunger <= 0)
            {
                takeDamage(1);
            }
        }

    }

    public bool TryPickup(CatchableItem item)
{
    if (item.Picked) return false;

    int quantityBefore = item.Stack.Quantity;
    int leftover = inventory.AddItem(item.Stack.Item, quantityBefore);
    int pickedAmount = quantityBefore - leftover;

    if (pickedAmount <= 0) return false;

    item.Stack.Remove(pickedAmount);

    if (item.Stack.Quantity == 0)
    {
        item.MarkPicked();
    }

    return true;
}



}