using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class CrosshairView
{
    private Texture2D _crosshairNoPortals;
    private Texture2D _crosshairBlueOnly;
    private Texture2D _crosshairOrangeOnly;
    private Texture2D _crosshairBothPortals;

    public void LoadContent(ContentManager content)
    {
        _crosshairNoPortals = content.Load<Texture2D>("crosshair_none");
        _crosshairBlueOnly = content.Load<Texture2D>("crosshair_blue");
        _crosshairOrangeOnly = content.Load<Texture2D>("crosshair_orange");
        _crosshairBothPortals = content.Load<Texture2D>("crosshair_both");
    }

    public void Draw(SpriteBatch spriteBatch, GameWorld world)
    {
        if (world.State != GameState.Playing)
            return;

        Texture2D texture = GetCurrentCrosshair(world);

        MouseState mouse = Mouse.GetState();

        float crosshairScale = 0.2f;

        Vector2 position = new Vector2(
            mouse.X - (texture.Width * crosshairScale) / 2f,
            mouse.Y - (texture.Height * crosshairScale) / 2f);

        spriteBatch.Draw(
            texture,
            position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            crosshairScale,
            SpriteEffects.None,
            0f);
    }

    private Texture2D GetCurrentCrosshair(GameWorld world)
    {
        bool hasBlue = world.BluePortal != null;
        bool hasOrange = world.OrangePortal != null;

        if (hasBlue && hasOrange)
            return _crosshairBothPortals;

        if (hasBlue)
            return _crosshairBlueOnly;

        if (hasOrange)
            return _crosshairOrangeOnly;

        return _crosshairNoPortals;
    }
}