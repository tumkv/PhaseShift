using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;
using System;

namespace PhaseShift.Views;

public class GameView
{
    private Texture2D _pixel;

    private Texture2D _levelBackground;

    private Texture2D _bluePortalTexture;
    private Texture2D _orangePortalTexture;
    private Texture2D _portalGlowTexture;

    private Texture2D _blueProjectileTexture;
    private Texture2D _orangeProjectileTexture;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _levelBackground = content.Load<Texture2D>("level_background");

        _bluePortalTexture = content.Load<Texture2D>("blue_portal");
        _orangePortalTexture = content.Load<Texture2D>("orange_portal");
        _portalGlowTexture = content.Load<Texture2D>("portal_glow");

        _blueProjectileTexture = content.Load<Texture2D>("blueprojectile");
        _orangeProjectileTexture = content.Load<Texture2D>("orangeprojectile");
    }





    public void DrawWorld(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(
            _levelBackground,
            new Rectangle(0, 0, 1600, 900),
            Color.White);

        DrawBackgroundBlocks(spriteBatch, world);
        DrawPlatforms(spriteBatch, world);
        DrawButtonDoor(spriteBatch, world);
        DrawElectricity(spriteBatch, world);
        DrawExit(spriteBatch, world);
        DrawProjectiles(spriteBatch, world);
        DrawPortals(spriteBatch, world);
        DrawSpikes(spriteBatch, world);
        DrawCube(spriteBatch, world);
        DrawPlayer(spriteBatch, world);


        if (world.State == GameState.Paused)
            DrawPause(spriteBatch);
    }

    private void DrawPause(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, 1600, 900),
            Color.Black * 0.6f);
    }

    private void DrawBackgroundBlocks(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var block in world.BackgroundBlocks)
        {
            spriteBatch.Draw(_pixel, block, Color.DarkGray);
        }
    }

    private void DrawPlatforms(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var platform in world.Platforms)
        {
            spriteBatch.Draw(_pixel, platform, Color.Gray);
        }
    }

    private void DrawExit(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(_pixel, world.Exit, Color.Green);
    }

    private void DrawCube(SpriteBatch spriteBatch, GameWorld world)
    {
        if (!world.Cube.HasCube)
            return;

        spriteBatch.Draw(_pixel, world.Cube.Bounds, Color.SandyBrown);
    }

    private void DrawButtonDoor(SpriteBatch spriteBatch, GameWorld world)
    {
        var buttonDoor = world.ButtonDoor;

        if (!buttonDoor.HasButtonDoorLevel)
            return;

        Color buttonColor = buttonDoor.DoorOpen
            ? Color.LimeGreen
            : Color.Red;

        spriteBatch.Draw(_pixel, buttonDoor.Button, buttonColor);

        if (!buttonDoor.DoorOpen)
            spriteBatch.Draw(_pixel, buttonDoor.Door, Color.DarkSlateGray);
    }

    private void DrawElectricity(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var electricZone in world.ElectricZones)
        {
            Color electricColor = world.Electricity.Active
                ? Color.Cyan
                : Color.DarkSlateGray;

            spriteBatch.Draw(_pixel, electricZone, electricColor);
        }
    }

    private void DrawSpikes(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var spike in world.Spikes)
        {
            spriteBatch.Draw(_pixel, spike, Color.Red);
        }
    }

    private void DrawPlayer(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(_pixel, world.Player.Bounds, Color.Orange);
    }

    private void DrawProjectiles(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var projectile in world.Projectiles)
        {
            Texture2D texture = projectile.IsBlue
                ? _blueProjectileTexture
                : _orangeProjectileTexture;

            float rotation =
                (float)Math.Atan2(
                    projectile.Direction.Y,
                    projectile.Direction.X);

            spriteBatch.Draw(
                texture,
                projectile.Position,
                null,
                Color.White * (projectile.Lifetime / projectile.MaxLifetime),
                rotation,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                0.02f,
                SpriteEffects.None,
                0f);
        }
    }

    private void DrawPortals(SpriteBatch spriteBatch, GameWorld world)
    {
        if (world.BluePortal != null)
        {
            DrawPortal(
                spriteBatch,
                world.BluePortal,
                _bluePortalTexture,
                Color.Cyan);
        }

        if (world.OrangePortal != null)
        {
            DrawPortal(
                spriteBatch,
                world.OrangePortal,
                _orangePortalTexture,
                Color.Orange);
        }
    }

    private void DrawPortal(
        SpriteBatch spriteBatch,
        PortalModel portal,
        Texture2D texture,
        Color glowColor)
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

        spriteBatch.Draw(
            _portalGlowTexture,
            new Vector2(glowBounds.Center.X, glowBounds.Center.Y),
            null,
            glowColor * 0.6f,
            rotation,
            glowOrigin,
            glowScale,
            effects,
            0f);

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

        spriteBatch.Draw(
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
}