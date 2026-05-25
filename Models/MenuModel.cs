using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PhaseShift.Models;

public class MenuModel
{
    public Rectangle NewGameButton = new Rectangle(120, 270, 320, 70);
    public Rectangle ContinueButton = new Rectangle(120, 360, 320, 70);
    public Rectangle SettingsButton = new Rectangle(120, 450, 320, 70);
    public Rectangle ExitButton = new Rectangle(120, 540, 320, 70);

    public Rectangle ResumeButton = new Rectangle(640, 280, 320, 70);
    public Rectangle PauseSettingsButton = new Rectangle(640, 380, 320, 70);
    public Rectangle MainMenuButton = new Rectangle(640, 480, 320, 70);

    public List<Rectangle> LevelButtons = new();

    public bool HoverNewGame;
    public bool HoverContinue;
    public bool HoverSettings;
    public bool HoverExit;

    public bool HoverResume;
    public bool HoverPauseSettings;
    public bool HoverMainMenu;

    public MenuModel()
    {
        for (int i = 0; i < 6; i++)
        {
            LevelButtons.Add(new Rectangle(120 + i * 140, 350, 100, 100));
        }
    }
}