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
    private List<Rectangle> _electricZones = new List<Rectangle>();


    #region Электропол
    // электро-пол
    private bool _electricActive = false;
    private float _electricTimer = 0f;

    private const float ElectricOnTime = 2f;
    private const float ElectricOffTime = 4f;
    #endregion

    #region Оформление
    // шрифт
    private SpriteFont _font;
    

    // левелкомплит экран
    private bool _levelCompletedScreen = false;
    private int _completedLevelNumber = 1;
    private float _levelCompleteAlpha = 0f;
    private const float LevelCompleteFadeSpeed = 2f;

    // бэкграунд
    private Texture2D _levelBackground;
    private Texture2D _controlHintTexture;
    private Vector2 _controlHintPosition;
    private bool _showControlHint = false;

    // курсор
    private Texture2D _crosshairNoPortals;
    private Texture2D _crosshairBlueOnly;
    private Texture2D _crosshairOrangeOnly;
    private Texture2D _crosshairBothPortals;

    private Texture2D GetCurrentCrosshair()
    {
        bool hasBlue = _bluePortal != null;
        bool hasOrange = _orangePortal != null;

        if (hasBlue && hasOrange)
            return _crosshairBothPortals;

        if (hasBlue)
            return _crosshairBlueOnly;

        if (hasOrange)
            return _crosshairOrangeOnly;

        return _crosshairNoPortals;
    }


    #endregion

    #region ГлавноеМеню
    // фон меню
    private Texture2D _menuBackground;

    // состояния игры
    private enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing
    }

    private GameState _gameState = GameState.MainMenu;

    // кнопки меню
    private Rectangle _newGameButton;
    private Rectangle _continueButton;
    private Rectangle _exitButton;

    // кнопки выбора уровней
    private List<Rectangle> _levelButtons = new List<Rectangle>();

    // hover
    private bool _hoverNewGame;
    private bool _hoverContinue;
    private bool _hoverExit;

    private void DrawMenuButton(Rectangle rect, string text, bool hovered)
    {
        Color buttonColor = hovered
            ? Color.Orange
            : Color.White;

        if (hovered)
        {
            Rectangle glow = new Rectangle(
                rect.X - 10,
                rect.Y - 10,
                rect.Width + 20,
                rect.Height + 20);

            _spriteBatch.Draw(_pixel, glow, Color.Orange * 0.35f);
        }

        _spriteBatch.Draw(_pixel, rect, buttonColor);

        Vector2 textSize = _font.MeasureString(text);

        _spriteBatch.DrawString(
            _font,
            text,
            new Vector2(
                rect.Center.X - textSize.X / 2,
                rect.Center.Y - textSize.Y / 2),
            Color.Black);
    }
    #endregion

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

    // куб
    private Vector2 _cubePosition = new Vector2(250, 720);
    private Vector2 _cubeVelocity = Vector2.Zero;

    private const int CubeSize = 40;
    private const float CubeHoldDistance = 90f;
    private const float CubeFollowSpeed = 0.25f;

    private bool _isHoldingCube = false;
    private bool _hasCube = false;
    private bool _isCubeTeleporting = false;

    // кнопка

    private Rectangle _button;
    private Rectangle _door;
    private bool _hasButtonDoorLevel = false;
    private bool _doorOpen = false;
    private int _doorPlatformIndex = -1;

    private Rectangle CubeBounds =>
        new Rectangle((int)_cubePosition.X, (int)_cubePosition.Y, CubeSize, CubeSize);

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

    private bool PortalTouchesElectricZone(Rectangle portal)
    {
        foreach (var electricZone in _electricZones)
        {
            if (portal.Intersects(electricZone))
                return true;
        }

        return false;
    }

    private bool PortalTouchesButton(Rectangle portal)
    {
        return _hasButtonDoorLevel && portal.Intersects(_button);
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

                if (_hasButtonDoorLevel &&
                    hitIndex == _doorPlatformIndex)
                {
                        return;
                }

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

                if (PortalTouchesElectricZone(newPortal))
                    return;

                if (PortalTouchesButton(newPortal))
                    return;

                if (isWall)
                {
                    if (newPortal.Top <= platform.Top || newPortal.Bottom >= platform.Bottom)
                        return;
                }
                else
                {
                    if (newPortal.Left <= platform.Left || newPortal.Right >= platform.Right)
                        return;
                }

                if (PortalOverlapsOtherPlatforms(newPortal, platform))
                    return;

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

    #region ТекстурыПорталов
    private Texture2D _bluePortalTexture;
    private Texture2D _orangePortalTexture;
    private Texture2D _portalGlowTexture;

    private void DrawPortal(Portal portal, Texture2D texture, Color glowColor)
    {
        bool horizontal = portal.Bounds.Width > portal.Bounds.Height;

        float rotation = 0f;
        SpriteEffects effects = SpriteEffects.None;

        if (!horizontal && portal.ExitDirection.X > 0)
            effects = SpriteEffects.FlipHorizontally;

        if (horizontal)
        {
            rotation = MathHelper.PiOver2;

            if (portal.ExitDirection.Y > 0)
                effects = SpriteEffects.FlipHorizontally;
        }

        // глоу
        Rectangle glowBounds = portal.Bounds;
        glowBounds.Inflate(3, 2);

        Vector2 glowOrigin = new Vector2(
        _portalGlowTexture.Width / 2f,
        _portalGlowTexture.Height / 2f);

        Vector2 glowScale = horizontal
            ? new Vector2(
                glowBounds.Height / (float)_portalGlowTexture.Width,
                glowBounds.Width / (float)_portalGlowTexture.Height)
            : new Vector2(
                glowBounds.Width / (float)_portalGlowTexture.Width,
                glowBounds.Height / (float)_portalGlowTexture.Height);

        _spriteBatch.Draw(
            _portalGlowTexture,
            new Vector2(glowBounds.Center.X, glowBounds.Center.Y),
            null,
            glowColor * 0.6f,
            rotation,
            glowOrigin,
            glowScale,
            effects,
            0f);

        // основа портала
        Vector2 origin = new Vector2(
        texture.Width / 2f,
        texture.Height / 2f);

        Vector2 scale = horizontal
            ? new Vector2(
                portal.Bounds.Height / (float)texture.Width,
                portal.Bounds.Width / (float)texture.Height)
            : new Vector2(
                portal.Bounds.Width / (float)texture.Width,
                portal.Bounds.Height / (float)texture.Height);

        _spriteBatch.Draw(
            texture,
            new Vector2(portal.Bounds.Center.X, portal.Bounds.Center.Y),
            null,
            glowColor,
            rotation,
            origin,
            scale,
            effects,
            0f);
    }
    #endregion

    #region КубИкнопка
    private void UpdateCube(KeyboardState keyboard, MouseState mouse)
    {
        bool ePressed = keyboard.IsKeyDown(Keys.E) &&
                        !_previousKeyboardState.IsKeyDown(Keys.E);

        Vector2 playerCenter = new Vector2(
            _playerPosition.X + PlayerWidth / 2,
            _playerPosition.Y + PlayerHeight / 2);

        Vector2 cubeCenter = new Vector2(
            _cubePosition.X + CubeSize / 2,
            _cubePosition.Y + CubeSize / 2);

        float distanceToCube = Vector2.Distance(playerCenter, cubeCenter);

        if (ePressed)
        {
            if (_isHoldingCube)
            {
                _isHoldingCube = false;
                _cubeVelocity = Vector2.Zero;
            }
            else if (distanceToCube < 100f)
            {
                _isHoldingCube = true;
                _cubeVelocity = Vector2.Zero;
            }
        }

        if (_isHoldingCube)
        {
            Vector2 mousePosition = new Vector2(mouse.X, mouse.Y);
            Vector2 direction = mousePosition - playerCenter;

            if (direction != Vector2.Zero)
                direction.Normalize();

            Vector2 targetPosition = playerCenter + direction * CubeHoldDistance;
            targetPosition -= new Vector2(CubeSize / 2, CubeSize / 2);

            _cubePosition = Vector2.Lerp(
                _cubePosition,
                targetPosition,
                CubeFollowSpeed);

            return;
        }

        _cubeVelocity.Y += Gravity;

        if (_cubeVelocity.Y > MaxFallSpeed)
            _cubeVelocity.Y = MaxFallSpeed;

        _cubePosition.Y += _cubeVelocity.Y;

        foreach (var platform in _platforms)
        {
            if (CubeBounds.Intersects(platform))
            {
                if (_cubeVelocity.Y > 0)
                {
                    _cubePosition.Y = platform.Top - CubeSize;
                    _cubeVelocity.Y = 0;
                }
            }
        }
    }

    private void UpdateButtonDoor()
    {
        if (!_hasButtonDoorLevel)
            return;

        bool buttonPressed =
            PlayerBounds.Intersects(_button) ||
            (_hasCube && CubeBounds.Intersects(_button));

        _doorOpen = buttonPressed;

        if (_doorPlatformIndex >= 0)
        {
            if (_doorOpen)
                _platforms[_doorPlatformIndex] = Rectangle.Empty;
            else
                _platforms[_doorPlatformIndex] = _door;
        }
    }


    #endregion

    #region ТелепортацияКуба
    private Vector2 GetCubeExitPosition(Portal portal)
    {
        Vector2 center = new Vector2(
            portal.Bounds.Center.X - CubeSize / 2,
            portal.Bounds.Center.Y - CubeSize / 2);

        return center + portal.ExitDirection * 50f;
    }

    private void TeleportCube()
    {
        if (!_hasCube || _isHoldingCube)
            return;

        if (_bluePortal == null || _orangePortal == null)
        {
            _isCubeTeleporting = false;
            return;
        }

        if (!_isCubeTeleporting)
        {
            if (CubeBounds.Intersects(_bluePortal.Bounds))
            {
                _cubePosition = GetCubeExitPosition(_orangePortal);
                _cubeVelocity = _orangePortal.ExitDirection * Math.Max(_cubeVelocity.Length(), 4f);
                _isCubeTeleporting = true;
            }
            else if (CubeBounds.Intersects(_orangePortal.Bounds))
            {
                _cubePosition = GetCubeExitPosition(_bluePortal);
                _cubeVelocity = _bluePortal.ExitDirection * Math.Max(_cubeVelocity.Length(), 4f);
                _isCubeTeleporting = true;
            }
        }

        if (!CubeBounds.Intersects(_bluePortal.Bounds) &&
            !CubeBounds.Intersects(_orangePortal.Bounds))
        {
            _isCubeTeleporting = false;
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
            // если тип перехода другой, сбрасываю запомненную скорость
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
        _electricZones.Clear();
        _electricActive = false;
        _electricTimer = 0f;
        _isHoldingCube = false;
        _cubeVelocity = Vector2.Zero;
        _hasButtonDoorLevel = false;
        _doorOpen = false;
        _doorPlatformIndex = -1;
        _hasCube = false;
        _isCubeTeleporting = false;

        _showControlHint = false;

        _bluePortal = null;
        _orangePortal = null;
        _isTeleporting = false;
        _preserveMomentum = false;
        _sameDirectionPortalSpeed = 0f;

        if (levelNumber == 1)
        {
            _platforms.Clear();

            _controlHintPosition = new Vector2(650, 520);
            _showControlHint = true;

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
            _showControlHint = false;

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
            _showControlHint = false;

            _exit = new Rectangle(1450, 410, 50, 100);

            // внешняя комната
            _platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена
            _platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок

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
        else if (levelNumber == 4)
        {
            _playerPosition = new Vector2(120, 760);
            _playerVelocity = Vector2.Zero;
            _showControlHint = false;

            // выход
            _exit = new Rectangle(1450, 660, 50, 100);

            _cubePosition = new Vector2(250, 720);

            // границы комнаты
            _platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок
            _platforms.Add(new Rectangle(0, 760, 1600, 200));   // нижний пол
            _platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена

            // левый и правый пол на одном уровне
            _platforms.Add(new Rectangle(40, 760, 260, 100));    // левая платформа
            _platforms.Add(new Rectangle(1300, 760, 260, 100));  // правая платформа

            // верхний блок туннеля
            _platforms.Add(new Rectangle(360, 300, 760, 40));  // нижняя часть верхнего блока
            _platforms.Add(new Rectangle(360, 0, 40, 300));  // левая стена верхнего блока
            _platforms.Add(new Rectangle(1080, 0, 40, 300)); // правая стена верхнего блока
            

            _backgroundBlocks.Add(new Rectangle(400, 0, 680, 300));

            // нижний блок туннеля
            _platforms.Add(new Rectangle(360, 640, 760, 40));  // верхняя часть нижнего блока
            _platforms.Add(new Rectangle(360, 640, 40, 220));  // левая стена нижнего блока
            _platforms.Add(new Rectangle(1080, 640, 40, 220)); // правая стена нижнего блока

            _backgroundBlocks.Add(new Rectangle(400, 680, 680, 180));

            // электрические зоны туннеля
            _electricZones.Add(new Rectangle(400, 340, 680, 30)); // электрический потолок
            _electricZones.Add(new Rectangle(400, 610, 680, 30)); // электрический пол

            _platforms.Add(new Rectangle(300, 640, 60, 40));
            _platforms.Add(new Rectangle(1120, 640, 60, 40));
        }
        else if (levelNumber == 5)
        {
            _hasButtonDoorLevel = true;
            _hasCube = true;
            _showControlHint = false;

            _playerPosition = new Vector2(120, 760);
            _playerVelocity = Vector2.Zero;

            _cubePosition = new Vector2(300, 760);
            _cubeVelocity = Vector2.Zero;

            // выход
            _exit = new Rectangle(1450, 400, 50, 100);

            // границы комнаты
            _platforms.Add(new Rectangle(0, 0, 1600, 40));      // потолок
            _platforms.Add(new Rectangle(0, 860, 1600, 40));    // пол
            _platforms.Add(new Rectangle(0, 0, 40, 900));       // левая стена
            _platforms.Add(new Rectangle(1560, 0, 40, 900));    // правая стена

            // нижняя стартовая зона
            _platforms.Add(new Rectangle(40, 800, 480, 60));

            // стенка справа от нижней стартовой зоны
            _platforms.Add(new Rectangle(520, 520, 40, 340));

            // верхняя левая площадка с кнопкой
            _platforms.Add(new Rectangle(40, 300, 520, 60));

            // платформа над кубом
            _platforms.Add(new Rectangle(320, 500, 250, 90));
            _platforms.Add(new Rectangle(360, 180, 200, 120));
            _backgroundBlocks.Add(new Rectangle(380, 200, 140, 100));

            // центральная дверь
            _door = new Rectangle(720, 40, 50, 460);
            _doorPlatformIndex = _platforms.Count;
            _platforms.Add(_door);

            // верхний правый потолочный блок
            _platforms.Add(new Rectangle(720, 40, 840, 60));

            // длинная центральная платформа справа
            _platforms.Add(new Rectangle(560, 500, 1000, 60));
            _platforms.Add(new Rectangle(560, 560, 1000, 300));
            _backgroundBlocks.Add(new Rectangle(600, 600, 920, 260));

            // верхняя правая площадка с выходом
            _platforms.Add(new Rectangle(1320, 500, 240, 60));
            _backgroundBlocks.Add(new Rectangle(1320, 560, 240, 300));

            // кнопка
            _button = new Rectangle(180, 280, 100, 20);
        }
        else if (levelNumber == 6)
        {
            _hasButtonDoorLevel = true;
            _hasCube = false;
            _showControlHint = false;

            _playerPosition = new Vector2(120, 690);
            _playerVelocity = Vector2.Zero;

            _exit = new Rectangle(1400, 200, 50, 100);

            // границы комнаты
            _platforms.Add(new Rectangle(0, 0, 1600, 40));
            _platforms.Add(new Rectangle(0, 860, 1600, 40));
            _platforms.Add(new Rectangle(0, 0, 40, 900));
            _platforms.Add(new Rectangle(1560, 0, 40, 900));

            // большой верхний левый блок
            _platforms.Add(new Rectangle(40, 450, 760, 40));   // низ блока
            _platforms.Add(new Rectangle(760, 40, 40, 450));   // правая стенка блока
            _backgroundBlocks.Add(new Rectangle(40, 40, 760, 410));

            // нижний путь
            _platforms.Add(new Rectangle(40, 730, 430, 140));
            _platforms.Add(new Rectangle(470, 730, 660, 140));

            // дверь
            _door = new Rectangle(470, 450, 50, 280);
            _doorPlatformIndex = _platforms.Count;
            _platforms.Add(_door);

            // кнопка
            _button = new Rectangle(220, 710, 140, 20);

            // большой правый блок с выходом
            _platforms.Add(new Rectangle(1100, 300, 460, 40));  // верх блока
            _platforms.Add(new Rectangle(1100, 300, 40, 560));  // левая стенка
            _platforms.Add(new Rectangle(1560, 300, 40, 560));  // правая стенка
            _backgroundBlocks.Add(new Rectangle(1140, 340, 420, 520));
            _platforms.Add(new Rectangle(1320, 300, 240, 40));
        }

        _playerVelocity = Vector2.Zero;
    }
    #endregion


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

        _newGameButton = new Rectangle(120, 300, 320, 70);
        _continueButton = new Rectangle(120, 400, 320, 70);
        _exitButton = new Rectangle(120, 500, 320, 70);

        for (int i = 0; i < 6; i++)
        {
            _levelButtons.Add(new Rectangle(120 + i * 140, 350, 100, 100));
        }


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _menuBackground = Content.Load<Texture2D>("menu_background");
        _levelBackground = Content.Load<Texture2D>("level_background");
        _controlHintTexture = Content.Load<Texture2D>("controls");

        _crosshairNoPortals = Content.Load<Texture2D>("crosshair_none");
        _crosshairBlueOnly = Content.Load<Texture2D>("crosshair_blue");
        _crosshairOrangeOnly = Content.Load<Texture2D>("crosshair_orange");
        _crosshairBothPortals = Content.Load<Texture2D>("crosshair_both");

        _bluePortalTexture = Content.Load<Texture2D>("blue_portal");
        _orangePortalTexture = Content.Load<Texture2D>("orange_portal");
        _portalGlowTexture = Content.Load<Texture2D>("portal_glow");


        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("DefaultFont");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        IsMouseVisible = _gameState != GameState.Playing;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // главное меню

        if (_gameState == GameState.MainMenu)
        {
            Point mousePoint = mouse.Position;

            _hoverNewGame = _newGameButton.Contains(mousePoint);
            _hoverContinue = _continueButton.Contains(mousePoint);
            _hoverExit = _exitButton.Contains(mousePoint);

            if (mouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_hoverNewGame)
                {
                    _currentLevel = 1;
                    LoadLevel(_currentLevel);
                    _gameState = GameState.Playing;
                }
                else if (_hoverContinue)
                {
                    _gameState = GameState.LevelSelect;
                }
                else if (_hoverExit)
                {
                    Exit();
                }
            }

            _previousMouseState = mouse;
            return;
        }

        // меню выбора уровней

        if (_gameState == GameState.LevelSelect)
        {
            Point mousePoint = mouse.Position;

            if (mouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < _levelButtons.Count; i++)
                {
                    if (_levelButtons[i].Contains(mousePoint))
                    {
                        _currentLevel = i + 1;
                        LoadLevel(_currentLevel);
                        _gameState = GameState.Playing;
                    }
                }
            }

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                _gameState = GameState.MainMenu;
            }

            _previousMouseState = mouse;
            return;
        }

        // финишный экран
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

                if (_currentLevel > 6)
                    _currentLevel = 1;

                LoadLevel(_currentLevel);
                _levelCompletedScreen = false;
            }

            _previousKeyboardState = keyboard;
            _previousMouseState = mouse;
            return;
        }

        // сброс порталов
        if (keyboard.IsKeyDown(Keys.R) &&
    !_previousKeyboardState.IsKeyDown(Keys.R))
        {
            _bluePortal = null;
            _orangePortal = null;
            _isTeleporting = false;
            _preserveMomentum = false;
            _sameDirectionPortalSpeed = 0f;
        }

        // шипы
        UpdateMovingSpikeTrap(deltaTime);

        if (_portalExitTimer > 0)
            _portalExitTimer -= deltaTime;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();


        // таймер электричества
        _electricTimer += deltaTime;

        if (_electricActive)
        {
            if (_electricTimer >= ElectricOnTime)
            {
                _electricTimer = 0f;
                _electricActive = false;
            }
        }
        else
        {
            if (_electricTimer >= ElectricOffTime)
            {
                _electricTimer = 0f;
                _electricActive = true;
            }
        }

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

        if (_hasCube)
        {
            UpdateCube(keyboard, mouse);
            TeleportCube();
        }
        UpdateButtonDoor();


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


        // выход
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

        // смерть от шипов
        foreach (var spike in _spikes)
        {
            if (PlayerBounds.Intersects(spike))
            {
                Die();
                return;
            }
        }

        // смерть от электричества
        if (_electricActive)
        {
            foreach (var electricZone in _electricZones)
            {
                if (PlayerBounds.Intersects(electricZone))
                {
                    Die();
                    return;
                }
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

        _spriteBatch.Draw(
        _levelBackground,
            new Rectangle(0, 0, 1600, 900),
            Color.White);
            
        if (_gameState == GameState.MainMenu)
        {
            _spriteBatch.Draw(
                _menuBackground,
                new Rectangle(0, 0, 1600, 900),
                Color.White);

            DrawMenuButton(_newGameButton, "NEW GAME", _hoverNewGame);
            DrawMenuButton(_continueButton, "CONTINUE", _hoverContinue);
            DrawMenuButton(_exitButton, "EXIT", _hoverExit);

            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        if (_gameState == GameState.LevelSelect)
        {
            GraphicsDevice.Clear(Color.Black);

            for (int i = 0; i < _levelButtons.Count; i++)
            {
                Rectangle button = _levelButtons[i];

                bool hovered = button.Contains(Mouse.GetState().Position);

                Color color = hovered
                    ? Color.Orange
                    : Color.White;

                if (hovered)
                {
                    Rectangle glow = new Rectangle(
                        button.X - 8,
                        button.Y - 8,
                        button.Width + 16,
                        button.Height + 16);

                    _spriteBatch.Draw(_pixel, glow, Color.Orange * 0.35f);
                }

                _spriteBatch.Draw(_pixel, button, color);

                string text = (i + 1).ToString();
                Vector2 size = _font.MeasureString(text);

                _spriteBatch.DrawString(
                    _font,
                    text,
                    new Vector2(
                        button.Center.X - size.X / 2,
                        button.Center.Y - size.Y / 2),
                    Color.Black);
            }

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

        if (_showControlHint)
        {
            float controlsScale = 0.35f;

            _spriteBatch.Draw(
                _controlHintTexture,
                _controlHintPosition = new Vector2(550, 480),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                controlsScale,
                SpriteEffects.None,
                0f);
        }

        if (_hasButtonDoorLevel)
        {
            Color buttonColor = _doorOpen ? Color.LimeGreen : Color.Red;
            _spriteBatch.Draw(_pixel, _button, buttonColor);

            if (!_doorOpen)
                _spriteBatch.Draw(_pixel, _door, Color.DarkSlateGray);
        }

        foreach (var electricZone in _electricZones)
        {
            Color electricColor = _electricActive
                ? Color.Cyan
                : Color.DarkSlateGray;

            _spriteBatch.Draw(_pixel, electricZone, electricColor);
        }

        if (_bluePortal != null)
        {
            DrawPortal(_bluePortal, _bluePortalTexture, Color.Cyan);
        }

        if (_orangePortal != null)
        {
            DrawPortal(_orangePortal, _orangePortalTexture, Color.Orange);
        }

        foreach (var spike in _spikes)
        {
            _spriteBatch.Draw(_pixel, spike, Color.Red);
        }

        _spriteBatch.Draw(_pixel, _exit, Color.Green);

        if (_hasCube)
        {
            Rectangle outerCube = CubeBounds;

            Rectangle innerCube = new Rectangle(
                outerCube.X + 6,
                outerCube.Y + 6,
                outerCube.Width - 12,
                outerCube.Height - 12);

            _spriteBatch.Draw(_pixel, outerCube, Color.DarkSlateGray);
            _spriteBatch.Draw(_pixel, innerCube, Color.Beige);
        }

        _spriteBatch.Draw(_pixel, PlayerBounds, Color.Orange);

        if (_gameState == GameState.Playing)
        {
            Texture2D crosshair = GetCurrentCrosshair();
            MouseState mouse = Mouse.GetState();

            float crosshairScale = 0.2f;

            Vector2 crosshairPosition = new Vector2(
                mouse.X - (crosshair.Width * crosshairScale) / 2f,
                mouse.Y - (crosshair.Height * crosshairScale) / 2f);

            _spriteBatch.Draw(
                crosshair,
                crosshairPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                crosshairScale,
                SpriteEffects.None,
                0f);
        }

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