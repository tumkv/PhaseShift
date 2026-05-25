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
    private LevelController _levelController;
    private CollisionController _collisionController;
    private PortalController _portalController;

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
        _levelController = new LevelController();
        _collisionController = new CollisionController();
        _portalController = new PortalController();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _gameView = new GameView(_pixel);

        _levelController.LoadLevel(_world, 1);
    }

    protected override void Update(GameTime gameTime)
{
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        _playerController.Update(_world, keyboard);
        _collisionController.ResolvePlayerCollisions(_world);
        _portalController.Update(_world, mouse);

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
}
