using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhaseShift;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _pixel;
    private List<Rectangle> _platforms = new List<Rectangle>();

    #region Движениеигрока
    private Vector2 _playerPosition = new Vector2(100, 100);
    private Vector2 _playerVelocity = Vector2.Zero;

    private const int PlayerWidth = 40;
    private const int PlayerHeight = 40;

    private bool _isOnGround = false;

    private const float MoveSpeed = 4f;
    private const float Gravity = 0.35f;
    private const float JumpForce = -8f;

    private Rectangle PlayerBounds =>
    new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, PlayerWidth, PlayerHeight);

    #endregion

    #region Порталы
        private Rectangle? _bluePortal = null;
        private Rectangle? _orangePortal = null;

        private const int PortalWidth = 20;
        private const int PortalHeight = 60;
    
        private MouseState _previousMouseState;
        private bool _isTeleporting = false;

    private void PlacePortal(bool isBlue, Point mousePosition)
    {
        Rectangle newPortal = new Rectangle(
            mousePosition.X - PortalWidth / 2,
            mousePosition.Y - PortalHeight / 2,
            PortalWidth,
            PortalHeight);

        if (isBlue)
            _bluePortal = newPortal;
        else
            _orangePortal = newPortal;
    }
    #endregion



    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _platforms.Add(new Rectangle(0, 400, 800, 50));   // нижний пол
        _platforms.Add(new Rectangle(250, 330, 200, 30)); // средняя платформа
        _platforms.Add(new Rectangle(520, 250, 180, 30)); // верхняя платформа

        // левая стена
        _platforms.Add(new Rectangle(0, 0, 20, 500));
        // правая стена
        _platforms.Add(new Rectangle(780, 0, 20, 500));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();

        // движение влево / вправо
        _playerVelocity.X = 0;

        if (keyboard.IsKeyDown(Keys.A))
            _playerVelocity.X = -MoveSpeed;

        if (keyboard.IsKeyDown(Keys.D))
            _playerVelocity.X = MoveSpeed;

        // прыжок
        if (keyboard.IsKeyDown(Keys.Space) && _isOnGround)
        {
            _playerVelocity.Y = JumpForce;
            _isOnGround = false;
        }

        // постановка порталов
        if (mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            PlacePortal(true, mouse.Position);
        }

        if (mouse.RightButton == ButtonState.Pressed &&
            _previousMouseState.RightButton == ButtonState.Released)
        {
            PlacePortal(false, mouse.Position);
        }

        // гравитация
        _playerVelocity.Y += Gravity;

        var oldBounds = PlayerBounds;

        // движение по X
        _playerPosition.X += _playerVelocity.X;
        var playerBounds = PlayerBounds;

        foreach (var platform in _platforms)
        {
            if (playerBounds.Intersects(platform))
            {
                bool wasAbove = oldBounds.Bottom <= platform.Top;
                bool wasBelow = oldBounds.Top >= platform.Bottom;

                // если это не пол и не потолок, значит боковое столкновение
                if (!wasAbove && !wasBelow)
                {
                    if (_playerVelocity.X > 0)
                        _playerPosition.X = platform.Left - PlayerWidth;
                    else if (_playerVelocity.X < 0)
                        _playerPosition.X = platform.Right;

                    playerBounds = PlayerBounds;
                }
            }
        }

        // движение по Y
        _playerPosition.Y += _playerVelocity.Y;
        playerBounds = PlayerBounds;

        // проверка на полу или нет
        _isOnGround = false;

        foreach (var platform in _platforms)
        {
            if (playerBounds.Intersects(platform))
            {
                if (_playerVelocity.Y > 0)
                {
                    _playerPosition.Y = platform.Top - PlayerHeight;
                    _playerVelocity.Y = 0;
                    _isOnGround = true;
                }
                else if (_playerVelocity.Y < 0)
                {
                    _playerPosition.Y = platform.Bottom;
                    _playerVelocity.Y = 0;
                }

                playerBounds = PlayerBounds;
            }
        }

        // телепортация
        if (_bluePortal.HasValue && _orangePortal.HasValue)
        {
            var bluePortal = _bluePortal.Value;
            var orangePortal = _orangePortal.Value;

            if (!_isTeleporting)
            {
                if (PlayerBounds.Intersects(bluePortal))
                {
                    _playerPosition = new Vector2(
                        orangePortal.Center.X - PlayerWidth / 2,
                        orangePortal.Center.Y - PlayerHeight / 2);

                    _isTeleporting = true;
                }
                else if (PlayerBounds.Intersects(orangePortal))
                {
                    _playerPosition = new Vector2(
                        bluePortal.Center.X - PlayerWidth / 2,
                        bluePortal.Center.Y - PlayerHeight / 2);

                    _isTeleporting = true;
                }
            }

            if (!PlayerBounds.Intersects(bluePortal) && !PlayerBounds.Intersects(orangePortal))
            {
                _isTeleporting = false;
            }
        }

        _previousMouseState = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin();

        foreach (var platform in _platforms)
        {
            _spriteBatch.Draw(_pixel, platform, Color.Gray);
        }

        if (_bluePortal.HasValue)
        {
            _spriteBatch.Draw(_pixel, _bluePortal.Value, Color.Blue);
        }

        if (_orangePortal.HasValue)
        {
            _spriteBatch.Draw(_pixel, _orangePortal.Value, Color.OrangeRed);
        }

        _spriteBatch.Draw(_pixel, PlayerBounds, Color.Orange);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}