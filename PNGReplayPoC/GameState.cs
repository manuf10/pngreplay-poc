using PNGReplayPoC.RandomNumberGeneration;

namespace PNGReplayPoC;

public struct GameState
{
    public RandomNumberGenerator  Rng;
    public Player                 Player;
    public Ball                   Ball;
    public Brick[]                Bricks;
    public InputState             InputState;
    public int                    BoardWidth;
    public int                    BoardHeight;
    public bool                   Lost;
    
    public GameState Clone(ref GameState copy)
    {
        copy = this;

        for (int i = 0; i < Bricks.Length; i++)
        {
            copy.Bricks[i] = Bricks[i];
        }
        
        return copy;
    }
}

public struct InputState
{
    public bool LeftButtonPressed;
    public bool RightButtonPressed;
}
