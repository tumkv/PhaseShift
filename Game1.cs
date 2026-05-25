using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;
using PhaseShift.Controllers;
using PhaseShift.Views;

namespace PhaseShift;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _pixel;

    private GameWorld _world;
    private PlayerController _playerController;
    private GameView _gameView;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.ApplyChanges();

        _world = new GameWorld();
        _playerController = new PlayerController();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _gameView = new GameView(_pixel);

        LoadLevel(1);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        _playerController.Update(_world, keyboard);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin();

        _gameView.DrawWorld(_spriteBatch, _world);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void LoadLevel(int levelNumber)
    {
        _world.Platforms.Clear();
        _world.BackgroundBlocks.Clear();
        _world.Spikes.Clear();

        _world.Player.Position = new Vector2(120, 760);
        _world.Player.Velocity = Vector2.Zero;
        _world.Exit = new Rectangle(1450, 780, 50, 80);

        _world.Platforms.Add(new Rectangle(0, 860, 1600, 40));
        _world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        _world.Platforms.Add(new Rectangle(1560, 0, 40, 900));
        _world.Platforms.Add(new Rectangle(0, 0, 1600, 40));
    }
}