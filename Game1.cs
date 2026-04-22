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

    private List<Rectangle> _platforms = new List<Rectangle>();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _platforms.Add(new Rectangle(0, 400, 800, 50));   // нижний пол
        _platforms.Add(new Rectangle(250, 300, 200, 30)); // средняя платформа
        _platforms.Add(new Rectangle(520, 220, 180, 30)); // верхняя платформа

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

        // гравитация
        _playerVelocity.Y += Gravity;

        // движение по X
        _playerPosition.X += _playerVelocity.X;

        // движение по Y
        _playerPosition.Y += _playerVelocity.Y;

        // проверка на полу или нет
        _isOnGround = false;

        var playerBounds = PlayerBounds;

        foreach (var platform in _platforms)
        {
            if (playerBounds.Intersects(platform))
            {
                // проверяем, что падаем сверху на платформу
                if (_playerVelocity.Y >= 0 &&
                    playerBounds.Bottom >= platform.Top &&
                    playerBounds.Bottom - _playerVelocity.Y <= platform.Top + 5)
                {
                    _playerPosition.Y = platform.Top - PlayerHeight;
                    _playerVelocity.Y = 0;
                    _isOnGround = true;

                    playerBounds = PlayerBounds;
                }
            }
        }

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

        _spriteBatch.Draw(_pixel, PlayerBounds, Color.Orange);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}