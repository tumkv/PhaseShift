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

    private PlayerSoundController _playerSoundController;
    private MenuView _menuView;
    private GameWorld _world;
    private HintView _hintView;
    private PlayerController _playerController;
    private LevelController _levelController;
    private CollisionController _collisionController;
    private PortalController _portalController;
    private SoundManager _soundManager;
    private GameController _gameController;
    private MenuController _menuController;
    private SettingsController _settingsController;
    private SettingsView _settingsView;
    private CubeController _cubeController;
    private ButtonDoorController _buttonDoorController;
    private TrapController _trapController;
    private ElectricityController _electricityController;
    private LevelProgressController _levelProgressController;
    private CubePortalController _cubePortalController;
    private CrosshairView _crosshairView;
    private LevelProgressView _levelProgressView;

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
        _playerSoundController = new PlayerSoundController();

        _levelController = new LevelController();
        _menuController = new MenuController(_levelController);
        _settingsController = new SettingsController();

        _collisionController = new CollisionController();
        _portalController = new PortalController();

        _cubeController = new CubeController();
        _cubePortalController = new CubePortalController();
        _buttonDoorController = new ButtonDoorController();


        _trapController = new TrapController();
        _electricityController = new ElectricityController();

        _levelProgressController = new LevelProgressController(_levelController);

        _gameController = new GameController(
            _playerController,
            _collisionController,
            _portalController,
            _levelController,
            _menuController,
            _settingsController,
            _cubeController,
            _cubePortalController,
            _buttonDoorController,
            _trapController,
            _electricityController,
            _levelProgressController,
            _playerSoundController);

        _soundManager = new SoundManager();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _menuView = new MenuView();
        _menuView.LoadContent(Content, GraphicsDevice);

        _settingsView = new SettingsView();
        _settingsView.LoadContent(Content, GraphicsDevice);

        _crosshairView = new CrosshairView();
        _crosshairView.LoadContent(Content);

        _levelProgressView = new LevelProgressView();
        _levelProgressView.LoadContent(Content, GraphicsDevice);

        _gameView = new GameView();
        _gameView.LoadContent(Content, GraphicsDevice);

        _hintView = new HintView();
        _hintView.LoadContent(Content, GraphicsDevice);

        _soundManager.LoadContent(Content);

        _levelController.LoadLevel(_world, 1);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        IsMouseVisible = _world.State != GameState.Playing;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _gameController.Update(
            _world,
            keyboard,
            mouse,
            deltaTime,
            _soundManager);

        if (_world.ShouldExitGame)
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin();

        if (_world.Progress.LevelCompletedScreen)
        {
            _levelProgressView.Draw(_spriteBatch, _world);
        }
        else if (_world.State == GameState.Playing)
        {
            _gameView.DrawWorld(_spriteBatch, _world);
            _hintView.Draw(_spriteBatch, _world);
            _crosshairView.Draw(_spriteBatch, _world);
        }
        else if (_world.State == GameState.Paused)
        {
            _gameView.DrawWorld(_spriteBatch, _world);
            _hintView.Draw(_spriteBatch, _world);
            _menuView.Draw(_spriteBatch, _world);
        }
        else if (_world.State == GameState.Settings)
        {
            _settingsView.Draw(_spriteBatch, _world);
        }
        else
        {
            _menuView.Draw(_spriteBatch, _world);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
