using EXILION.Network;
using EXILION.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EXILION.Scenes;

public sealed class MultiplayerScene : Scene
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly InputManager input;
    private readonly List<Button> serverButtons = [];
    private readonly List<DiscoveredServer> servers = [];
    private readonly Button refresh;
    private readonly Button back;
    private Task<IReadOnlyList<DiscoveredServer>>? discoveryTask;
    private Task<GameConnection>? connectTask;
    private readonly CancellationTokenSource cancellation = new();
    private SpriteFont? font;
    private SpriteFont? fontBig;
    private string status = "Buscando servidores en la red local...";

    public MultiplayerScene(IGameData gameData) : base(gameData)
    {
        graphicsDevice = gameData.GetGraphicsDevice();
        input = gameData.GetInput();
        Texture2D button = Assets.Sprites.Button;
        SpriteFont buttonFont = Assets.Fonts.PixelArt;
        refresh = new Button("Refresh", new Rectangle(0, 0, GameContext.ScaleX(180), GameContext.ScaleY(50)), button, buttonFont);
        back = new Button("Back", new Rectangle(0, 0, GameContext.ScaleX(180), GameContext.ScaleY(50)), button, buttonFont);
    }

    public override void LoadContent()
    {
        font = Assets.Fonts.PixelArt;
        fontBig = Assets.Fonts.PixelArtBig;
        LayoutButtons();
        StartDiscovery();
    }

    public override void Update(GameTime gameTime)
    {
        if (stopUpdating) return;
        MouseState mouse = input.CurrentMouse;
        CompleteDiscoveryIfReady();
        CompleteConnectionIfReady();
        if (connectTask != null) return;

        if (input.IsKeyPressed(Keys.Escape) || back.isClicked(mouse))
        {
            SceneManager.ChangeScene(new MainMenu(GameData));
            return;
        }
        if (refresh.isClicked(mouse)) StartDiscovery();
        for (int index = 0; index < serverButtons.Count; index++)
        {
            if (!serverButtons[index].isClicked(mouse)) continue;
            status = $"Conectando a {servers[index].WorldName}...";
            connectTask = GameConnection.ConnectAsync(servers[index].Endpoint, Environment.UserName, cancellation.Token);
            return;
        }
    }

    public override void DrawUI(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(fontBig!, "Multiplayer", new Vector2(GameContext.ScaleX(70), GameContext.ScaleY(60)), Color.White);
        spriteBatch.DrawString(font!, status, new Vector2(GameContext.ScaleX(70), GameContext.ScaleY(115)), Color.LightGray);
        refresh.Draw(spriteBatch);
        back.Draw(spriteBatch);
        foreach (Button server in serverButtons) server.Draw(spriteBatch);
    }

    public override void UnloadContent()
    {
        stopUpdating = true;
        cancellation.Cancel();
        if (connectTask is { IsCompletedSuccessfully: true }) connectTask.Result.Dispose();
        cancellation.Dispose();
        serverButtons.Clear();
        servers.Clear();
        font = null;
    }

    private void StartDiscovery()
    {
        if (discoveryTask is { IsCompleted: false }) return;
        serverButtons.Clear();
        servers.Clear();
        status = "Buscando servidores en la red local...";
        discoveryTask = ServerDiscovery.DiscoverAsync(cancellationToken: cancellation.Token);
    }

    private void CompleteDiscoveryIfReady()
    {
        if (discoveryTask is not { IsCompleted: true }) return;
        try
        {
            servers.AddRange(discoveryTask.GetAwaiter().GetResult());
            status = servers.Count == 0 ? "No se encontraron servidores. Revisa el firewall y refresca." : "Elegi un mundo para unirte.";
            LayoutServerButtons();
        }
        catch (Exception exception) { status = $"No se pudo buscar servidores: {exception.Message}"; }
        finally { discoveryTask = null; }
    }

    private void CompleteConnectionIfReady()
    {
        if (connectTask is not { IsCompleted: true }) return;
        try
        {
            GameConnection connection = connectTask.GetAwaiter().GetResult();
            connectTask = null;
            SceneManager.ChangeScene(new GameScene(GameData, connection.Ticket.World, connection));
        }
        catch (Exception exception)
        {
            status = $"No se pudo conectar: {exception.Message}";
            connectTask = null;
        }
    }

    private void LayoutButtons()
    {
        refresh.position = new Vector2(GameContext.ScaleX(70), GameContext.ScaleY(650));
        back.position = new Vector2(graphicsDevice.Viewport.Width - GameContext.ScaleX(250), GameContext.ScaleY(650));
    }

    private void LayoutServerButtons()
    {
        serverButtons.Clear();
        for (int index = 0; index < servers.Count; index++)
        {
            DiscoveredServer server = servers[index];
            serverButtons.Add(new Button($"{server.WorldName} ({server.Endpoint.Address})",
                new Rectangle(GameContext.ScaleX(70), GameContext.ScaleY(170 + index * 65), GameContext.ScaleX(760), GameContext.ScaleY(50)),
                Assets.Sprites.Button, Assets.Fonts.PixelArt));
        }
    }
}
