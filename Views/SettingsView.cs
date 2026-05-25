using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class SettingsView
{
    private Texture2D _pixel;
    private SpriteFont _font;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = content.Load<SpriteFont>("DefaultFont");
    }

    public void Draw(SpriteBatch spriteBatch, GameWorld world)
    {
        if (world.State != GameState.Settings)
            return;

        var settings = world.Settings;

        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1600, 900), Color.Black);

        spriteBatch.DrawString(
            _font,
            "SETTINGS",
            new Vector2(120, 120),
            Color.Orange);

        DrawSlider(
            spriteBatch,
            "Music Volume",
            settings.MusicSliderBar,
            settings.MusicVolume,
            190);

        DrawSlider(
            spriteBatch,
            "SFX Volume",
            settings.SfxSliderBar,
            settings.SfxVolume,
            290);

        spriteBatch.DrawString(
            _font,
            "Press ESC to go back",
            new Vector2(120, 500),
            Color.Gray);
    }

    private void DrawSlider(
        SpriteBatch spriteBatch,
        string title,
        Rectangle bar,
        float value,
        int titleY)
    {
        spriteBatch.DrawString(
            _font,
            title,
            new Vector2(120, titleY),
            Color.White);

        spriteBatch.Draw(_pixel, bar, Color.DarkGray);

        Rectangle fill = new Rectangle(
            bar.X,
            bar.Y,
            (int)(bar.Width * value),
            bar.Height);

        spriteBatch.Draw(_pixel, fill, Color.Orange);

        Rectangle knob = new Rectangle(
            fill.Right - 10,
            bar.Y - 8,
            20,
            24);

        spriteBatch.Draw(_pixel, knob, Color.White);
    }
}