using Microsoft.Xna.Framework;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class LevelController
{
    public void LoadLevel(GameWorld world, int levelNumber)
    {
        world.Platforms.Clear();
        world.BackgroundBlocks.Clear();
        world.Spikes.Clear();
        world.ElectricZones.Clear();
        world.Projectiles.Clear();

        world.BluePortal = null;
        world.OrangePortal = null;

        world.CurrentLevel = levelNumber;

        world.Player.Velocity = Vector2.Zero;
        world.Player.IsOnGround = false;
        world.Player.PreserveMomentum = false;

        world.Cube.HasCube = false;
        world.Cube.IsHolding = false;
        world.Cube.IsTeleporting = false;
        world.Cube.Velocity = Vector2.Zero;

        world.ButtonDoor.HasButtonDoorLevel = false;
        world.ButtonDoor.DoorOpen = false;
        world.ButtonDoor.ButtonWasPressed = false;
        world.ButtonDoor.DoorPlatformIndex = -1;

        world.MovingSpikeTrap.HasMovingSpikeTrap = false;
        world.MovingSpikeTrap.Direction = 1;

        world.Electricity.Active = false;
        world.Electricity.Timer = 0f;

        world.IsTeleporting = false;
        world.PortalExitTimer = 0f;
        world.SameDirectionPortalSpeed = 0f;

        world.Progress.IsFading = false;
        world.Progress.RestartAfterFade = false;
        world.Progress.FadeAlpha = 0f;
        world.Progress.LevelCompletedScreen = false;
        world.Progress.LevelCompleteAlpha = 0f;

        if (levelNumber == 1)
            LoadLevel1(world);
        else if (levelNumber == 2)
            LoadLevel2(world);
        else if (levelNumber == 3)
            LoadLevel3(world);
        else if (levelNumber == 4)
            LoadLevel4(world);
        else if (levelNumber == 5)
            LoadLevel5(world);
        else if (levelNumber == 6)
            LoadLevel6(world);
        else
            LoadLevel1(world);

        world.Player.Velocity = Vector2.Zero;
    }

    private void LoadLevel1(GameWorld world)
    {
        world.Player.Position = new Vector2(120, 760);
        world.Exit = new Rectangle(1450, 780, 50, 80);

        // границы комнаты
        world.Platforms.Add(new Rectangle(0, 860, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));

        // центральный большой блок
        world.Platforms.Add(new Rectangle(500, 420, 600, 40));
        world.Platforms.Add(new Rectangle(500, 420, 40, 440));
        world.Platforms.Add(new Rectangle(1060, 420, 40, 440));

        int platformWidth = 300;
        int centerX = (1600 - platformWidth) / 2;

        world.BackgroundBlocks.Add(new Rectangle(540, 460, 520, 400));

        world.Platforms.Add(new Rectangle(centerX, 200, platformWidth, 30));
        world.Platforms.Add(new Rectangle(centerX, 230, platformWidth, 30));
    }

    private void LoadLevel2(GameWorld world)
    {
        world.Player.Position = new Vector2(120, 780);
        world.Exit = new Rectangle(1480, 770, 50, 90);

        // внешняя комната
        world.Platforms.Add(new Rectangle(0, 860, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));

        // верхний левый большой блок
        world.Platforms.Add(new Rectangle(40, 40, 800, 40));
        world.Platforms.Add(new Rectangle(40, 40, 40, 240));
        world.Platforms.Add(new Rectangle(800, 40, 40, 240));
        world.Platforms.Add(new Rectangle(40, 240, 800, 40));

        // центральный нижний блок
        world.Platforms.Add(new Rectangle(480, 520, 420, 40));
        world.Platforms.Add(new Rectangle(480, 520, 40, 340));
        world.Platforms.Add(new Rectangle(860, 520, 40, 340));

        // высокий правый блок
        world.Platforms.Add(new Rectangle(1080, 280, 280, 40));
        world.Platforms.Add(new Rectangle(1080, 280, 40, 580));
        world.Platforms.Add(new Rectangle(1320, 280, 40, 580));

        world.BackgroundBlocks.Add(new Rectangle(80, 80, 720, 160));
        world.BackgroundBlocks.Add(new Rectangle(520, 560, 340, 300));
        world.BackgroundBlocks.Add(new Rectangle(1120, 320, 200, 540));
    }

    private void LoadLevel3(GameWorld world)
    {
        world.Player.Position = new Vector2(120, 470);
        world.Exit = new Rectangle(1450, 410, 50, 100);

        // внешняя комната
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));

        world.Platforms.Add(new Rectangle(40, 510, 600, 100));
        world.Platforms.Add(new Rectangle(1160, 510, 400, 100));

        world.BackgroundBlocks.Add(new Rectangle(40, 530, 600, 60));
        world.BackgroundBlocks.Add(new Rectangle(1160, 530, 400, 60));

        AddMovingSpikeTrap(
            world,
            x: 160,
            y: 220,
            width: 460,
            height: 40,
            supportX: 360,
            supportWidth: 40,
            topY: 220,
            bottomY: 420);
    }

    private void LoadLevel4(GameWorld world)
    {
        world.Player.Position = new Vector2(120, 760);
        world.Exit = new Rectangle(1450, 660, 50, 100);

        // границы комнаты
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 760, 1600, 200));
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));

        // левый и правый пол
        world.Platforms.Add(new Rectangle(40, 760, 260, 100));
        world.Platforms.Add(new Rectangle(1300, 760, 260, 100));

        // верхний блок туннеля
        world.Platforms.Add(new Rectangle(360, 300, 760, 40));
        world.Platforms.Add(new Rectangle(360, 0, 40, 300));
        world.Platforms.Add(new Rectangle(1080, 0, 40, 300));

        world.BackgroundBlocks.Add(new Rectangle(400, 0, 680, 300));

        // нижний блок туннеля
        world.Platforms.Add(new Rectangle(360, 640, 760, 40));
        world.Platforms.Add(new Rectangle(360, 640, 40, 220));
        world.Platforms.Add(new Rectangle(1080, 640, 40, 220));

        world.BackgroundBlocks.Add(new Rectangle(400, 680, 680, 180));

        // электрические зоны туннеля
        world.ElectricZones.Add(new Rectangle(400, 340, 680, 30));
        world.ElectricZones.Add(new Rectangle(400, 610, 680, 30));

        world.Platforms.Add(new Rectangle(300, 640, 60, 40));
        world.Platforms.Add(new Rectangle(1120, 640, 60, 40));
    }

    private void LoadLevel5(GameWorld world)
    {
        world.Cube.HasCube = true;
        world.ButtonDoor.HasButtonDoorLevel = true;

        world.Player.Position = new Vector2(120, 760);

        world.Cube.Position = new Vector2(300, 760);
        world.Cube.Velocity = Vector2.Zero;

        world.Exit = new Rectangle(1450, 400, 50, 100);

        // границы комнаты
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 860, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));

        // нижняя стартовая зона
        world.Platforms.Add(new Rectangle(40, 800, 480, 60));

        // стенка справа от нижней стартовой зоны
        world.Platforms.Add(new Rectangle(520, 520, 40, 340));

        // верхняя левая площадка с кнопкой
        world.Platforms.Add(new Rectangle(40, 300, 520, 60));

        // платформа над кубом
        world.Platforms.Add(new Rectangle(320, 500, 250, 90));
        world.Platforms.Add(new Rectangle(360, 180, 200, 120));
        world.BackgroundBlocks.Add(new Rectangle(380, 200, 140, 100));

        // центральная дверь
        world.ButtonDoor.Door = new Rectangle(720, 40, 50, 460);
        world.ButtonDoor.DoorPlatformIndex = world.Platforms.Count;
        world.Platforms.Add(world.ButtonDoor.Door);

        // верхний правый потолочный блок
        world.Platforms.Add(new Rectangle(720, 40, 840, 60));

        // длинная центральная платформа справа
        world.Platforms.Add(new Rectangle(560, 500, 1000, 60));
        world.Platforms.Add(new Rectangle(560, 560, 1000, 300));
        world.BackgroundBlocks.Add(new Rectangle(600, 600, 920, 260));

        // верхняя правая площадка с выходом
        world.Platforms.Add(new Rectangle(1320, 500, 240, 60));
        world.BackgroundBlocks.Add(new Rectangle(1320, 560, 240, 300));

        // кнопка
        world.ButtonDoor.Button = new Rectangle(180, 280, 100, 20);
    }

    private void LoadLevel6(GameWorld world)
    {
        world.ButtonDoor.HasButtonDoorLevel = true;
        world.Cube.HasCube = false;

        world.Player.Position = new Vector2(120, 690);
        world.Exit = new Rectangle(1400, 200, 50, 100);

        // границы комнаты
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 860, 1600, 40));
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));

        // большой верхний левый блок
        world.Platforms.Add(new Rectangle(40, 450, 760, 40));
        world.Platforms.Add(new Rectangle(760, 40, 40, 450));
        world.BackgroundBlocks.Add(new Rectangle(40, 40, 760, 410));

        // нижний путь
        world.Platforms.Add(new Rectangle(40, 730, 430, 140));
        world.Platforms.Add(new Rectangle(470, 730, 660, 140));

        // дверь
        world.ButtonDoor.Door = new Rectangle(470, 450, 50, 280);
        world.ButtonDoor.DoorPlatformIndex = world.Platforms.Count;
        world.Platforms.Add(world.ButtonDoor.Door);

        // кнопка
        world.ButtonDoor.Button = new Rectangle(220, 710, 140, 20);

        // большой правый блок с выходом
        world.Platforms.Add(new Rectangle(1100, 300, 460, 40));
        world.Platforms.Add(new Rectangle(1100, 300, 40, 560));
        world.Platforms.Add(new Rectangle(1560, 300, 40, 560));
        world.BackgroundBlocks.Add(new Rectangle(1140, 340, 420, 520));
        world.Platforms.Add(new Rectangle(1320, 300, 240, 40));
    }

    private void AddMovingSpikeTrap(
        GameWorld world,
        int x,
        int y,
        int width,
        int height,
        int supportX,
        int supportWidth,
        int topY,
        int bottomY)
    {
        var trap = world.MovingSpikeTrap;

        trap.HasMovingSpikeTrap = true;

        trap.X = x;
        trap.Width = width;
        trap.Height = height;

        trap.SupportX = supportX;
        trap.SupportWidth = supportWidth;

        trap.TopY = topY;
        trap.BottomY = bottomY;

        trap.Y = y;
        trap.Direction = 1;

        trap.PlatformIndex = world.Platforms.Count;
        world.Platforms.Add(new Rectangle(x, y, width, height));

        trap.SupportIndex = world.Platforms.Count;
        world.Platforms.Add(new Rectangle(supportX, 40, supportWidth, y - 40));

        trap.SpikeStartIndex = world.Spikes.Count;
        trap.SpikeCount = 0;

        for (int spikeX = x + 20; spikeX < x + width - 20; spikeX += 30)
        {
            world.Spikes.Add(new Rectangle(spikeX, y + height, 20, 40));
            trap.SpikeCount++;
        }
    }
}