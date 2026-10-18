using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Saving;
using EXILION.Scenes;

namespace EXILION.UI;

public sealed class WorldCreationPanel : IDisposable
{
    #nullable enable
    public const int MaxNameLength = 32;
    public const int MaxSeedLength = 11; // limited by -2147483648"

    private readonly GameContext gameContext;
    private readonly GameWindow window;
    private readonly Texture2D pixel;
    private readonly SpriteFont font;

    private readonly TextInput nameInput;
    private readonly TextInput seedInput;
    private readonly Button confirmButton;
    private readonly Button cancelButton;

    private Rectangle hostBounds;
    private Rectangle bounds;
    private TextInput? focusedInput;
    private MouseState previousMouse;
    private string errorMessage = string.Empty;
    private bool isSubscribed;
    private bool disposed;

    public event Action<string, int>? CreateRequested;
    public event Action? Cancelled;

    public bool IsOpen { get; private set; }
    public bool IsBusy { get; private set; }

    public WorldCreationPanel(GameContext gameContext, GraphicsDevice graphicsDevice, GameWindow window)
    {
        this.gameContext = gameContext;
        this.window = window;

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        font = Assets.Fonts.PixelArt;

        nameInput = new TextInput(MaxNameLength, IsValidNameChar, "Enter a name");
        seedInput = new TextInput(MaxSeedLength, IsValidSeedChar, "Empty = random");

        cancelButton = new Button(
            "Cancel",
            new Rectangle(0, 0, gameContext.ScaleX(180), gameContext.ScaleY(50)),
            Assets.Sprites.Button,
            font
        );

        confirmButton = new Button(
            "Create world",
            new Rectangle(0, 0, gameContext.ScaleX(220), gameContext.ScaleY(50)),
            Assets.Sprites.Button,
            font
        );
    }

    public void Open(Rectangle host)
    {
        if (IsOpen) return;

        hostBounds = host;
        IsOpen = true;
        IsBusy = false;
        errorMessage = string.Empty;

        nameInput.Clear();
        seedInput.Clear();
        previousMouse = Mouse.GetState();

        Layout();
        SetFocus(nameInput);

        if (!isSubscribed)
        {
            window.TextInput += OnTextInput;
            isSubscribed = true;
        }
    }

    public void Close()
    {
        IsOpen = false;
        IsBusy = false;
        SetFocus(null);
        Unsubscribe();
    }

    public void ReportError(string message)
    {
        IsBusy = false;
        errorMessage = message;
    }

