using PhaseShift.Managers;
using PhaseShift.Models;
using Microsoft.Xna.Framework;

namespace PhaseShift.Controllers;

public class ButtonDoorController
{
    public void Update(GameWorld world, SoundManager soundManager)
    {
        var buttonDoor = world.ButtonDoor;

        if (!buttonDoor.HasButtonDoorLevel)
            return;

        bool cubePressesButton =
            world.Cube.HasCube &&
            world.Cube.Bounds.Intersects(buttonDoor.Button);

        bool buttonPressed =
            world.Player.Bounds.Intersects(buttonDoor.Button) ||
            cubePressesButton;

        if (buttonPressed && !buttonDoor.ButtonWasPressed)
        {
            soundManager.PlayButtonPress();

            if (!buttonDoor.DoorOpen)
                soundManager.PlayDoorOpen();
        }

        if (!buttonPressed && buttonDoor.ButtonWasPressed)
        {
            soundManager.PlayButtonRelease();
        }

        buttonDoor.ButtonWasPressed = buttonPressed;
        buttonDoor.DoorOpen = buttonPressed;

        if (buttonDoor.DoorPlatformIndex >= 0)
        {
            if (buttonDoor.DoorOpen)
                world.Platforms[buttonDoor.DoorPlatformIndex] = Rectangle.Empty;
            else
                world.Platforms[buttonDoor.DoorPlatformIndex] = buttonDoor.Door;
        }
    }
}