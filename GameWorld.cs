using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PhaseShift.Models;

public class GameWorld
{
    public PlayerModel Player = new PlayerModel();

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
}