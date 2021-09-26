# MatrixScreen
A C# .NET Core terminal application that displays a terminal of characters in the style of the Matrix movies.

I was inspired by running across a couple of samples of this on YouTube and thought it would be a fun little project.
I also made a video of me writing this so that I can share it with others and maybe even inspire some to take an
interest in programming.

This code uses a single thread to handle the logic and drawing of the screen, while the main thread waits for user
input to exit the program. Concurrency is achieved during the display by implementing a cooperative multitasking scheme,
similar to what you might find in game development.

Another fun excercise might be to convert this concurrency model to use Task Parallel Library (TPL) or something sililar,
where each display column runs in its own thread, instead of cooperatively sharing time.

The drawing implemented in this application relies on ANSI Escape Sequences for both color and positioning of the text.

And though this is a Windows Console application, it does not run in the windows console because windows console does
not support, by default, ANSI escape sequences. It does, however, run fine in the Visual Studio 2019 console by
launching it from within visual studio.

The following links provides useful information about ANSI escape sequences as well as a way to enable support
in windows console.


Microsoft Guide on workign with escape sequences

https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences


Stack overflow article discussing escape sequences in win32

https://stackoverflow.com/questions/16755142/how-to-make-win32-console-recognize-ansi-vt100-escape-sequences


A blog that talks about this issue and has a bit of code to enable windows console to support ANSI escape secuences

https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/


Wikipedia article

https://en.wikipedia.org/wiki/ANSI_escape_code

