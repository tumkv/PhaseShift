using Microsoft.Xna.Framework;

namespace PhaseShift.Models;

public class PortalModel
{
    public Rectangle Bounds;
    public Vector2 ExitDirection;

    public PortalModel(Rectangle bounds, Vector2 exitDirection)
    {
        Bounds = bounds;
        ExitDirection = exitDirection;
    }
}