using System;
using System.Text;
using System.Text.Json;
using Baker76.Pngcs;
using Baker76.Pngcs.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PNGReplayPoC.RandomNumberGeneration;

namespace PNGReplayPoC;

public class PNGReplayPoC : Game
{
    private static readonly int ScreenWidth = 480;
    private static readonly int ScreenHeight = 480;
    private const int FPS = 60;
    
    private static readonly int BrickRows = 3;
    private static readonly int BrickColumns = 5;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _screenTarget;
    
    public static SpriteFont Font;
    
    private GameState _gameState;
    private GameState _previousGameState;
    
    private bool _useSavedInputNextFrame;
    private bool _loadGameScreenshot;
    private string _loadGameScreenshotPath;
    
    public PNGReplayPoC(string gameScreenshotPath)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / FPS);
        
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        if (!string.IsNullOrEmpty(gameScreenshotPath))
        {
            QueuePNGReplay(gameScreenshotPath);
        }   
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Window.Title = $"WindowSize: {_graphics.GraphicsDevice.Viewport.Width}x{_graphics.GraphicsDevice.Viewport.Height}";
        Font = Content.Load<SpriteFont>("Font");
        
        _screenTarget = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            false, SurfaceFormat.Color, DepthFormat.None);
        
        InitializeGameState();
    }
    
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (_loadGameScreenshot)
        {
            LoadStateFromPNG(_loadGameScreenshotPath);
            _loadGameScreenshot = false;
            _useSavedInputNextFrame = true;
            return;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.R))
        {
            QueuePNGReplay("GameScreenshot.png");
            return;
        }
        
        _gameState.Clone(ref _previousGameState);

        if (_useSavedInputNextFrame)
        {
            _useSavedInputNextFrame = false;
        }
        else
        {
            _gameState.InputState.RightButtonPressed = Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right);
            _gameState.InputState.LeftButtonPressed = Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left);
        }
        
        _gameState.Player.Update(gameTime, _gameState.InputState);
        _gameState.Ball.Update(gameTime, ref _gameState);

        if (_gameState.Lost)
        {
            InitializeGameState();
        }
        
        base.Update(gameTime);
    }
    
    private void QueuePNGReplay(string gameScreenshotPath)
    {
        _loadGameScreenshotPath = gameScreenshotPath;
        _loadGameScreenshot = true;
        _useSavedInputNextFrame = true;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_screenTarget);
        {
            GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin();
            
            foreach (var brick in _gameState.Bricks)
            {
                brick.Draw(_spriteBatch);
            }
            
            _gameState.Player.Draw(_spriteBatch);
            _gameState.Ball.Draw(_spriteBatch);
            
            _spriteBatch.End();
        }

        GraphicsDevice.SetRenderTarget(null);
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_screenTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();   
        }

        base.Draw(gameTime);
    }
    
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // One might want to also save the exception message, stacktrace...
        SavePNG("GameScreenshot.png");
        Environment.Exit(1);
    }
    
    /// Saves a PNG file with the screen's render target as the image data and an extra chunk called gaMe
    private void SavePNG(string fileName, string gameStateChunkId = "gaMe")
    {
        int width = _screenTarget.Width;
        int height = _screenTarget.Height;
        
        var imgInfo = new ImageInfo(width, height, 8, false);
        
        var writer = FileHelper.CreatePngWriter(fileName, imgInfo, true);

        // Write screenshot data
        {
            var pixels = new Color[width * height];
            _screenTarget.GetData(pixels);
            
            var line = new ImageLine(imgInfo);
            for (int y = 0; y < height; y++)
            {

                for (int x = 0; x < width; x++)
                {
                    var c = pixels[y * width + x];
                    int i = x * 3;
                    line.Scanline[i] = c.R;
                    line.Scanline[i + 1] = c.G;
                    line.Scanline[i + 2] = c.B;
                }

                writer.WriteRow(line, y);
            }   
        }
        
        // Write previous frame's game state, and input state
        try
        {
            string gameStateJson = JsonSerializer.Serialize(_previousGameState, new JsonSerializerOptions{ IncludeFields = true });
            byte[] bytes = Encoding.UTF8.GetBytes(gameStateJson);
            var customChunk = new PngChunkUNKNOWN(gameStateChunkId, writer.ImgInfo);
            customChunk.SetData(bytes);
            writer.GetChunksList().Queue(customChunk);
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
            
        writer.End();
    }

    private void LoadStateFromPNG(string fileName, string gameStateChunkId = "gaMe")
    {
        try
        {
            var reader = FileHelper.CreatePngReader(fileName);
            reader.ReadSkippingAllRows();

            var chunks = reader.GetChunksList().GetChunks();
            foreach (var chunk in chunks)
            {
                if (chunk is PngChunkUNKNOWN u && chunk.Id == gameStateChunkId)
                {
                    var json = Encoding.UTF8.GetString(u.GetData());
                    _gameState = JsonSerializer.Deserialize<GameState>(json, new JsonSerializerOptions{ IncludeFields = true });
                }
            }
        
            reader.End();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private void InitializeGameState()
    {
        int seed = (int)DateTime.Now.Ticks;
        _gameState.Rng = new RandomNumberGenerator(seed);

        _gameState.BoardWidth = ScreenWidth;
        _gameState.BoardHeight = ScreenHeight;
        _gameState.Lost = false;
        _gameState.Player = new Player
        {
            Position = new Vector2((float)(ScreenWidth - Player.Width) / 2, 450.0f)
        };
        
        _gameState.Bricks = new Brick[BrickRows * BrickColumns];
        for (int row = 0; row < BrickRows; row++)
        {
            for (int column = 0; column < BrickColumns; column++)
            {
                int xPadding = (column + 1) * 4;
                int yPadding = (row + 1) * 4;
                _gameState.Bricks[row * BrickColumns + column] = new Brick
                {
                    Position = new Vector2(column * Brick.Width + xPadding + 28, row * Brick.Height + yPadding + 26),
                    IsAlive = true,
                    Color = GetRandomBrickColor(_gameState.Rng.Next()),
                    IsCrash = _gameState.Rng.NextBool()
                };
            }
        }

        _gameState.Ball = new Ball
        {
            Position = new Vector2((float)(ScreenWidth - Ball.Width) / 2, 150.0f),
            Velocity = new Vector2(-Ball.Speed, Ball.Speed)
        };
    }
    
    private Pico8Color GetRandomBrickColor(int random)
    {
        var values = Enum.GetValues(typeof(Pico8Color));
        return (Pico8Color)values.GetValue(random % (values.Length - 1));
    }
}