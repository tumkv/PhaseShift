using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class PlayerController
{
    private const float MoveSpeed = 4f;
    private const float Gravity = 0.35f;
    private const float JumpForce = -8f;
    private const float Acceleration = 0.45f;
    private const float Friction = 0.4f;
    private const float AirControl = 0.15f;
    private const float MaxFallSpeed = 16f;

    public void Update(GameWorld world, KeyboardState keyboard)
    {
        var player = world.Player;

        float targetSpeed = 0f;

        if (keyboard.IsKeyDown(Keys.A))
            targetSpeed = -MoveSpeed;

        if (keyboard.IsKeyDown(Keys.D))
            targetSpeed = MoveSpeed;

        if (!player.PreserveMomentum)
        {
            if (targetSpeed != 0)
            {
                float control = player.IsOnGround ? Acceleration : AirControl;

                if (player.Velocity.X < targetSpeed)
                    player.Velocity.X += control;
                else if (player.Velocity.X > targetSpeed)
                    player.Velocity.X -= control;
            }
            else
            {
                if (player.IsOnGround)
                {
                    if (player.Velocity.X > 0)
                        player.Velocity.X -= Friction;
                    else if (player.Velocity.X < 0)
                        player.Velocity.X += Friction;

                    if (Math.Abs(player.Velocity.X) < Friction)
                        player.Velocity.X = 0;
                }
            }
        }
        else
        {
            if (keyboard.IsKeyDown(Keys.A))
                player.Velocity.X -= AirControl;

            if (keyboard.IsKeyDown(Keys.D))
                player.Velocity.X += AirControl;
        }

        if (keyboard.IsKeyDown(Keys.Space) && player.IsOnGround)
        {
            player.Velocity.Y = JumpForce;
            player.IsOnGround = false;
        }

        player.Velocity.Y += Gravity;

        if (player.Velocity.Y > MaxFallSpeed)
            player.Velocity.Y = MaxFallSpeed;

        if (player.Velocity.Y < -MaxFallSpeed)
            player.Velocity.Y = -MaxFallSpeed;
    }
}