using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class MovingSpikeTrapModel
{
    public bool HasMovingSpikeTrap = false;

    public int PlatformIndex;
    public int SupportIndex;
    public int SpikeStartIndex;
    public int SpikeCount;

    public float Y;
    public int Direction = 1;

    public int X;
    public int Width;
    public int Height;

    public int SupportX;
    public int SupportWidth;

    public int TopY;
    public int BottomY;

    public const float Speed = 45f;
}