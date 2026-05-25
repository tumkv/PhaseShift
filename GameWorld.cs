using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PhaseShift.Models;

public class GameWorld
{
    public GameState State = GameState.MainMenu;

    public MenuModel Menu = new MenuModel();
    public SettingsModel Settings = new SettingsModel();

    public bool ShouldExitGame = false;

    public PlayerModel Player = new PlayerModel();

    public CubeModel Cube = new CubeModel();
    public ButtonDoorModel ButtonDoor = new ButtonDoorModel();
    public MovingSpikeTrapModel MovingSpikeTrap = new MovingSpikeTrapModel();
    public ElectricityModel Electricity = new ElectricityModel();

    public List<Rectangle> Platforms = new();
    public List<Rectangle> BackgroundBlocks = new();
    public List<Rectangle> Spikes = new();
    public List<Rectangle> ElectricZones = new();

    public PortalModel BluePortal;
    public PortalModel OrangePortal;

    public List<PortalProjectileModel> Projectiles = new();

    public Rectangle Exit;

    public int CurrentLevel = 1;

    public bool ElectricActive;
    public float ElectricTimer;

    public bool LevelCompletedScreen;
    public int CompletedLevelNumber;
    public float LevelCompleteAlpha;

    public bool IsTeleporting = false;
    public float PortalExitTimer = 0f;
    public float SameDirectionPortalSpeed = 0f;
}