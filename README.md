A sample project for saving and replaying a frame based on a PNG file. It’s a small (and buggy) breakout game made in C# with MonoGame, where bricks might randomly crash the game when hit by the ball.

## Idea
When a crash occurs (an exception or an assertion fails), save a PNG file containing a screenshot of the game and all the data needed to reproduce that exact crash on another computer. Players or testers could send a single PNG file to the developer, who could then parse it to (ideally) recreate the same error by essentially replaying the simulation step that led to it, making debugging much easier. No need to set up X, Y, or Z conditions, since that comes for “free” from replaying the frame.

This is a screenshot taken before a crash:

![game screenshot](https://github.com/manuf10/pngreplay-poc/blob/main/GameScreenshot.png)

And here is the custom PNG chunk called “gaMe” that contains the game data:
![game screenshot](https://github.com/manuf10/pngreplay-poc/blob/main/hex.jpg)

FYI I got the idea from this podcast about game engine programming https://www.youtube.com/watch?v=mFBmoCv5EcQ

## Requirements

- On exception (or a failed assertion, like “player can’t have HP = 0 and be alive”), you must be able to catch the error and run some code before the process exits (to create the said PNG). In C# I used the AppDomain.CurrentDomain.UnhandledException event and it seems to work. 
Note that some exceptions, like stack overflows, can’t be caught AFAIK, and others might corrupt the game’s memory, potentially making the previously copied game state data invalid, so keep that in mind. In C++ I think the way to go is structured exception handling.
- Always have a copy of the previous frame’s state and the current frame’s inputs, every frame, and be able to serialize/deserialize them in any format you want later. We need the previous frame’s state because we want to reproduce the exact same chain of events that led to the error. Copying the game state every frame will obviously add some overhead, so keep that in mind. It would be useful to make this feature toggleable.
- The game simulation should be deterministic (to increase the chances of reproducing the error). If the game uses a random number generator, then its state should also be serialized somehow, in order to get the same random numbers when we replay. 
Fixed timestep could be necessary as well. If not possible, I guess one could save the delta time used when the error was produced in the PNG, and use that value when we replay?
- Have the ability to read and create PNG files, and to add a “custom” PNG chunk to them to store game data. For this sample I used the Baker76.Pngcs library.
PNG chunks have 4 letter case sensitive ASCII names, and the case of these letters is a bit field that gives the PNG decoders information about the chunk. I chose to use “gaMe” and it worked. You can read more on [Wikipedia](https://en.wikipedia.org/wiki/PNG#%22Chunks%22_within_the_file).

## Other considerations
- The viability will depend on the tools used to make the game, and the type of the game. I’m not really sure how difficult or tedious it would be to implement this in a Unity, Unreal or Godot game.
- You might want to save any additional information that you consider useful, such as the build version, an error message, a stacktrace, etc.
- Some image hosting services (such as Imgur) and tools may strip non-critical data from PNGs. The good news is that I tested sharing over Discord, and it kept the data :), the max file size without Nitro is 10MB though, so Google Drive might be a better option. One could take it a step further and automate the sharing of the file part.

## Some pseudocode

```
function handle_crash()
{
    screenshot = capture_window_screenshot()
    png_data = create_png(screenshot)

    serialized_data = serialize(previous_frame_state, input_state)
    png_custom_chunk_name = "gaMe"
    add_png_chunk(png_custom_chunk_name, serialized_data, png_data)
    
    save_to_disk("screenshot.png", png_data)
    exit()
}

function replay_from_png()
{
    png_data = read_png("screenshot.png")
    png_custom_chunk_name = "gaMe"
    game_data = get_chunk_data(png_custom_chunk_name, png_data)
    restore_state_from_data(game_data, game_state, input_state) <--- must copy the saved game state and input state
}
```

In the frame after loading the game and input state from the PNG, we must skip reading the new input, because we want to use the input we've just restored.
For this game, it doesn’t make any difference because the crash is always caused by the ball’s own movement, not the player.

See
- `QueuePNGReplay(string gameScreenshotPath)`
- `OnUnhandledException(object sender, UnhandledExceptionEventArgs e)`
- `SavePNG(string fileName, string gameStateChunkId = "gaMe")`
- `LoadStateFromPNG(string fileName, string gameStateChunkId = "gaMe")`
- `Update(GameTime gameTime)` to check the game loop.

