using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MouseActivitySimulator
{
    class Program
    {
        private static bool _continue = false;
        private static readonly object _lock = new object();

        // Input types
        private const int INPUT_MOUSE = 0;

        // Mouse event constants
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const int _totalMinutes = 60;

        static async Task Main(string[] args)
        {
            lock (_lock)
            {
                _continue = true;
            }
            Console.WriteLine("Mouse activity started.");
            await Task.Run(() => PerformMouseActivity());


            //Console.WriteLine("Enter 'start' to begin mouse activity and 'stop' to end:");
            //while (true)
            //{
            //    var command = Console.ReadLine()?.ToLower();
            //    if (command == "start")
            //    {
            //        lock (_lock)
            //        {
            //            _continue = true;
            //        }
            //        Console.WriteLine("Mouse activity started.");
            //        await Task.Run(() => PerformMouseActivity());
            //    }
            //    else if (command == "stop")
            //    {
            //        lock (_lock)
            //        {
            //            _continue = false;
            //        }
            //        Console.WriteLine("Mouse activity stopped.");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Invalid command. Please enter 'start' or 'stop'.");
            //    }
            //}
        }

        private static void PerformMouseActivity()
        {
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMinutes < _totalMinutes)
            {
                lock (_lock)
                {
                    if (!_continue)
                        break;
                }

                // Move the mouse to (500, 500) and click every 5 seconds
                MouseMove(100, 100);
                MouseClick();
                Console.WriteLine($"Remaining minutes {Math.Round(_totalMinutes - (DateTime.Now - startTime).TotalMinutes)}");

                Thread.Sleep(10000); // Wait for 5 seconds before the next action
            }
        }

        public static void MouseMove(int x, int y)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = MOUSEEVENTF_MOVE
                    }
                }
            };
            SendInput(1, new INPUT[] { input }, INPUT.Size);
        }

        public static void MouseClick()
        {
            var inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_MOUSE,
                    u = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dwFlags = MOUSEEVENTF_LEFTDOWN
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_MOUSE,
                    u = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dwFlags = MOUSEEVENTF_LEFTUP
                        }
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        // Structures and imports for SendInput function
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public InputUnion u;

            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}