using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PNGReplayPoC;

public struct Player
{
    private const int Speed = 300;
    public static readonly int Width = 100;
    public static readonly int Height = 20;
    
    public Vector2 Position;

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(Position, Width, Height, Pico8Color.Whiter);    
    }

    public void Update(GameTime gameTime, InputState inputState)
    {
        if (inputState.RightButtonPressed)
            Position.X += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        else if (inputState.LeftButtonPressed)
            Position.X -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public Rectangle GetRectangle()
    {
        return new Rectangle(Position.ToPoint(), new Point(Width, Height));
    }
}