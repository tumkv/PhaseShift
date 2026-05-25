using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace PhaseShift.Managers;

public class SoundManager
{
    private readonly Random _random = new Random();

    private Song _backgroundMusic;

    private SoundEffect _bluePortalShootSound;
    private SoundEffect _orangePortalShootSound;
    private SoundEffect _bluePortalOpenSound;
    private SoundEffect _orangePortalOpenSound;
    private SoundEffect _portalCloseSound;

    private SoundEffect _portalEnterSound;
    private SoundEffect _portalExitSound;

    private SoundEffect _cubeHoldSound;
    private SoundEffectInstance _cubeHoldInstance;
    private SoundEffect _cubeFailSound;

    private SoundEffect _buttonPressSound;
    private SoundEffect _buttonReleaseSound;

    private SoundEffect _doorOpenSound;

    private SoundEffect _movingTrapSound;
    private SoundEffectInstance _movingTrapInstance;

    private readonly List<SoundEffect> _footstepSounds = new();
    private readonly List<SoundEffect> _landingSounds = new();
    private readonly List<SoundEffect> _highVelocityImpactSounds = new();
    private readonly List<SoundEffect> _portalInvalidSurfaceSounds = new();

    private const float MovingTrapVolume = 0.25f;
    private const float PortalInvalidSurfaceVolume = 0.7f;
    private const float HighVelocityImpactVolume = 0.55f;

    public float MusicVolume { get; set; } = 1f;
    public float SfxVolume { get; set; } = 1f;

    public void LoadContent(ContentManager content)
    {
        _backgroundMusic = content.Load<Song>("background_music");

        _bluePortalShootSound = content.Load<SoundEffect>("blue_shoot");
        _orangePortalShootSound = content.Load<SoundEffect>("orange_shoot");

        _bluePortalOpenSound = content.Load<SoundEffect>("blue_open");
        _orangePortalOpenSound = content.Load<SoundEffect>("orange_open");
        _portalCloseSound = content.Load<SoundEffect>("portal_close");

        _portalEnterSound = content.Load<SoundEffect>("portal_enter");
        _portalExitSound = content.Load<SoundEffect>("portal_exit");

        _cubeHoldSound = content.Load<SoundEffect>("cube_hold");
        _cubeHoldInstance = _cubeHoldSound.CreateInstance();
        _cubeHoldInstance.IsLooped = true;

        _cubeFailSound = content.Load<SoundEffect>("cube_fail");

        _buttonPressSound = content.Load<SoundEffect>("button_press");
        _buttonReleaseSound = content.Load<SoundEffect>("button_release");

        _doorOpenSound = content.Load<SoundEffect>("door_open");

        _movingTrapSound = content.Load<SoundEffect>("moving_trap");
        _movingTrapInstance = _movingTrapSound.CreateInstance();
        _movingTrapInstance.IsLooped = true;

        for (int i = 2; i <= 9; i++)
            _footstepSounds.Add(content.Load<SoundEffect>($"footstep{i}"));

        for (int i = 2; i <= 9; i++)
            _landingSounds.Add(content.Load<SoundEffect>($"land{i}"));

        for (int i = 1; i <= 4; i++)
            _highVelocityImpactSounds.Add(content.Load<SoundEffect>($"highvelocity_impact{i}"));

        for (int i = 1; i <= 2; i++)
            _portalInvalidSurfaceSounds.Add(content.Load<SoundEffect>($"portalinvalidsurface{i}"));

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = MusicVolume;
        MediaPlayer.Play(_backgroundMusic);
    }

    public void UpdateVolumes()
    {
        MediaPlayer.Volume = MusicVolume;

        if (_movingTrapInstance != null)
            _movingTrapInstance.Volume = SfxVolume * MovingTrapVolume;

        if (_cubeHoldInstance != null)
            _cubeHoldInstance.Volume = SfxVolume;
    }

    private void PlayRandomSound(List<SoundEffect> sounds, float volumeMultiplier = 1f)
    {
        if (sounds.Count == 0)
            return;

        int index = _random.Next(sounds.Count);

        float finalVolume = Math.Clamp(SfxVolume * volumeMultiplier, 0f, 1f);

        sounds[index].Play(finalVolume, 0f, 0f);
    }

    public void PlayFootstep()
    {
        PlayRandomSound(_footstepSounds, 1f);
    }

    public void PlayLanding()
    {
        PlayRandomSound(_landingSounds, 1f);
    }

    public void PlayHighVelocityImpact(float impactSpeed)
    {
        float speedVolume = Math.Clamp(impactSpeed / 16f, 0.4f, 1f);
        PlayRandomSound(_highVelocityImpactSounds, HighVelocityImpactVolume * speedVolume);
    }

    public void PlayPortalInvalidSurface()
    {
        PlayRandomSound(_portalInvalidSurfaceSounds, PortalInvalidSurfaceVolume);
    }

    public void PlayBluePortalShoot()
    {
        _bluePortalShootSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayOrangePortalShoot()
    {
        _orangePortalShootSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayBluePortalOpen()
    {
        _bluePortalOpenSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayOrangePortalOpen()
    {
        _orangePortalOpenSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayPortalClose()
    {
        _portalCloseSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayPortalEnter()
    {
        _portalEnterSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayPortalExit()
    {
        _portalExitSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayCubeFail()
    {
        _cubeFailSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayCubeHold()
    {
        _cubeHoldInstance.Volume = SfxVolume;
        _cubeHoldInstance.Play();
    }

    public void StopCubeHold()
    {
        _cubeHoldInstance.Stop();
    }

    public void PlayButtonPress()
    {
        _buttonPressSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayButtonRelease()
    {
        _buttonReleaseSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayDoorOpen()
    {
        _doorOpenSound.Play(SfxVolume, 0f, 0f);
    }

    public void PlayMovingTrap()
    {
        if (_movingTrapInstance == null)
            return;

        if (_movingTrapInstance.State != SoundState.Playing)
        {
            _movingTrapInstance.Volume = SfxVolume * MovingTrapVolume;
            _movingTrapInstance.Play();
        }
    }

    public void StopMovingTrap()
    {
        if (_movingTrapInstance == null)
            return;

        if (_movingTrapInstance.State == SoundState.Playing)
            _movingTrapInstance.Stop();
    }
}