using Microsoft.Xna.Framework.Input;
using PhaseShift.Managers;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class GameController
{
    private readonly PlayerController _playerController;
    private readonly CollisionController _collisionController;
    private readonly PortalController _portalController;
    private readonly LevelController _levelController;
    private readonly MenuController _menuController;
    private readonly SettingsController _settingsController;

    private KeyboardState _previousKeyboardState;

    public GameController(
        PlayerController playerController,
        CollisionController collisionController,
        PortalController portalController,
        LevelController levelController,
        MenuController menuController,
        SettingsController settingsController)
    {
        _playerController = playerController;
        _collisionController = collisionController;
        _portalController = portalController;
        _levelController = levelController;
        _menuController = menuController;
        _settingsController = settingsController;
    }

    // пауза
    private void HandleGameState(GameWorld world, KeyboardState keyboard)
    {
        bool escapePressed =
            keyboard.IsKeyDown(Keys.Escape) &&
            !_previousKeyboardState.IsKeyDown(Keys.Escape);

        if (escapePressed && world.State == GameState.Playing)
        {
            world.State = GameState.Paused;
        }

        _previousKeyboardState = keyboard;
    }

    public void Update(
        GameWorld world,
        KeyboardState keyboard,
        MouseState mouse,
        float deltaTime,
    SoundManager soundManager)
    {
        _menuController.Update(world, mouse, keyboard);
        _settingsController.Update(world, mouse, keyboard);

        soundManager.MusicVolume = world.Settings.MusicVolume;
        soundManager.SfxVolume = world.Settings.SfxVolume;
        soundManager.UpdateVolumes();

        HandleGameState(world, keyboard);

        if (world.State != GameState.Playing)
            return;

        _playerController.Update(world, keyboard);
        _collisionController.ResolvePlayerCollisions(world);
        _portalController.Update(world, mouse, deltaTime, soundManager);

        CheckExit(world);
        CheckDeath(world);
    }

    private void CheckExit(GameWorld world)
    {
        if (!world.Player.Bounds.Intersects(world.Exit))
            return;

        int nextLevel = world.CurrentLevel + 1;

        // пока у нас перенесён только 1 уровень
        if (nextLevel > 1)
            nextLevel = 1;

        _levelController.LoadLevel(world, nextLevel);
    }

    private void CheckDeath(GameWorld world)
    {
        // если игрок упал вниз
        if (world.Player.Position.Y > 950)
        {
            _levelController.LoadLevel(world, world.CurrentLevel);
            return;
        }

        // смерть от шипов
        foreach (var spike in world.Spikes)
        {
            if (world.Player.Bounds.Intersects(spike))
            {
                _levelController.LoadLevel(world, world.CurrentLevel);
                return;
            }
        }

        // смерть от электричества
        if (world.ElectricActive)
        {
            foreach (var electricZone in world.ElectricZones)
            {
                if (world.Player.Bounds.Intersects(electricZone))
                {
                    _levelController.LoadLevel(world, world.CurrentLevel);
                    return;
                }
            }
        }
    }
}