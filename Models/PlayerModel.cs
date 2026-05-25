using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class PlayerModel
{
    public Vector2 Position;
    public Vector2 Velocity;

    public const int Width = 40;
    public const int Height = 40;

    public bool IsOnGround;
    public bool PreserveMomentum;

    public Rectangle Bounds =>
        new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
}