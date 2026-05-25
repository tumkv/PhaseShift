using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class MenuController
{
    private readonly LevelController _levelController;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    public MenuController(LevelController levelController)
    {
        _levelController = levelController;
    }

    public void Update(GameWorld world, MouseState mouse, KeyboardState keyboard)
    {
        if (world.State == GameState.MainMenu)
        {
            UpdateMainMenu(world, mouse);
        }
        else if (world.State == GameState.LevelSelect)
        {
            UpdateLevelSelect(world, mouse, keyboard);
        }
        else if (world.State == GameState.Paused)
        {
            UpdatePauseMenu(world, mouse, keyboard);
        }

        _previousMouseState = mouse;
        _previousKeyboardState = keyboard;
    }

    private bool LeftMouseClicked(MouseState mouse)
    {
        return mouse.LeftButton == ButtonState.Pressed &&
               _previousMouseState.LeftButton == ButtonState.Released;
    }

    private bool EscapePressed(KeyboardState keyboard)
    {
        return keyboard.IsKeyDown(Keys.Escape) &&
               !_previousKeyboardState.IsKeyDown(Keys.Escape);
    }

    private void UpdateMainMenu(GameWorld world, MouseState mouse)
    {
        Point mousePoint = mouse.Position;
        var menu = world.Menu;

        menu.HoverNewGame = menu.NewGameButton.Contains(mousePoint);
        menu.HoverContinue = menu.ContinueButton.Contains(mousePoint);
        menu.HoverSettings = menu.SettingsButton.Contains(mousePoint);
        menu.HoverExit = menu.ExitButton.Contains(mousePoint);

        if (!LeftMouseClicked(mouse))
            return;

        if (menu.HoverNewGame)
        {
            _levelController.LoadLevel(world, 1);
            world.State = GameState.Playing;
        }
        else if (menu.HoverContinue)
        {
            world.State = GameState.LevelSelect;
        }
        else if (menu.HoverSettings)
        {
            world.State = GameState.Settings;
        }
        else if (menu.HoverExit)
        {
            world.ShouldExitGame = true;
        }
    }

    private void UpdateLevelSelect(GameWorld world, MouseState mouse, KeyboardState keyboard)
    {
        if (EscapePressed(keyboard))
        {
            world.State = GameState.MainMenu;
            return;
        }

        if (!LeftMouseClicked(mouse))
            return;

        Point mousePoint = mouse.Position;

        for (int i = 0; i < world.Menu.LevelButtons.Count; i++)
        {
            if (world.Menu.LevelButtons[i].Contains(mousePoint))
            {
                _levelController.LoadLevel(world, i + 1);
                world.State = GameState.Playing;
                return;
            }
        }
    }

    private void UpdatePauseMenu(GameWorld world, MouseState mouse, KeyboardState keyboard)
    {
        Point mousePoint = mouse.Position;
        var menu = world.Menu;

        menu.HoverResume = menu.ResumeButton.Contains(mousePoint);
        menu.HoverPauseSettings = menu.PauseSettingsButton.Contains(mousePoint);
        menu.HoverMainMenu = menu.MainMenuButton.Contains(mousePoint);

        if (EscapePressed(keyboard))
        {
            world.State = GameState.Playing;
            return;
        }

        if (!LeftMouseClicked(mouse))
            return;

        if (menu.HoverResume)
        {
            world.State = GameState.Playing;
        }
        else if (menu.HoverPauseSettings)
        {
            world.State = GameState.Settings;
        }
        else if (menu.HoverMainMenu)
        {
            world.State = GameState.MainMenu;
        }
    }
}