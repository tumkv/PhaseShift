using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Managers;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class PortalController
{
    private const int PlayerWidth = 40;
    private const int PlayerHeight = 40;
    private const float MaxFallSpeed = 16f;

    private const float ProjectileSpeed = 70f;
    private const float PortalRayStep = 4f;

    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    public void Update(
        GameWorld world,
        MouseState mouse,
        KeyboardState keyboard,
        float deltaTime,
        SoundManager soundManager)
    {
        if (world.PortalExitTimer > 0f)
            world.PortalExitTimer -= deltaTime;

        if (keyboard.IsKeyDown(Keys.R) &&
            !_previousKeyboardState.IsKeyDown(Keys.R))
        {
            ResetPortals(world, soundManager);
        }

        if (mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            ShootPortalProjectile(world, mouse.Position, true, soundManager);
        }

        if (mouse.RightButton == ButtonState.Pressed &&
            _previousMouseState.RightButton == ButtonState.Released)
        {
            ShootPortalProjectile(world, mouse.Position, false, soundManager);
        }

        UpdateProjectiles(world, soundManager);
        UpdateTeleportation(world);

        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
    }

    private void UpdateTeleportation(GameWorld world)
    {
        if (world.BluePortal == null || world.OrangePortal == null)
        {
            world.IsTeleporting = false;
            return;
        }

        var player = world.Player;

        if (!world.IsTeleporting)
        {
            if (player.Bounds.Intersects(world.BluePortal.Bounds))
            {
                player.Position = GetExitPosition(world.OrangePortal);
                ApplyExitVelocity(world, world.BluePortal, world.OrangePortal);

                world.IsTeleporting = true;
            }
            else if (player.Bounds.Intersects(world.OrangePortal.Bounds))
            {
                player.Position = GetExitPosition(world.BluePortal);
                ApplyExitVelocity(world, world.OrangePortal, world.BluePortal);

                world.IsTeleporting = true;
            }
        }

        if (!player.Bounds.Intersects(world.BluePortal.Bounds) &&
            !player.Bounds.Intersects(world.OrangePortal.Bounds))
        {
            world.IsTeleporting = false;
        }
    }

    private Vector2 GetExitPosition(PortalModel portal)
    {
        Vector2 center = new Vector2(
            portal.Bounds.Center.X - PlayerModel.Width / 2,
            portal.Bounds.Center.Y - PlayerModel.Height / 2);

        return center + portal.ExitDirection * 50f;
    }

    private void ApplyExitVelocity(
        GameWorld world,
        PortalModel entryPortal,
        PortalModel exitPortal)
    {
        world.Player.Velocity = RotateMomentum(
            world,
            world.Player.Velocity,
            entryPortal.ExitDirection,
            exitPortal.ExitDirection);

        world.Player.PreserveMomentum = true;

        world.PortalExitTimer = 0.15f;
    }

    private Vector2 RotateMomentum(
        GameWorld world,
        Vector2 velocity,
        Vector2 entryDirection,
        Vector2 exitDirection)
    {
        float speed = velocity.Length();

        if (speed < 1f)
            return Vector2.Zero;

        if (entryDirection == exitDirection)
        {
            if (world.SameDirectionPortalSpeed <= 0f)
                world.SameDirectionPortalSpeed = speed;

            speed = Math.Min(speed, world.SameDirectionPortalSpeed);
        }
        else
        {
            world.SameDirectionPortalSpeed = 0f;
        }

        if (speed > MaxFallSpeed)
            speed = MaxFallSpeed;

        return exitDirection * speed;
    }

    private void ResetPortals(GameWorld world, SoundManager soundManager)
    {
        if (world.BluePortal != null || world.OrangePortal != null)
        {
            soundManager.PlayPortalClose();
        }

        world.BluePortal = null;
        world.OrangePortal = null;

        world.IsTeleporting = false;
        world.PortalExitTimer = 0f;
        world.SameDirectionPortalSpeed = 0f;

        world.Player.PreserveMomentum = false;

        if (world.Cube.HasCube)
            world.Cube.IsTeleporting = false;
    }

    private void ShootPortalProjectile(
        GameWorld world,
        Point mousePosition,
        bool isBlue,
        SoundManager soundManager)
    {
        Vector2 playerCenter = new Vector2(
            world.Player.Position.X + PlayerModel.Width / 2,
            world.Player.Position.Y + PlayerModel.Height / 2);

        Vector2 target = new Vector2(mousePosition.X, mousePosition.Y);
        Vector2 direction = target - playerCenter;

        if (direction == Vector2.Zero)
            return;

        direction.Normalize();

        if (isBlue)
            soundManager.PlayBluePortalShoot();
        else
            soundManager.PlayOrangePortalShoot();

        world.Projectiles.Add(new PortalProjectileModel(
            playerCenter,
            direction,
            isBlue));
    }

    private void UpdateProjectiles(GameWorld world, SoundManager soundManager)
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

                if (PointInsideBackgroundBlock(world, point))
                {
                    soundManager.PlayPortalInvalidSurface();
                    shouldRemove = true;
                    break;
                }

                foreach (var platform in world.Platforms)
                {
                    if (platform.Contains(point))
                    {
                        PlacePortal(
                            world,
                            projectile.IsBlue,
                            point,
                            projectile.Direction,
                            platform,
                            soundManager);
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

    private void PlacePortal(
        GameWorld world,
        bool isBlue,
        Point checkPoint,
        Vector2 direction,
        Rectangle platform,
        SoundManager soundManager)
    {
        const int PortalWidth = 20;
        const int PortalHeight = 60;
        const int MagnetDistance = 5;

        bool isWall = platform.Height > platform.Width;

        Rectangle newPortal;
        Vector2 exitDirection;

        if (IsForbiddenPlatform(world, platform))
        {
            soundManager.PlayPortalInvalidSurface();
            return;
        }

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

        if (PortalTouchesBackgroundBlock(world, newPortal) ||
            PortalTouchesElectricZone(world, newPortal) ||
            PortalTouchesButton(world, newPortal) ||
            PortalTouchesDoor(world, newPortal) ||
            PortalOverlapsOtherPlatforms(world, newPortal, platform))
        {
            soundManager.PlayPortalInvalidSurface();
            return;
        }

        if (PortalTouchesPlatformEdge(isWall, newPortal, platform))
        {
            soundManager.PlayPortalInvalidSurface();
            return;
        }

        if (isBlue && world.OrangePortal != null)
        {
            Rectangle magnetZone = world.OrangePortal.Bounds;
            magnetZone.Inflate(MagnetDistance, MagnetDistance);

            if (magnetZone.Intersects(newPortal))
            {
                newPortal = world.OrangePortal.Bounds;
                exitDirection = world.OrangePortal.ExitDirection;
                world.OrangePortal = null;
            }
        }
        else if (!isBlue && world.BluePortal != null)
        {
            Rectangle magnetZone = world.BluePortal.Bounds;
            magnetZone.Inflate(MagnetDistance, MagnetDistance);

            if (magnetZone.Intersects(newPortal))
            {
                newPortal = world.BluePortal.Bounds;
                exitDirection = world.BluePortal.ExitDirection;
                world.BluePortal = null;
            }
        }

        var portal = new PortalModel(newPortal, exitDirection);

        if (isBlue)
        {
            world.BluePortal = portal;
            soundManager.PlayBluePortalOpen();
        }
        else
        {
            world.OrangePortal = portal;
            soundManager.PlayOrangePortalOpen();
        }
    }

    private bool PointInsideBackgroundBlock(GameWorld world, Point point)
    {
        foreach (var block in world.BackgroundBlocks)
        {
            if (block.Contains(point))
                return true;
        }

        return false;
    }

    private bool PortalTouchesBackgroundBlock(GameWorld world, Rectangle portal)
    {
        foreach (var block in world.BackgroundBlocks)
        {
            if (portal.Intersects(block))
                return true;
        }

        return false;
    }

    private bool PortalTouchesElectricZone(GameWorld world, Rectangle portal)
    {
        foreach (var electricZone in world.ElectricZones)
        {
            if (portal.Intersects(electricZone))
                return true;
        }

        return false;
    }

    private bool PortalTouchesButton(GameWorld world, Rectangle portal)
    {
        return world.ButtonDoor.HasButtonDoorLevel &&
               portal.Intersects(world.ButtonDoor.Button);
    }

    private bool PortalTouchesDoor(GameWorld world, Rectangle portal)
    {
        return world.ButtonDoor.HasButtonDoorLevel &&
               !world.ButtonDoor.DoorOpen &&
               portal.Intersects(world.ButtonDoor.Door);
    }

    private bool PortalOverlapsOtherPlatforms(
        GameWorld world,
        Rectangle portal,
        Rectangle targetPlatform)
    {
        foreach (var platform in world.Platforms)
        {
            if (platform == targetPlatform)
                continue;

            if (platform == Rectangle.Empty)
                continue;

            if (portal.Intersects(platform))
                return true;
        }

        return false;
    }

    private bool PortalTouchesPlatformEdge(
        bool isWall,
        Rectangle portal,
        Rectangle platform)
    {
        if (isWall)
        {
            return portal.Top <= platform.Top ||
                   portal.Bottom >= platform.Bottom;
        }

        return portal.Left <= platform.Left ||
               portal.Right >= platform.Right;
    }

    private bool IsForbiddenPlatform(GameWorld world, Rectangle platform)
    {
        var trap = world.MovingSpikeTrap;

        if (trap.HasMovingSpikeTrap)
        {
            if (trap.PlatformIndex >= 0 &&
                trap.PlatformIndex < world.Platforms.Count &&
                world.Platforms[trap.PlatformIndex] == platform)
                return true;

            if (trap.SupportIndex >= 0 &&
                trap.SupportIndex < world.Platforms.Count &&
                world.Platforms[trap.SupportIndex] == platform)
                return true;
        }

        if (world.ButtonDoor.HasButtonDoorLevel &&
            world.ButtonDoor.DoorPlatformIndex >= 0 &&
            world.ButtonDoor.DoorPlatformIndex < world.Platforms.Count &&
            world.Platforms[world.ButtonDoor.DoorPlatformIndex] == platform)
            return true;

        return false;
    }
}