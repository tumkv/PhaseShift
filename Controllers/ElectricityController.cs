using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class ElectricityController
{
    public void Update(GameWorld world, float deltaTime)
    {
        var electricity = world.Electricity;

        electricity.Timer += deltaTime;

        if (electricity.Active)
        {
            if (electricity.Timer >= ElectricityModel.OnTime)
            {
                electricity.Timer = 0f;
                electricity.Active = false;
            }
        }
        else
        {
            if (electricity.Timer >= ElectricityModel.OffTime)
            {
                electricity.Timer = 0f;
                electricity.Active = true;
            }
        }
    }
}