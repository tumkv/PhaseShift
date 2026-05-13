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
    private Rectangle _exit;
    private List<Rectangle> _spikes = new List<Rectangle>();
    private List<Rectangle> _noPortalSurfaces = new List<Rectangle>();

    private SpriteFont _font;

    private bool _levelCompletedScreen = false;
    private int _completedLevelNumber = 1;
    private float _levelCompleteAlpha = 0f;
    private const float LevelCompleteFadeSpeed = 2f;

    #region ЛогикаПерсонажа
    private Vector2 _playerPosition = new Vector2(100, 100);
    private Vector2 _playerVelocity = Vector2.Zero;

    private const int PlayerWidth = 40;
    private const int PlayerHeight = 40;

    private bool _isOnGround = false;
    private bool _preserveMomentum = false;

    private const float MoveSpeed = 4f;
    private const float Gravity = 0.35f;
    private const float JumpForce = -8f;
    private const float Acceleration = 0.45f;
    private const float Friction = 0.4f;
    private const float AirControl = 0.15f;
    private const float MaxFallSpeed = 16f;


    // смерть
    private bool _isFading = false;
    private bool _restartAfterFade = false;
    private float _fadeAlpha = 0f;
    private const float FadeSpeed = 2.5f;

    private void Die()
    {
        if (_isFading)
            return;

        _isFading = true;
        _restartAfterFade = true;
        _fadeAlpha = 0f;
    }


    private Rectangle PlayerBounds =>
    new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, PlayerWidth, PlayerHeight);

    private bool IsInsidePortal(Rectangle bounds)
    {
        return (_bluePortal != null && bounds.Intersects(_bluePortal.Bounds)) ||
               (_orangePortal != null && bounds.Intersects(_orangePortal.Bounds));
    }
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
    private KeyboardState _previousKeyboardState;

    private bool PortalOverlapsOtherPlatforms(Rectangle portal, Rectangle targetPlatform)
    {
        foreach (var platform in _platforms)
        {
            if (platform == targetPlatform)
                continue;

            if (portal.Intersects(platform))
                return true;
        }

        return false;
    }
    private bool PointInsideBackgroundBlock(Point point)
    {
        foreach (var block in _backgroundBlocks)
        {
            if (block.Contains(point))
                return true;
        }

        return false;
    }

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

                if (PointInsideBackgroundBlock(checkPoint))
                    return;

                int hitIndex = -1;

                for (int i = 0; i < _platforms.Count; i++)
                {
                    if (_platforms[i].Contains(checkPoint))
                    {
                        hitIndex = i;
                        break;
                    }
                }

                if (hitIndex == -1)
                    continue;

                if (_hasMovingSpikeTrap &&
                    (hitIndex == _movingTrapPlatformIndex || hitIndex == _movingTrapSupportIndex))
                    return;

                Rectangle platform = _platforms[hitIndex];

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

                if (isWall)
                {
                    if (newPortal.Top <= platform.Top || newPortal.Bottom >= platform.Bottom)
                        continue;
                }
                else
                {
                    if (newPortal.Left <= platform.Left || newPortal.Right >= platform.Right)
                        continue;
                }

                if (PortalOverlapsOtherPlatforms(newPortal, platform))
                    continue;

                const int magnetDistance = 5;

                if (isBlue && _orangePortal != null)
                {
                    Rectangle magnetZone = _orangePortal.Bounds;
                    magnetZone.Inflate(magnetDistance, magnetDistance);

                    if (magnetZone.Intersects(newPortal))
                    {
                        newPortal = _orangePortal.Bounds;
                        exitDirection = _orangePortal.ExitDirection;
                        _orangePortal = null;
                    }
                }
                else if (!isBlue && _bluePortal != null)
                {
                    Rectangle magnetZone = _bluePortal.Bounds;
                    magnetZone.Inflate(magnetDistance, magnetDistance);

                    if (magnetZone.Intersects(newPortal))
                    {
                        newPortal = _bluePortal.Bounds;
                        exitDirection = _bluePortal.ExitDirection;
                        _bluePortal = null;
                    }
                }

                var portal = new Portal(newPortal, exitDirection);

                if (isBlue)
                    _bluePortal = portal;
                else
                    _orangePortal = portal;

                return;
        }
    }
    #endregion

    #region ПлатформаСшипами

    private bool _hasMovingSpikeTrap = false;

    private int _movingTrapPlatformIndex;
    private int _movingTrapSupportIndex;
    private int _movingTrapSpikeStartIndex;
    private int _movingTrapSpikeCount;

    private float _movingTrapY;
    private int _movingTrapDirection = 1;

    private int _trapX;
    private int _trapWidth;
    private int _trapHeight;
    private int _trapSupportX;
    private int _trapSupportWidth;
    private int _trapTopY;
    private int _trapBottomY;

    private const float TrapSpeed = 45f;

    private void AddMovingSpikeTrap(
    int x,
    int y,
    int width,
    int height,
    int supportX,
    int supportWidth,
    int topY,
    int bottomY)
    {
        _hasMovingSpikeTrap = true;

        _trapX = x;
        _trapWidth = width;
        _trapHeight = height;
        _trapSupportX = supportX;
        _trapSupportWidth = supportWidth;
        _trapTopY = topY;
        _trapBottomY = bottomY;

        _movingTrapY = y;
        _movingTrapDirection = 1;

        _movingTrapPlatformIndex = _platforms.Count;
        _platforms.Add(new Rectangle(x, y, width, height));

        _movingTrapSupportIndex = _platforms.Count;
        _platforms.Add(new Rectangle(supportX, 40, supportWidth, y - 40));

        _noPortalSurfaces.Add(_platforms[_movingTrapPlatformIndex]);
        _noPortalSurfaces.Add(_platforms[_movingTrapSupportIndex]);

        _movingTrapSpikeStartIndex = _spikes.Count;
        _movingTrapSpikeCount = 0;

        for (int spikeX = x + 20; spikeX < x + width - 20; spikeX += 30)
        {
            _spikes.Add(new Rectangle(spikeX, y + height, 20, 40));
            _movingTrapSpikeCount++;
        }
    }

    private void UpdateMovingSpikeTrap(float deltaTime)
    {
        if (!_hasMovingSpikeTrap)
            return;

        _movingTrapY += _movingTrapDirection * TrapSpeed * deltaTime;

        if (_movingTrapY >= _trapBottomY)
        {
            _movingTrapY = _trapBottomY;
            _movingTrapDirection = -1;
        }

        if (_movingTrapY <= _trapTopY)
        {
            _movingTrapY = _trapTopY;
            _movingTrapDirection = 1;
        }

        _platforms[_movingTrapPlatformIndex] = new Rectangle(
            _trapX,
            (int)_movingTrapY,
            _trapWidth,
            _trapHeight);
            _noPortalSurfaces.Clear();
            _noPortalSurfaces.Add(_platforms[_movingTrapPlatformIndex]);
            _noPortalSurfaces.Add(_platforms[_movingTrapSupportIndex]);

        _platforms[_movingTrapSupportIndex] = new Rectangle(
            _trapSupportX,
            40,
            _trapSupportWidth,
            (int)_movingTrapY - 40);

        for (int i = 0; i < _movingTrapSpikeCount; i++)
        {
            int spikeX = _trapX + 20 + i * 30;

            _spikes[_movingTrapSpikeStartIndex + i] = new Rectangle(
                spikeX,
                (int)_movingTrapY + _trapHeight,
                20,
                40);
        }
    }
    #endregion

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

        _portalExitTimer = 0.15f; // время свободного вылета
    }

    private Vector2 RotateMomentum(Vector2 velocity, Vector2 entryDirection, Vector2 exitDirection)
    {
        float speed = velocity.Length();

        if (speed < 1f)
            return Vector2.Zero;

        // если порталы смотрят в одну сторону
        if (entryDirection == exitDirection)
        {
            // первый раз запоминаем скорость входа
            if (_sameDirectionPortalSpeed <= 0f)
                _sameDirectionPortalSpeed = speed;

            // дальше не даю скорости расти
            speed = Math.Min(speed, _sameDirectionPortalSpeed);
        }
        else
        {
            // если тип перехода другой, сбрасываем запомненную скорость
            _sameDirectionPortalSpeed = 0f;
        }

        if (speed > MaxFallSpeed)
            speed = MaxFallSpeed;

        return exitDirection * speed;
    }
    #endregion

    #region Уровни
    private List<Rectangle> _backgroundBlocks = new List<Rectangle>();
    private int _currentLevel = 1;

    private void LoadLevel(int levelNumber)
    {
        _platforms.Clear();
        _backgroundBlocks.Clear();
        _spikes.Clear();
        _hasMovingSpikeTrap = false;
        _noPortalSurfaces.Clear();

        _bluePortal = null;
        _orangePortal = null;
        _isTeleporting = false;
        _preserveMomentum = false;
        _sameDirectionPortalSpeed = 0f;

        if (levelNumber == 1)
        {
            _platforms.Clear();

            _playerPosition = new Vector2(120, 760);
            _exit = new Rectangle(1450, 780, 50, 80);

            // границы комнаты
            _platforms.Add(new Rectangle(0, 860, 1600, 40));   // пол
            _platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена
            _platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок

            // центральный большой блок
            _platforms.Add(new Rectangle(500, 420, 600, 40));  // верх
            _platforms.Add(new Rectangle(500, 420, 40, 440));  // левая стенка
            _platforms.Add(new Rectangle(1060, 420, 40, 440)); // правая стенка

            // верхняя платформа (по центру)
            int platformWidth = 300;
            int centerX = (1600 - platformWidth) / 2;

            _backgroundBlocks.Add(new Rectangle(540, 460, 520, 400));

            _platforms.Add(new Rectangle(centerX, 200, platformWidth, 30));
            _platforms.Add(new Rectangle(centerX, 230, platformWidth, 30));
        }
        else if (levelNumber == 2)
        {
            _playerPosition = new Vector2(120, 780);
            _playerVelocity = Vector2.Zero;

            _exit = new Rectangle(1480, 770, 50, 90);

            // внешняя комната
            _platforms.Add(new Rectangle(0, 860, 1600, 40));   // пол
            _platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена
            _platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок

            // верхний левый большой блок
            _platforms.Add(new Rectangle(40, 40, 800, 40));    // верх
            _platforms.Add(new Rectangle(40, 40, 40, 240));    // левая стенка
            _platforms.Add(new Rectangle(800, 40, 40, 240));   // правая стенка
            _platforms.Add(new Rectangle(40, 240, 800, 40));   // низ

            // центральный нижний блок
            _platforms.Add(new Rectangle(480, 520, 420, 40));  // верх
            _platforms.Add(new Rectangle(480, 520, 40, 340));  // левая стенка
            _platforms.Add(new Rectangle(860, 520, 40, 340));  // правая стенка

            // высокий правый блок
            _platforms.Add(new Rectangle(1080, 280, 280, 40)); // верх
            _platforms.Add(new Rectangle(1080, 280, 40, 580)); // левая стенка
            _platforms.Add(new Rectangle(1320, 280, 40, 580)); // правая стенка

            _backgroundBlocks.Add(new Rectangle(80, 80, 720, 160));     // верхний левый блок
            _backgroundBlocks.Add(new Rectangle(520, 560, 340, 300));   // центральный блок
            _backgroundBlocks.Add(new Rectangle(1120, 320, 200, 540));  // правый высокий блок


        }
        else if (levelNumber == 3)
        {
            _playerPosition = new Vector2(120, 470);
            _playerVelocity = Vector2.Zero;

            _exit = new Rectangle(1450, 410, 50, 100);

            // внешняя комната
            _platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена
            _platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок

            // ВАЖНО: общего нижнего пола НЕТ, чтобы была настоящая пропасть

            _platforms.Add(new Rectangle(40, 510, 600, 100));
            _platforms.Add(new Rectangle(1160, 510, 400, 100));

            _backgroundBlocks.Add(new Rectangle(40, 530, 600, 60));
            _backgroundBlocks.Add(new Rectangle(1160, 530, 400, 60));

            AddMovingSpikeTrap(
                x: 160,
                y: 220,
                width: 460,
                height: 40,
                supportX: 360,
                supportWidth: 40,
                topY: 220,
                bottomY: 420);
        }

        _playerVelocity = Vector2.Zero;
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
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.ApplyChanges();

        LoadLevel(_currentLevel);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("DefaultFont");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_levelCompletedScreen)
        {
            _levelCompleteAlpha += LevelCompleteFadeSpeed * deltaTime;

            if (_levelCompleteAlpha > 1f)
                _levelCompleteAlpha = 1f;

            if (_levelCompleteAlpha >= 1f &&
                (keyboard.GetPressedKeys().Length > 0 ||
                 mouse.LeftButton == ButtonState.Pressed ||
                 mouse.RightButton == ButtonState.Pressed))
            {
                _currentLevel++;

                if (_currentLevel > 3)
                    _currentLevel = 1;

                LoadLevel(_currentLevel);
                _levelCompletedScreen = false;
            }

            _previousKeyboardState = keyboard;
            _previousMouseState = mouse;
            return;
        }

        if (keyboard.IsKeyDown(Keys.R) &&
    !_previousKeyboardState.IsKeyDown(Keys.R))
        {
            _bluePortal = null;
            _orangePortal = null;
            _isTeleporting = false;
            _preserveMomentum = false;
            _sameDirectionPortalSpeed = 0f;
        }

        UpdateMovingSpikeTrap(deltaTime);

        if (_portalExitTimer > 0)
            _portalExitTimer -= deltaTime;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();

        // затемнение экрана при смерти
        if (_isFading)
        {
            _fadeAlpha += FadeSpeed * deltaTime;

            if (_fadeAlpha >= 1f)
            {
                _fadeAlpha = 1f;

                if (_restartAfterFade)
                {
                    LoadLevel(_currentLevel);
                    _restartAfterFade = false;
                }

                _isFading = false;
            }
        }
        else if (_fadeAlpha > 0f)
        {
            _fadeAlpha -= FadeSpeed * deltaTime;

            if (_fadeAlpha < 0f)
                _fadeAlpha = 0f;
        }

        // движение влево / вправо
        float targetSpeed = 0f;

        if (keyboard.IsKeyDown(Keys.A))
            targetSpeed = -MoveSpeed;

        if (keyboard.IsKeyDown(Keys.D))
            targetSpeed = MoveSpeed;

        if (!_preserveMomentum)
        {
            if (targetSpeed != 0)
            {
                float control = _isOnGround ? Acceleration : AirControl;

                if (_playerVelocity.X < targetSpeed)
                    _playerVelocity.X += control;
                else if (_playerVelocity.X > targetSpeed)
                    _playerVelocity.X -= control;
            }
            else
            {
                if (_isOnGround)
                {
                    if (_playerVelocity.X > 0)
                        _playerVelocity.X -= Friction;
                    else if (_playerVelocity.X < 0)
                        _playerVelocity.X += Friction;

                    if (Math.Abs(_playerVelocity.X) < Friction)
                        _playerVelocity.X = 0;
                }
            }
        }
        else
        {
            // небольшое управление в воздухе во время инерции
            if (keyboard.IsKeyDown(Keys.A))
                _playerVelocity.X -= AirControl;

            if (keyboard.IsKeyDown(Keys.D))
                _playerVelocity.X += AirControl;
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

        if (_playerVelocity.Y > MaxFallSpeed)
            _playerVelocity.Y = MaxFallSpeed;

        if (_playerVelocity.Y < -MaxFallSpeed)
            _playerVelocity.Y = -MaxFallSpeed;

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
        float remainingY = _playerVelocity.Y;
        _isOnGround = false;

        while (Math.Abs(remainingY) > 0)
        {
            float stepY = Math.Clamp(remainingY, -2f, 2f);
            remainingY -= stepY;

            _playerPosition.Y += stepY;
            playerBounds = PlayerBounds;

            foreach (var platform in _platforms)
            {
                if (!playerBounds.Intersects(platform))
                    continue;

                // коллизию пропускаем только если реально внутри портала
                if (_portalExitTimer > 0 && IsInsidePortal(playerBounds))
                    continue;

                if (stepY > 0)
                {
                    _playerPosition.Y = platform.Top - PlayerHeight;
                    _playerVelocity.Y = 0;
                    _isOnGround = true;
                }
                else if (stepY < 0)
                {
                    _playerPosition.Y = platform.Bottom;
                    _playerVelocity.Y = 0;
                }

                _preserveMomentum = false;
                _sameDirectionPortalSpeed = 0f;
                remainingY = 0;
                playerBounds = PlayerBounds;
                break;
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
        else
        {
            _isTeleporting = false;
        }

            _previousMouseState = mouse;
            _previousKeyboardState = keyboard;

        if (PlayerBounds.Intersects(_exit))
        {
            _completedLevelNumber = _currentLevel;
            _levelCompletedScreen = true;
            _levelCompleteAlpha = 0f;

            _bluePortal = null;
            _orangePortal = null;
            _playerVelocity = Vector2.Zero;

            return;
        }

        foreach (var spike in _spikes)
        {
            if (PlayerBounds.Intersects(spike))
            {
                Die();
                return;
            }
        }

        // если упал в пропасть
        if (_playerPosition.Y > 950)
        {
            Die();
            return;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin();

        if (_levelCompletedScreen)
        {
            GraphicsDevice.Clear(Color.Black);

            Color textColor = Color.White * _levelCompleteAlpha;
            Color fadeWhite = Color.White * _levelCompleteAlpha;
            Color fadeBlack = Color.Black * _levelCompleteAlpha;

            _spriteBatch.DrawString(
                _font,
                $"LEVEL {_completedLevelNumber} COMPLETED",
                new Vector2(40, 40),
                textColor);

            string continueText = "Press any key to continue";
            Vector2 textSize = _font.MeasureString(continueText);

            _spriteBatch.DrawString(
                _font,
                continueText,
                new Vector2(
                    (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                    GraphicsDevice.Viewport.Height - 120),
                textColor);

            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        foreach (var block in _backgroundBlocks)
        {
            _spriteBatch.Draw(_pixel, block, Color.DarkGray);
        }

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

        foreach (var spike in _spikes)
        {
            _spriteBatch.Draw(_pixel, spike, Color.Red);
        }

        _spriteBatch.Draw(_pixel, _exit, Color.Green);

        _spriteBatch.Draw(_pixel, PlayerBounds, Color.Orange);

        if (_fadeAlpha > 0f)
        {
            _spriteBatch.Draw(
                _pixel,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Black * _fadeAlpha);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}