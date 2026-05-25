using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class GameView
{
    private readonly Texture2D _pixel;

    public GameView(Texture2D pixel)
    {
        _pixel = pixel;
    }

    public void DrawWorld(SpriteBatch spriteBatch, GameWorld world)
    {
        foreach (var block in world.BackgroundBlocks)
            spriteBatch.Draw(_pixel, block, Color.DarkGray);

        foreach (var platform in world.Platforms)
            spriteBatch.Draw(_pixel, platform, Color.Gray);

        foreach (var projectile in world.Projectiles)
        {
            Rectangle projectileRect = new Rectangle(
                (int)projectile.Position.X - 4,
                (int)projectile.Position.Y - 4,
                8,
                8);

            spriteBatch.Draw(
                _pixel,
                projectileRect,
                projectile.IsBlue ? Color.Cyan : Color.Orange);
        }

        if (world.BluePortal != null)
            spriteBatch.Draw(_pixel, world.BluePortal.Bounds, Color.Cyan);

        if (world.OrangePortal != null)
            spriteBatch.Draw(_pixel, world.OrangePortal.Bounds, Color.Orange);

        foreach (var spike in world.Spikes)
            spriteBatch.Draw(_pixel, spike, Color.Red);

        spriteBatch.Draw(_pixel, world.Exit, Color.Green);

        spriteBatch.Draw(_pixel, world.Player.Bounds, Color.Orange);
    }
}