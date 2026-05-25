using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class PortalController
{
    private const float ProjectileSpeed = 70f;
    private const float PortalRayStep = 4f;

    private MouseState _previousMouseState;

    public void Update(GameWorld world, MouseState mouse)
    {
        if (mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            ShootPortalProjectile(world, mouse.Position, true);
        }

        if (mouse.RightButton == ButtonState.Pressed &&
            _previousMouseState.RightButton == ButtonState.Released)
        {
            ShootPortalProjectile(world, mouse.Position, false);
        }

        UpdateProjectiles(world);

        _previousMouseState = mouse;
    }

    private void ShootPortalProjectile(GameWorld world, Point mousePosition, bool isBlue)
    {
        Vector2 playerCenter = new Vector2(
            world.Player.Position.X + PlayerModel.Width / 2,
            world.Player.Position.Y + PlayerModel.Height / 2);

        Vector2 target = new Vector2(mousePosition.X, mousePosition.Y);
        Vector2 direction = target - playerCenter;

        if (direction == Vector2.Zero)
            return;

        direction.Normalize();

        world.Projectiles.Add(new PortalProjectileModel(
            playerCenter,
            direction,
            isBlue));
    }

    private void UpdateProjectiles(GameWorld world)
    {
        for (int i = world.Projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = world.Projectiles[i];

            projectile.Lifetime -= 1f / 60f;

            if (projectile.Lifetime < 0f)
                projectile.Lifetime = 0f;

            bool shouldRemove = false;
            float remainingDistance = ProjectileSpeed;

            while (remainingDistance > 0)
            {
                float step = Math.Min(remainingDistance, PortalRayStep);
                remainingDistance -= step;

                projectile.Position += projectile.Direction * step;

                Point point = projectile.Position.ToPoint();

                foreach (var platform in world.Platforms)
                {
                    if (platform.Contains(point))
                    {
                        PlaceSimplePortal(world, projectile.IsBlue, point, projectile.Direction, platform);
                        shouldRemove = true;
                        break;
                    }
                }

                if (shouldRemove)
                    break;

                if (projectile.Position.X < 0 ||
                    projectile.Position.X > 1600 ||
                    projectile.Position.Y < 0 ||
                    projectile.Position.Y > 900)
                {
                    shouldRemove = true;
                    break;
                }
            }

            if (shouldRemove)
                world.Projectiles.RemoveAt(i);
        }
    }

    private void PlaceSimplePortal(
        GameWorld world,
        bool isBlue,
        Point checkPoint,
        Vector2 direction,
        Rectangle platform)
    {
        const int PortalWidth = 20;
        const int PortalHeight = 60;

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

        if (isBlue)
            world.BluePortal = new PortalModel(newPortal, exitDirection);
        else
            world.OrangePortal = new PortalModel(newPortal, exitDirection);
    }
}