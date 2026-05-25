using Microsoft.Xna.Framework;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class CollisionController
{

    private bool IsInsidePortal(GameWorld world, Rectangle bounds)
    {
        return (world.BluePortal != null && bounds.Intersects(world.BluePortal.Bounds)) ||
               (world.OrangePortal != null && bounds.Intersects(world.OrangePortal.Bounds));
    }

    public void ResolvePlayerCollisions(GameWorld world)
    {
        var player = world.Player;

        Rectangle oldBounds = player.Bounds;

        // движение по X
        player.Position.X += player.Velocity.X;

        Rectangle playerBounds = player.Bounds;

        if (world.PortalExitTimer <= 0f)
        {
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
                    world.SameDirectionPortalSpeed = 0f;

                    playerBounds = player.Bounds;
                }
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

                if (world.PortalExitTimer > 0f && IsInsidePortal(world, playerBounds))
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