namespace PhaseShift.Models;

public class ElectricityModel
{
    public bool Active = false;
    public float Timer = 0f;

    public const float OnTime = 2f;
    public const float OffTime = 4f;
}