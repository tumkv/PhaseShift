using Microsoft.Xna.Framework;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class CollisionController
{
    public void ResolvePlayerCollisions(GameWorld world)
    {
        var player = world.Player;

        Rectangle oldBounds = player.Bounds;

        // движение по X
        player.Position.X += player.Velocity.X;

        Rectangle playerBounds = player.Bounds;

        foreach (var platform in world.Platforms)
        {
            if (!playerBounds.Intersects(platform))
                continue;

            bool wasAbove = oldBounds.Bottom <= platform.Top;
            bool wasBelow = oldBounds.Top >= platform.Bottom;

            if (!wasAbove && !wasBelow)
            {
                if (player.Velocity.X > 0)
                    player.Position.X = platform.Left - PlayerModel.Width;
                else if (player.Velocity.X < 0)
                    player.Position.X = platform.Right;

                player.Velocity.X = 0;
                player.PreserveMomentum = false;

                playerBounds = player.Bounds;
            }
        }

        // движение по Y
        float remainingY = player.Velocity.Y;
        player.IsOnGround = false;

        while (Math.Abs(remainingY) > 0)
        {
            float stepY = Math.Clamp(remainingY, -2f, 2f);
            remainingY -= stepY;

            player.Position.Y += stepY;
            playerBounds = player.Bounds;

            foreach (var platform in world.Platforms)
            {
                if (!playerBounds.Intersects(platform))
                    continue;

                if (stepY > 0)
                {
                    player.Position.Y = platform.Top - PlayerModel.Height;
                    player.Velocity.Y = 0;
                    player.IsOnGround = true;
                }
                else if (stepY < 0)
                {
                    player.Position.Y = platform.Bottom;
                    player.Velocity.Y = 0;
                }

                player.PreserveMomentum = false;
                remainingY = 0;

                playerBounds = player.Bounds;
                break;
            }
        }
    }
}