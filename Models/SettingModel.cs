using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class SettingsModel
{
    public float MusicVolume = 1f;
    public float SfxVolume = 1f;

    public Rectangle MusicSliderBar = new Rectangle(120, 240, 320, 8);
    public Rectangle SfxSliderBar = new Rectangle(120, 340, 320, 8);

    public bool DraggingMusicSlider = false;
    public bool DraggingSfxSlider = false;
}