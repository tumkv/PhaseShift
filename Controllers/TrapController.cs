using Microsoft.Xna.Framework;
using PhaseShift.Managers;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class TrapController
{
    public void Update(GameWorld world, float deltaTime, SoundManager soundManager)
    {
        var trap = world.MovingSpikeTrap;

        if (!trap.HasMovingSpikeTrap)
        {
            soundManager.StopMovingTrap();
            return;
        }

        soundManager.PlayMovingTrap();

        trap.Y += trap.Direction * MovingSpikeTrapModel.Speed * deltaTime;

        if (trap.Y >= trap.BottomY)
        {
            trap.Y = trap.BottomY;
            trap.Direction = -1;
        }

        if (trap.Y <= trap.TopY)
        {
            trap.Y = trap.TopY;
            trap.Direction = 1;
        }

        world.Platforms[trap.PlatformIndex] = new Rectangle(
            trap.X,
            (int)trap.Y,
            trap.Width,
            trap.Height);

        world.Platforms[trap.SupportIndex] = new Rectangle(
            trap.SupportX,
            40,
            trap.SupportWidth,
            (int)trap.Y - 40);

        for (int i = 0; i < trap.SpikeCount; i++)
        {
            int spikeX = trap.X + 20 + i * 30;

            world.Spikes[trap.SpikeStartIndex + i] = new Rectangle(
                spikeX,
                (int)trap.Y + trap.Height,
                20,
                40);
        }
    }
}