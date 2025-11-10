using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PNGReplayPoC;

public static class Extensions
{
    private static Texture2D _whiteTexture;
    
    public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 position, int width, int height, Pico8Color color)
    {
        if (_whiteTexture == null)
        {
            _whiteTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _whiteTexture.SetData([Color.White]);
        }
        
        spriteBatch.Draw(_whiteTexture, position, new Rectangle((int)position.X, (int)position.Y, width, height), GetColorFromHex(color) * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
    }
    
    private static Color GetColorFromHex(Pico8Color color)
    {
        uint hex = (uint)color;
        uint reversedHex = (uint)(((hex & 0xFF) << 16) | (hex & 0xFF00) | ((hex >> 16) & 0xFF) | 0xFF << 24);
        
        return new Color(reversedHex);
    }
}