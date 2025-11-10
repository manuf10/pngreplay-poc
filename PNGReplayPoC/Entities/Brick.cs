using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PNGReplayPoC;

public struct Brick
{
    public static int Width = 80;
    public static int Height = 24;
    
    public Vector2 Position { get; set; }
    public Pico8Color Color { get; set; }
    public bool IsAlive { get; set; }
    public bool IsCrash { get; set; }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive) return;
        
        spriteBatch.DrawRectangle(Position, Width, Height, Color);
        
        if (IsCrash)
            spriteBatch.DrawString(PNGReplayPoC.Font, "CRASH", new Vector2(Position.X + 8, Position.Y),
                Microsoft.Xna.Framework.Color.Black, 0f, Vector2.Zero, 0.45f, SpriteEffects.None, 0f);
    }
    
    public void Break()
    {
        if (IsCrash)
        {
            throw new Exception("Whoops, the game crashed.");
        }
        
        IsAlive = false;
    }
}