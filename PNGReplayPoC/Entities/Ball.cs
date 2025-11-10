using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PNGReplayPoC;

public struct Ball
{
    public static readonly int Width = 20;
    public static readonly int Speed = 200;
    private static readonly float BounceSpeedUp = 1.2f;
    
    public Vector2 Position;
    public Vector2 Velocity;
    
    public void Update(GameTime gameTime, ref GameState gameState)
    {
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var ballRect = GetRectangle();
        var playerRect = gameState.Player.GetRectangle();
        var leftWall = new Rectangle(-1, 0, 1, gameState.BoardHeight);
        var rightWall = new Rectangle(gameState.BoardWidth, 0, 1, gameState.BoardHeight);
        var topWall = new Rectangle(0, -1, gameState.BoardWidth, 1);
        var bottomWall = new Rectangle(0, gameState.BoardHeight, gameState.BoardWidth, 1);

        if (ballRect.Intersects(leftWall) || ballRect.Intersects(rightWall))
            Velocity.X *= -1;
        
        if (ballRect.Intersects(topWall))
            Velocity.Y *= -1;

        if (ballRect.Intersects(bottomWall))
        {
            gameState.Lost = true;
            return;
        }
        
        if (ballRect.Intersects(playerRect))
        {
            float paddleCenterX = playerRect.X + playerRect.Width / 2f;
            float ballXRelativeToCenter = (gameState.Ball.Position.X - paddleCenterX) / (playerRect.Width / 2f);
            ballXRelativeToCenter = MathHelper.Clamp(ballXRelativeToCenter, -1f, 1f);
            
            float angle = MathHelper.ToRadians(90f - 45f * ballXRelativeToCenter);

            float speed = gameState.Ball.Velocity.Length();
            Velocity.X = speed * (float)Math.Cos(angle);
            Velocity.Y = -speed * (float)Math.Sin(angle);
            
            Position.Y = gameState.Player.Position.Y - ballRect.Height - 1;
            
            SpeedUp();
        }
        

        for (int i = 0; i < gameState.Bricks.Length; i++)
        {
            if (!gameState.Bricks[i].IsAlive) continue;
            
            var brickRect = new Rectangle(gameState.Bricks[i].Position.ToPoint(), new Point(Brick.Width, Brick.Height));
            if (ballRect.Intersects(brickRect))
            {
                gameState.Bricks[i].Break();
                Velocity.Y *= -1;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(Position, Width, Width, Pico8Color.RadioactiveGreen);
    }

    private void SpeedUp()
    {
        Velocity *= BounceSpeedUp;
    }

    private Rectangle GetRectangle()
    {
        return new Rectangle(Position.ToPoint(), new Point(Width, Width));
    }
}