using Microsoft.Xna.Framework.Input;
using PhaseShift.Managers;
using PhaseShift.Models;
using System;

namespace PhaseShift.Controllers;

public class PlayerSoundController
{
    private const float FootstepInterval = 0.35f;
    private const float MinLandingSpeed = 3f;
    private const float HighVelocityImpactMinSpeed = 5f;
    private const float ImpactCooldownTime = 0.35f;

    private float _footstepCooldown = 0f;
    private float _impactCooldown = 0f;
    private bool _wasOnGround = false;

    public void UpdateTimers(float deltaTime)
    {
        if (_footstepCooldown > 0f)
            _footstepCooldown -= deltaTime;

        if (_impactCooldown > 0f)
            _impactCooldown -= deltaTime;
    }

    public void UpdateFootsteps(
        GameWorld world,
        KeyboardState keyboard,
        SoundManager soundManager)
    {
        bool isHoldingMoveKey =
            keyboard.IsKeyDown(Keys.A) ||
            keyboard.IsKeyDown(Keys.D);

        bool canStep =
            isHoldingMoveKey &&
            world.Player.IsOnGround &&
            Math.Abs(world.Player.Velocity.X) > 0.3f &&
            !world.Progress.IsFading &&
            !world.Progress.LevelCompletedScreen;

        if (!canStep)
            return;

        if (_footstepCooldown <= 0f)
        {
            soundManager.PlayFootstep();
            _footstepCooldown = FootstepInterval;
        }
    }

    public void UpdateLandingSound(
        GameWorld world,
        float verticalSpeedBeforeCollision,
        SoundManager soundManager)
    {
        bool justLanded =
            !_wasOnGround &&
            world.Player.IsOnGround;

        if (justLanded && verticalSpeedBeforeCollision > MinLandingSpeed)
        {
            soundManager.PlayLanding();
        }

        _wasOnGround = world.Player.IsOnGround;
    }

    public void PlayHighVelocityImpact(
        float impactSpeed,
        SoundManager soundManager)
    {
        if (_impactCooldown > 0f)
            return;

        if (impactSpeed < HighVelocityImpactMinSpeed)
            return;

        soundManager.PlayHighVelocityImpact(impactSpeed);
        _impactCooldown = ImpactCooldownTime;
    }

    public void Reset()
    {
        _footstepCooldown = 0f;
        _impactCooldown = 0f;
        _wasOnGround = false;
    }
}