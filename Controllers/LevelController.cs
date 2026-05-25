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
    }
}