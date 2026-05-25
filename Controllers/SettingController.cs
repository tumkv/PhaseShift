using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class SettingsController
{
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    public void Update(GameWorld world, MouseState mouse, KeyboardState keyboard)
    {
        if (world.State != GameState.Settings)
        {
            _previousMouseState = mouse;
            _previousKeyboardState = keyboard;
            return;
        }

        var settings = world.Settings;
        Point mousePoint = mouse.Position;

        Rectangle musicKnob = GetMusicKnob(settings);
        Rectangle sfxKnob = GetSfxKnob(settings);

        if (mouse.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            if (musicKnob.Contains(mousePoint))
                settings.DraggingMusicSlider = true;

            if (sfxKnob.Contains(mousePoint))
                settings.DraggingSfxSlider = true;
        }

        if (mouse.LeftButton == ButtonState.Released)
        {
            settings.DraggingMusicSlider = false;
            settings.DraggingSfxSlider = false;
        }

        if (settings.DraggingMusicSlider)
        {
            settings.MusicVolume =
                (mouse.X - settings.MusicSliderBar.X) /
                (float)settings.MusicSliderBar.Width;

            settings.MusicVolume = MathHelper.Clamp(settings.MusicVolume, 0f, 1f);
        }

        if (settings.DraggingSfxSlider)
        {
            settings.SfxVolume =
                (mouse.X - settings.SfxSliderBar.X) /
                (float)settings.SfxSliderBar.Width;

            settings.SfxVolume = MathHelper.Clamp(settings.SfxVolume, 0f, 1f);
        }

        bool escapePressed =
            keyboard.IsKeyDown(Keys.Escape) &&
            !_previousKeyboardState.IsKeyDown(Keys.Escape);

        if (escapePressed)
            world.State = GameState.MainMenu;

        _previousMouseState = mouse;
        _previousKeyboardState = keyboard;
    }

    private Rectangle GetMusicKnob(SettingsModel settings)
    {
        return new Rectangle(
            (int)(settings.MusicSliderBar.X + settings.MusicVolume * settings.MusicSliderBar.Width) - 10,
            settings.MusicSliderBar.Y - 8,
            20,
            24);
    }

    private Rectangle GetSfxKnob(SettingsModel settings)
    {
        return new Rectangle(
            (int)(settings.SfxSliderBar.X + settings.SfxVolume * settings.SfxSliderBar.Width) - 10,
            settings.SfxSliderBar.Y - 8,
            20,
            24);
    }
}