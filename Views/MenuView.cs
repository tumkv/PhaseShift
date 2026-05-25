using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class MenuView
{
    private Texture2D _pixel;
    private Texture2D _menuBackground;
    private SpriteFont _font;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _menuBackground = content.Load<Texture2D>("menu_background");
        _font = content.Load<SpriteFont>("DefaultFont");
    }

    public void Draw(SpriteBatch spriteBatch, GameWorld world)
    {
        if (world.State == GameState.MainMenu)
        {
            DrawMainMenu(spriteBatch, world);
        }
        else if (world.State == GameState.LevelSelect)
        {
            DrawLevelSelect(spriteBatch, world);
        }
        else if (world.State == GameState.Paused)
        {
            DrawPauseMenu(spriteBatch, world);
        }
    }

    private void DrawMainMenu(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(
            _menuBackground,
            new Rectangle(0, 0, 1600, 900),
            Color.White);

        var menu = world.Menu;

        DrawMenuButton(spriteBatch, menu.NewGameButton, "NEW GAME", menu.HoverNewGame);
        DrawMenuButton(spriteBatch, menu.ContinueButton, "CONTINUE", menu.HoverContinue);
        DrawMenuButton(spriteBatch, menu.SettingsButton, "SETTINGS", menu.HoverSettings);
        DrawMenuButton(spriteBatch, menu.ExitButton, "EXIT", menu.HoverExit);
    }

    private void DrawLevelSelect(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1600, 900), Color.Black);

        for (int i = 0; i < world.Menu.LevelButtons.Count; i++)
        {
            Rectangle button = world.Menu.LevelButtons[i];
            bool hovered = button.Contains(Microsoft.Xna.Framework.Input.Mouse.GetState().Position);

            DrawMenuButton(spriteBatch, button, (i + 1).ToString(), hovered);
        }
    }

    private void DrawPauseMenu(SpriteBatch spriteBatch, GameWorld world)
    {
        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, 1600, 900),
            Color.Black * 0.6f);

        string pausedText = "PAUSED";
        Vector2 pausedSize = _font.MeasureString(pausedText);

        spriteBatch.DrawString(
            _font,
            pausedText,
            new Vector2((1600 - pausedSize.X) / 2, 180),
            Color.White);

        var menu = world.Menu;

        DrawMenuButton(spriteBatch, menu.ResumeButton, "RESUME", menu.HoverResume);
        DrawMenuButton(spriteBatch, menu.PauseSettingsButton, "SETTINGS", menu.HoverPauseSettings);
        DrawMenuButton(spriteBatch, menu.MainMenuButton, "MAIN MENU", menu.HoverMainMenu);
    }

    private void DrawMenuButton(SpriteBatch spriteBatch, Rectangle rect, string text, bool hovered)
    {
        Color buttonColor = hovered
            ? Color.Orange
            : Color.White;

        if (hovered)
        {
            Rectangle glow = new Rectangle(
                rect.X - 10,
                rect.Y - 10,
                rect.Width + 20,
                rect.Height + 20);

            spriteBatch.Draw(_pixel, glow, Color.Orange * 0.35f);
        }

        spriteBatch.Draw(_pixel, rect, buttonColor);

        Vector2 textSize = _font.MeasureString(text);

        spriteBatch.DrawString(
            _font,
            text,
            new Vector2(
                rect.Center.X - textSize.X / 2,
                rect.Center.Y - textSize.Y / 2),
            Color.Black);
    }
}