using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class ButtonDoorModel
{
    public bool HasButtonDoorLevel = false;

    public Rectangle Button;
    public Rectangle Door;

    public bool DoorOpen = false;
    public bool ButtonWasPressed = false;

    public int DoorPlatformIndex = -1;
}