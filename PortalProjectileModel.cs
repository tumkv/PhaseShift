using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class PortalProjectileModel
{
    public Vector2 Position;
    public Vector2 Direction;
    public bool IsBlue;

    public float Lifetime;
    public float MaxLifetime;

    public PortalProjectileModel(Vector2 position, Vector2 direction, bool isBlue)
    {
        Position = position;
        Direction = direction;
        IsBlue = isBlue;

        MaxLifetime = 0.2f;
        Lifetime = MaxLifetime;
    }
}