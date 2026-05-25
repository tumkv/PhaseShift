using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhaseShift.Models;

namespace PhaseShift.Views;

public class HintView
{
    private Texture2D _controlHintTexture;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _controlHintTexture = content.Load<Texture2D>("controls");
    }

    public void Draw(SpriteBatch spriteBatch, GameWorld world)
    {
        if (!world.Hint.IsVisible)
            return;

        float controlsScale = 0.35f;

        spriteBatch.Draw(
            _controlHintTexture,
            world.Hint.Position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            controlsScale,
            SpriteEffects.None,
            0f);
    }
}