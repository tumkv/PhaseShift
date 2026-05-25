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
    private readonly CubeController _cubeController;
    private readonly ButtonDoorController _buttonDoorController;
    private readonly TrapController _trapController;
    private readonly ElectricityController _electricityController;
    private readonly LevelProgressController _levelProgressController;
    private readonly CubePortalController _cubePortalController;
    private readonly PlayerSoundController _playerSoundController;

    private KeyboardState _previousKeyboardState;

    public GameController(
        PlayerController playerController,
        CollisionController collisionController,
        PortalController portalController,
        LevelController levelController,
        MenuController menuController,
        SettingsController settingsController,
        CubeController cubeController,
        CubePortalController cubePortalController,
        ButtonDoorController buttonDoorController,
        TrapController trapController,
        ElectricityController electricityController,
        LevelProgressController levelProgressController,
        PlayerSoundController playerSoundController)
    {
        _playerController = playerController;
        _collisionController = collisionController;
        _portalController = portalController;
        _levelController = levelController;
        _menuController = menuController;
        _settingsController = settingsController;
        _cubeController = cubeController;
        _cubePortalController = cubePortalController;
        _buttonDoorController = buttonDoorController;
        _trapController = trapController;
        _electricityController = electricityController;
        _levelProgressController = levelProgressController;
        _playerSoundController = playerSoundController;
    }

    // пауза
    private void HandleGameState(GameWorld world, KeyboardState keyboard)
    {
        bool escapePressed =
            keyboard.IsKeyDown(Keys.Escape) &&
            !_previousKeyboardState.IsKeyDown(Keys.Escape);

        if (escapePressed)
        {
            if (world.State == GameState.Playing)
            {
                world.State = GameState.Paused;
            }
            else if (world.State == GameState.Paused)
            {
                world.State = GameState.Playing;
            }
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

        _levelProgressController.Update(world, keyboard, mouse, deltaTime);

        if (world.Progress.IsFading || world.Progress.LevelCompletedScreen)
            return;

        HandleGameState(world, keyboard);

        if (world.State != GameState.Playing)
            return;

        _playerSoundController.UpdateTimers(deltaTime);

        float verticalSpeedBeforeCollision = world.Player.Velocity.Y;

        _playerController.Update(world, keyboard);
        _trapController.Update(world, deltaTime, soundManager);
        _electricityController.Update(world, deltaTime);

        _collisionController.ResolvePlayerCollisions(world);

        _playerSoundController.PlayHighVelocityImpact(
            _collisionController.LastHorizontalImpactSpeed,
            soundManager);

        _playerSoundController.UpdateFootsteps(
            world,
            keyboard,
            soundManager);

        _playerSoundController.UpdateLandingSound(
            world,
            verticalSpeedBeforeCollision,
            soundManager);

        _portalController.Update(world, mouse, keyboard, deltaTime, soundManager);

        _cubeController.Update(world, keyboard, mouse, soundManager);
        _cubePortalController.Update(world);

        _buttonDoorController.Update(world, soundManager);



        CheckExit(world);
        CheckDeath(world);
    }

    private void CheckExit(GameWorld world)
    {
        if (!world.Player.Bounds.Intersects(world.Exit))
            return;

        _levelProgressController.CompleteLevel(world);
    }

    private void CheckDeath(GameWorld world)
    {
        // если игрок упал вниз
        if (world.Player.Position.Y > 950)
        {
            _levelProgressController.Die(world);
            return;
        }

        // смерть от шипов
        foreach (var spike in world.Spikes)
        {
            if (world.Player.Bounds.Intersects(spike))
            {
                _levelProgressController.Die(world);
                return;
            }
        }

        // смерть от электричества
        if (world.Electricity.Active)
        {
            foreach (var electricZone in world.ElectricZones)
            {
                if (world.Player.Bounds.Intersects(electricZone))
                {
                    _levelProgressController.Die(world);
                    return;
                }
            }
        }
    }
}