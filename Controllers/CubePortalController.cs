using Microsoft.Xna.Framework;
using PhaseShift.Models;

namespace PhaseShift.Controllers;

public class CubePortalController
{
    public void Update(GameWorld world)
    {
        var cube = world.Cube;

        if (!cube.HasCube || cube.IsHolding)
            return;

        if (world.BluePortal == null || world.OrangePortal == null)
        {
            cube.IsTeleporting = false;
            return;
        }

        if (!cube.IsTeleporting)
        {
            if (cube.Bounds.Intersects(world.BluePortal.Bounds))
            {
                cube.Position = GetCubeExitPosition(world.OrangePortal);
                cube.Velocity = world.OrangePortal.ExitDirection * MathHelper.Max(cube.Velocity.Length(), 4f);
                cube.IsTeleporting = true;
            }
            else if (cube.Bounds.Intersects(world.OrangePortal.Bounds))
            {
                cube.Position = GetCubeExitPosition(world.BluePortal);
                cube.Velocity = world.BluePortal.ExitDirection * MathHelper.Max(cube.Velocity.Length(), 4f);
                cube.IsTeleporting = true;
            }
        }

        if (!cube.Bounds.Intersects(world.BluePortal.Bounds) &&
            !cube.Bounds.Intersects(world.OrangePortal.Bounds))
        {
            cube.IsTeleporting = false;
        }
    }

    private Vector2 GetCubeExitPosition(PortalModel portal)
    {
        Vector2 center = new Vector2(
            portal.Bounds.Center.X - CubeModel.Size / 2,
            portal.Bounds.Center.Y - CubeModel.Size / 2);

        return center + portal.ExitDirection * 50f;
    }
}