using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class CubeModel
{
    public Vector2 Position = new Vector2(300, 760);
    public Vector2 Velocity = Vector2.Zero;

    public const int Size = 40;
    public const float HoldDistance = 90f;
    public const float FollowSpeed = 0.25f;

    public bool HasCube = false;
    public bool IsHolding = false;
    public bool IsTeleporting = false;

    public Rectangle Bounds =>
        new Rectangle((int)Position.X, (int)Position.Y, Size, Size);
}