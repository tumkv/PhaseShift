using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;
using PhaseShift.Controllers;
using PhaseShift.Views;
using PhaseShift.Managers;

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
    private SoundManager _soundManager;
    private GameController _gameController;

    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

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

        _gameController = new GameController(
            _playerController,
            _collisionController,
            _portalController,
            _levelController);

        _soundManager = new SoundManager();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _soundManager.LoadContent(Content);

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

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _soundManager.MusicVolume = _musicVolume;
        _soundManager.SfxVolume = _sfxVolume;
        _soundManager.UpdateVolumes();

        _gameController.Update(
            _world,
            keyboard,
            mouse,
            deltaTime,
            _soundManager);

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
