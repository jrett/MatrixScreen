using System;
using System.Threading.Tasks;

namespace MatrixScreen
{
    /// <summary>
    /// This class will provide the entry point to the application, manage its
    /// lifecycle, as well as provide the timing for, and run, the update/draw loops.
    /// All of the work runs in a single worker thread, allowing the user to exit by pressing any key.
    /// </summary>
    internal class Program
    {
        private long _millisecondsPerFrame = 16; // 16ms = 60fps
        private long _previousFrameTime = 0;

        private ColumnsManager _columnsManager;
        static void Main(string[] args)
        {
            Program instance = new Program();
            instance.StartLoop();
            Console.ReadKey();  // Pressing a key will kill the thread and exit the program.
        }

        private async void StartLoop()
        {
            try
            {
                _columnsManager = new ColumnsManager();
                await Task.Factory.StartNew(RunLoop);
            }
            catch(Exception e)
            {
                Console.WriteLine($"There was an unfortunate crash, luckily nobody was hurt: {e.ToString()}");
            }
        }

        private async Task RunLoop()
        {
            long now;
            long elapsed;

            while (true)
            {
                now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                elapsed = now - _previousFrameTime;
                if (elapsed >= _millisecondsPerFrame)
                {
                    Update(elapsed);
                    Draw(elapsed);
                    _previousFrameTime = now;
                }
                else
                {
                    await Task.Delay(1);
                }
            }
        }

        private void Update(long elapsedTime)
        {
            _columnsManager.Update(elapsedTime);
        }
        private void Draw(long elapsedTime)
        {
            _columnsManager.Draw(elapsedTime);
        }
    }
}