    public void Update(MouseState mouse)
    {
        if (!IsOpen) return;

        Layout();

        bool leftClicked = mouse.LeftButton == ButtonState.Pressed
                        && previousMouse.LeftButton == ButtonState.Released;
        previousMouse = mouse;

        if (IsBusy) return;

        if (leftClicked)
        {
            Point mousePosition = mouse.Position;

            if (nameInput.Contains(mousePosition)) SetFocus(nameInput);
            else if (seedInput.Contains(mousePosition)) SetFocus(seedInput);
        }

        if (cancelButton.isClicked(mouse))
        {
            Cancel();
            return;
        }

        if (confirmButton.isClicked(mouse))
        {
            Submit();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;

        // Oscurece el listado de fondo.
        spriteBatch.Draw(pixel, hostBounds, Color.Black * 0.6f);

        // Fondo y borde del diálogo.
        int borderSize = gameContext.ScaleX(4);
        Color borderColor = new Color(46, 15, 74);

        spriteBatch.Draw(pixel, bounds, new Color(10, 6, 16));
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderSize), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - borderSize, bounds.Width, borderSize), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, borderSize, bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - borderSize, bounds.Y, borderSize, bounds.Height), borderColor);

        spriteBatch.DrawString(font, "Create new world",
            new Vector2(bounds.X + gameContext.ScaleX(40), bounds.Y + gameContext.ScaleY(30)),
            Color.White);

        spriteBatch.DrawString(font, "World name",
            new Vector2(nameInput.Bounds.X, nameInput.Bounds.Y - gameContext.ScaleY(28)),
            Color.LightGray);

        spriteBatch.DrawString(font, "Seed",
            new Vector2(seedInput.Bounds.X, seedInput.Bounds.Y - gameContext.ScaleY(28)),
            Color.LightGray);

        nameInput.Draw(spriteBatch, pixel, font);
        seedInput.Draw(spriteBatch, pixel, font);

        string status = IsBusy ? "Creating world..." : errorMessage;
        if (status.Length > 0)
        {
            spriteBatch.DrawString(font, status,
                new Vector2(bounds.X + gameContext.ScaleX(40), bounds.Bottom - gameContext.ScaleY(130)),
                IsBusy ? Color.LightGray : Color.OrangeRed);
        }

        cancelButton.Draw(spriteBatch, 1f);
        confirmButton.Draw(spriteBatch, 1f);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        Close();
        pixel.Dispose();
    }

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!IsOpen || IsBusy) return;

        switch (e.Key)
        {
            case Keys.Escape:
                Cancel();
                return;

            case Keys.Enter:
                Submit();
                return;

            case Keys.Tab:
                SetFocus(focusedInput == nameInput ? seedInput : nameInput);
                return;

            case Keys.Back:
                DeleteCharacter();
                return;
        }

        if (e.Character == '\b')
        {
            DeleteCharacter();
            return;
        }

        if (char.IsControl(e.Character) || focusedInput == null) return;

        if (focusedInput.TryAppend(e.Character))
        {
            errorMessage = string.Empty;
        }
    }

    private void DeleteCharacter()
    {
        if (focusedInput != null && focusedInput.Backspace())
        {
            errorMessage = string.Empty;
        }
    }

    private void Submit()
    {
        if (IsBusy) return;

        if (!TryValidate(out string worldName, out int seed, out string error))
        {
            errorMessage = error;
            return;
        }

        errorMessage = string.Empty;
        IsBusy = true;
        CreateRequested?.Invoke(worldName, seed);
    }

    private void Cancel()
    {
        Close();
        Cancelled?.Invoke();
    }

    private void SetFocus(TextInput? input)
    {
        if (focusedInput != null) focusedInput.IsFocused = false;

        focusedInput = input;

        if (focusedInput != null) focusedInput.IsFocused = true;
    }

    private bool TryValidate(out string worldName, out int seed, out string error)
    {
        worldName = nameInput.Text.Trim();
        seed = 0;
        error = string.Empty;

        if (worldName.Length == 0)
        {
            error = "The world name is required.";
            return false;
        }

        if (worldName.Length > MaxNameLength)
        {
            error = $"The name can't exceed {MaxNameLength} characters.";
            return false;
        }

        string rawSeed = seedInput.Text.Trim();

        if (rawSeed.Length == 0)
        {
            seed = Random.Shared.Next();
            return true;
        }

        if (!int.TryParse(rawSeed, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out seed))
        {
            error = "The seed must be a whole number between -2147483648 and 2147483647.";
            return false;
        }

        return true;
    }

    private void Layout()
    {
        int width = gameContext.ScaleX(640);
        int height = gameContext.ScaleY(420);

        bounds = new Rectangle(
            hostBounds.Center.X - width / 2,
            hostBounds.Center.Y - height / 2,
            width,
            height
        );

        int fieldWidth = gameContext.ScaleX(480);
        int fieldHeight = gameContext.ScaleY(56);
        int fieldX = bounds.Center.X - fieldWidth / 2;

        nameInput.Bounds = new Rectangle(fieldX, bounds.Y + gameContext.ScaleY(100), fieldWidth, fieldHeight);
        seedInput.Bounds = new Rectangle(fieldX, bounds.Y + gameContext.ScaleY(200), fieldWidth, fieldHeight);

        int buttonY = bounds.Bottom - gameContext.ScaleY(90);
        cancelButton.position = new Vector2(bounds.X + gameContext.ScaleX(40), buttonY);
        confirmButton.position = new Vector2(bounds.Right - gameContext.ScaleX(260), buttonY);
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;

        window.TextInput -= OnTextInput;
        isSubscribed = false;
    }

    private static bool IsValidNameChar(string current, char c)
        => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_';

    private static bool IsValidSeedChar(string current, char c)
        => (c >= '0' && c <= '9') || (c == '-' && current.Length == 0);
}
