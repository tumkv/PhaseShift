using Microsoft.Xna.Framework;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class LevelController
{

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

    public void LoadLevel(GameWorld world, int levelNumber)
    {
        world.Platforms.Clear();
        world.BackgroundBlocks.Clear();
        world.Spikes.Clear();
        world.ElectricZones.Clear();
        world.Projectiles.Clear();

        world.BluePortal = null;
        world.OrangePortal = null;
        world.Projectiles.Clear();

        world.IsTeleporting = false;
        world.PortalExitTimer = 0f;
        world.SameDirectionPortalSpeed = 0f;


        world.CurrentLevel = levelNumber;

        if (levelNumber == 1)
        {
            LoadLevel1(world);
        }
    }

    private void LoadLevel1(GameWorld world)
    {
        world.Player.Position = new Vector2(120, 760);
        world.Player.Velocity = Vector2.Zero;
        world.Player.IsOnGround = false;
        world.Player.PreserveMomentum = false;

        world.Cube.HasCube = false;
        world.Cube.IsHolding = false;
        world.Cube.IsTeleporting = false;
        world.Cube.Velocity = Vector2.Zero;

        world.MovingSpikeTrap.HasMovingSpikeTrap = false;
        world.MovingSpikeTrap.Direction = 1;

        world.Electricity.Active = false;
        world.Electricity.Timer = 0f;

        world.ElectricZones.Clear();
        world.Spikes.Clear();

        world.ButtonDoor.HasButtonDoorLevel = false;
        world.ButtonDoor.DoorOpen = false;
        world.ButtonDoor.ButtonWasPressed = false;
        world.ButtonDoor.DoorPlatformIndex = -1;

        world.Exit = new Rectangle(1450, 780, 50, 80);

        // границы комнаты
        world.Platforms.Add(new Rectangle(0, 860, 1600, 40));   // пол
        world.Platforms.Add(new Rectangle(0, 0, 40, 900));      // левая стена
        world.Platforms.Add(new Rectangle(1560, 0, 40, 900));   // правая стена
        world.Platforms.Add(new Rectangle(0, 0, 1600, 40));     // потолок

        // центральный большой блок
        world.Platforms.Add(new Rectangle(500, 420, 600, 40));
        world.Platforms.Add(new Rectangle(500, 420, 40, 440));
        world.Platforms.Add(new Rectangle(1060, 420, 40, 440));

        // фон внутри блока
        world.BackgroundBlocks.Add(new Rectangle(540, 460, 520, 400));

        // верхняя платформа
        int platformWidth = 300;
        int centerX = (1600 - platformWidth) / 2;

        world.Platforms.Add(new Rectangle(centerX, 200, platformWidth, 30));
        world.Platforms.Add(new Rectangle(centerX, 230, platformWidth, 30));

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
}