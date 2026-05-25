using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class LevelProgressView
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
        var progress = world.Progress;

        if (!progress.LevelCompletedScreen)
            return;

        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, 1600, 900),
            Color.Black);

        Color textColor = Color.White * progress.LevelCompleteAlpha;

        string title = $"LEVEL {progress.CompletedLevelNumber} COMPLETED";
        Vector2 titleSize = _font.MeasureString(title);

        spriteBatch.DrawString(
            _font,
            title,
            new Vector2((1600 - titleSize.X) / 2, 360),
            textColor);

        string continueText = "Press any key to continue";
        Vector2 continueSize = _font.MeasureString(continueText);

        spriteBatch.DrawString(
            _font,
            continueText,
            new Vector2((1600 - continueSize.X) / 2, 460),
            textColor);
    }
}