using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class LevelProgressController
{
    private readonly LevelController _levelController;

    public LevelProgressController(LevelController levelController)
    {
        _levelController = levelController;
    }

    public void Update(GameWorld world, KeyboardState keyboard, MouseState mouse, float deltaTime)
    {
        UpdateDeathFade(world, deltaTime);
        UpdateLevelCompletedScreen(world, keyboard, mouse, deltaTime);
    }

    public void Die(GameWorld world)
    {
        var progress = world.Progress;

        if (progress.IsFading)
            return;

        progress.IsFading = true;
        progress.RestartAfterFade = true;
        progress.FadeAlpha = 0f;
    }

    public void CompleteLevel(GameWorld world)
    {
        var progress = world.Progress;

        progress.CompletedLevelNumber = world.CurrentLevel;
        progress.LevelCompletedScreen = true;
        progress.LevelCompleteAlpha = 0f;

        world.BluePortal = null;
        world.OrangePortal = null;
        world.Player.Velocity = Microsoft.Xna.Framework.Vector2.Zero;
    }

    private void UpdateDeathFade(GameWorld world, float deltaTime)
    {
        var progress = world.Progress;

        if (progress.IsFading)
        {
            progress.FadeAlpha += LevelProgressModel.FadeSpeed * deltaTime;

            if (progress.FadeAlpha >= 1f)
            {
                progress.FadeAlpha = 1f;

                if (progress.RestartAfterFade)
                {
                    _levelController.LoadLevel(world, world.CurrentLevel);
                    progress.RestartAfterFade = false;
                }

                progress.IsFading = false;
            }
        }
        else if (progress.FadeAlpha > 0f)
        {
            progress.FadeAlpha -= LevelProgressModel.FadeSpeed * deltaTime;

            if (progress.FadeAlpha < 0f)
                progress.FadeAlpha = 0f;
        }
    }

    private void UpdateLevelCompletedScreen(
        GameWorld world,
        KeyboardState keyboard,
        MouseState mouse,
        float deltaTime)
    {
        var progress = world.Progress;

        if (!progress.LevelCompletedScreen)
            return;

        progress.LevelCompleteAlpha += LevelProgressModel.LevelCompleteFadeSpeed * deltaTime;

        if (progress.LevelCompleteAlpha > 1f)
            progress.LevelCompleteAlpha = 1f;

        bool anyInput =
            keyboard.GetPressedKeys().Length > 0 ||
            mouse.LeftButton == ButtonState.Pressed ||
            mouse.RightButton == ButtonState.Pressed;

        if (progress.LevelCompleteAlpha >= 1f && anyInput)
        {
            int nextLevel = world.CurrentLevel + 1;

            if (nextLevel > 6)
                nextLevel = 1;

            _levelController.LoadLevel(world, nextLevel);
            progress.LevelCompletedScreen = false;
        }
    }
}