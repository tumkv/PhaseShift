using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Managers;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class CubeController
{
    private const float Gravity = 0.35f;
    private const float MaxFallSpeed = 16f;

    private KeyboardState _previousKeyboardState;

    public void Update(
        GameWorld world,
        KeyboardState keyboard,
        MouseState mouse,
        SoundManager soundManager)
    {
        var cube = world.Cube;

        if (!cube.HasCube)
            return;

        bool ePressed =
            keyboard.IsKeyDown(Keys.E) &&
            !_previousKeyboardState.IsKeyDown(Keys.E);

        Vector2 playerCenter = new Vector2(
            world.Player.Position.X + PlayerModel.Width / 2,
            world.Player.Position.Y + PlayerModel.Height / 2);

        Vector2 cubeCenter = new Vector2(
            cube.Position.X + CubeModel.Size / 2,
            cube.Position.Y + CubeModel.Size / 2);

        float distanceToCube = Vector2.Distance(playerCenter, cubeCenter);

        if (ePressed)
        {
            if (cube.IsHolding)
            {
                cube.IsHolding = false;
                cube.Velocity = Vector2.Zero;
                soundManager.StopCubeHold();
            }
            else if (distanceToCube < 100f)
            {
                cube.IsHolding = true;
                cube.Velocity = Vector2.Zero;
                soundManager.PlayCubeHold();
            }
            else
            {
                soundManager.PlayCubeFail();
            }
        }

        if (cube.IsHolding)
        {
            Vector2 mousePosition = new Vector2(mouse.X, mouse.Y);
            Vector2 direction = mousePosition - playerCenter;

            if (direction != Vector2.Zero)
                direction.Normalize();

            Vector2 targetPosition = playerCenter + direction * CubeModel.HoldDistance;
            targetPosition -= new Vector2(CubeModel.Size / 2, CubeModel.Size / 2);

            cube.Position = Vector2.Lerp(
                cube.Position,
                targetPosition,
                CubeModel.FollowSpeed);

            _previousKeyboardState = keyboard;
            return;
        }

        cube.Velocity.Y += Gravity;

        if (cube.Velocity.Y > MaxFallSpeed)
            cube.Velocity.Y = MaxFallSpeed;

        cube.Position.Y += cube.Velocity.Y;

        foreach (var platform in world.Platforms)
        {
            if (cube.Bounds.Intersects(platform))
            {
                if (cube.Velocity.Y > 0)
                {
                    cube.Position.Y = platform.Top - CubeModel.Size;
                    cube.Velocity.Y = 0;
                }
            }
        }

        _previousKeyboardState = keyboard;
    }
}