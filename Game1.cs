using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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
    private bool _preserveMomentum = false;

    private const float MoveSpeed = 4f;
    private const float Gravity = 0.35f;
    private const float JumpForce = -8f;


    private Rectangle PlayerBounds =>
    new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, PlayerWidth, PlayerHeight);

    #endregion

    #region Порталы
    private Portal _bluePortal = null;
    private Portal _orangePortal = null;

    private const int PortalWidth = 20;
    private const int PortalHeight = 60;

    private const float PortalShootDistance = 1000f;
    private const float PortalRayStep = 4f;

    private MouseState _previousMouseState;
    private bool _isTeleporting = false;

    private float _inputLockTimer = 0f;
    private float _portalExitTimer = 0.08f;
    private float _sameDirectionPortalSpeed = 0f;

    private class Portal
    {
        public Rectangle Bounds;
        public Vector2 ExitDirection;

        public Portal(Rectangle bounds, Vector2 exitDirection)
        {
            Bounds = bounds;
            ExitDirection = exitDirection;
        }
    }

    private void PlacePortal(bool isBlue, Point mousePosition)
    {
        Vector2 playerCenter = new Vector2(
            _playerPosition.X + PlayerWidth / 2,
            _playerPosition.Y + PlayerHeight / 2);

        Vector2 target = new Vector2(mousePosition.X, mousePosition.Y);
        Vector2 direction = target - playerCenter;

        if (direction == Vector2.Zero)
            return;

        direction.Normalize();

        for (float distance = 0; distance < PortalShootDistance; distance += PortalRayStep)
        {
            Vector2 currentPoint = playerCenter + direction * distance;
            Point checkPoint = new Point((int)currentPoint.X, (int)currentPoint.Y);

            foreach (var platform in _platforms)
            {
                if (!platform.Contains(checkPoint))
                    continue;

                bool isWall = platform.Height > platform.Width;
                Rectangle newPortal;
                Vector2 exitDirection;

                if (isWall)
                {
                    int portalX;

                    if (direction.X > 0)
                    {
                        portalX = platform.Left - PortalWidth;
                        exitDirection = new Vector2(-1, 0);
                    }
                    else
                    {
                        portalX = platform.Right;
                        exitDirection = new Vector2(1, 0);
                    }

                    int portalY = checkPoint.Y - PortalHeight / 2;

                    if (portalY < platform.Top)
                        portalY = platform.Top;

                    if (portalY + PortalHeight > platform.Bottom)
                        portalY = platform.Bottom - PortalHeight;

                    newPortal = new Rectangle(portalX, portalY, PortalWidth, PortalHeight);
                }
                else
                {
                    int portalX = checkPoint.X - PortalHeight / 2;

                    if (portalX < platform.Left)
                        portalX = platform.Left;

                    if (portalX + PortalHeight > platform.Right)
                        portalX = platform.Right - PortalHeight;

                    int portalY;

                    if (direction.Y > 0)
                    {
                        portalY = platform.Top - PortalWidth;
                        exitDirection = new Vector2(0, -1);
                    }
                    else
                    {
                        portalY = platform.Bottom;
                        exitDirection = new Vector2(0, 1);
                    }

                    newPortal = new Rectangle(portalX, portalY, PortalHeight, PortalWidth);
                }

                var portal = new Portal(newPortal, exitDirection);

                if (isBlue)
                    _bluePortal = portal;
                else
                    _orangePortal = portal;

                return;
            }
        }
    }
    #endregion

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    #region Логикавыбросапорталов
    private Vector2 GetExitPosition(Portal portal)
    {
        Vector2 center = new Vector2(
            portal.Bounds.Center.X - PlayerWidth / 2,
            portal.Bounds.Center.Y - PlayerHeight / 2);

        return center + portal.ExitDirection * 50f;
    }

    private void ApplyExitVelocity(Portal entryPortal, Portal exitPortal)
    {
        _playerVelocity = RotateMomentum(
            _playerVelocity,
            entryPortal.ExitDirection,
            exitPortal.ExitDirection);

        _preserveMomentum = true;

        _portalExitTimer = 0.15f; // время “свободного вылета”
    }

    private Vector2 RotateMomentum(Vector2 velocity, Vector2 entryDirection, Vector2 exitDirection)
    {
        float speed = velocity.Length();

        if (speed < 1f)
            return Vector2.Zero;

        // Если порталы смотрят в одну сторону, например пол -> пол
        if (entryDirection == exitDirection)
        {
            // первый раз запоминаем скорость входа
            if (_sameDirectionPortalSpeed <= 0f)
                _sameDirectionPortalSpeed = speed;

            // дальше не даём скорости расти
            speed = Math.Min(speed, _sameDirectionPortalSpeed);
        }
        else
        {
            // если тип перехода другой, сбрасываем запомненную скорость
            _sameDirectionPortalSpeed = 0f;
        }

        return exitDirection * speed;
    }
    #endregion

    protected override void Initialize()
    {
        _platforms.Add(new Rectangle(0, 400, 800, 50));   // нижний пол
        _platforms.Add(new Rectangle(250, 330, 200, 30)); // средняя платформа
        _platforms.Add(new Rectangle(520, 250, 180, 30)); // верхняя платформа

        // левая стена
        _platforms.Add(new Rectangle(0, 0, 20, 500));
        // правая стена
        _platforms.Add(new Rectangle(780, 0, 20, 500));
        // потолок
        _platforms.Add(new Rectangle(0, 0, 800, 20)); 

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

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_portalExitTimer > 0)
            _portalExitTimer -= deltaTime;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();

        // движение влево / вправо
        if (!_preserveMomentum)
        {
            _playerVelocity.X = 0;

            if (keyboard.IsKeyDown(Keys.A))
                _playerVelocity.X = -MoveSpeed;

            if (keyboard.IsKeyDown(Keys.D))
                _playerVelocity.X = MoveSpeed;
        }
        else
        {
            // во время инерции можно чуть-чуть управлять в воздухе
            if (keyboard.IsKeyDown(Keys.A))
                _playerVelocity.X -= 0.15f;

            if (keyboard.IsKeyDown(Keys.D))
                _playerVelocity.X += 0.15f;
        }

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

        if (_portalExitTimer <= 0)
        {
            foreach (var platform in _platforms)
            {
                if (playerBounds.Intersects(platform))
                {
                    bool wasAbove = oldBounds.Bottom <= platform.Top;
                    bool wasBelow = oldBounds.Top >= platform.Bottom;

                    if (!wasAbove && !wasBelow)
                    {
                        if (_playerVelocity.X > 0)
                            _playerPosition.X = platform.Left - PlayerWidth;
                        else if (_playerVelocity.X < 0)
                            _playerPosition.X = platform.Right;

                        _playerVelocity.X = 0;
                        _preserveMomentum = false;
                        _sameDirectionPortalSpeed = 0f;

                        playerBounds = PlayerBounds;
                    }
                }
            }
        }

        // движение по Y
        _playerPosition.Y += _playerVelocity.Y;
        playerBounds = PlayerBounds;

        _isOnGround = false;

        if (_portalExitTimer <= 0)
        {
            foreach (var platform in _platforms)
            {
                if (playerBounds.Intersects(platform))
                {
                    bool touchingBluePortal = _bluePortal != null && playerBounds.Intersects(_bluePortal.Bounds);
                    bool touchingOrangePortal = _orangePortal != null && playerBounds.Intersects(_orangePortal.Bounds);

                    // если мы влетели в портал — не гасим скорость платформой
                    if (touchingBluePortal || touchingOrangePortal)
                        continue;

                    if (_playerVelocity.Y > 0)
                    {
                        _playerPosition.Y = platform.Top - PlayerHeight;
                        _playerVelocity.Y = 0;
                        _isOnGround = true;
                        _preserveMomentum = false;
                    }
                    else if (_playerVelocity.Y < 0)
                    {
                        _playerPosition.Y = platform.Bottom;
                        _playerVelocity.Y = 0;
                        _preserveMomentum = false;
                    }

                    playerBounds = PlayerBounds;
                }
            }
        }

        // телепортация
        if (_bluePortal != null && _orangePortal != null)
        {
            if (!_isTeleporting)
            {
                if (PlayerBounds.Intersects(_bluePortal.Bounds))
                {
                    _playerPosition = GetExitPosition(_orangePortal);
                    ApplyExitVelocity(_bluePortal, _orangePortal);
                    _isTeleporting = true;
                }
                else if (PlayerBounds.Intersects(_orangePortal.Bounds))
                {
                    _playerPosition = GetExitPosition(_bluePortal);
                    ApplyExitVelocity(_orangePortal, _bluePortal);
                    _isTeleporting = true;
                }
            }

            if (!PlayerBounds.Intersects(_bluePortal.Bounds) &&
                !PlayerBounds.Intersects(_orangePortal.Bounds))
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

        if (_bluePortal != null)
        {
            _spriteBatch.Draw(_pixel, _bluePortal.Bounds, Color.Blue);
        }

        if (_orangePortal != null)
        {
            _spriteBatch.Draw(_pixel, _orangePortal.Bounds, Color.OrangeRed);
        }

        _spriteBatch.Draw(_pixel, PlayerBounds, Color.Orange);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}