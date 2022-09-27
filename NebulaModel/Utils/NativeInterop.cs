using System;
using System.Threading;
using System.Runtime.InteropServices;
using BepInEx;

namespace NebulaModel.Utils
{
    public class NativeInterop
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        public static void HideWindow()
        {
            ShowWindow(GetActiveWindow(), SW_HIDE);
        }


        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(CtrlHandler handler, bool add);

        private delegate bool CtrlHandler(CtrlType sig);

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine($"Exiting app due to {sig}");

            switch(sig)
            {
                // Only CTRL_C, CTRL_BREAK events have no timeout
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_BREAK_EVENT:
                    Console.WriteLine($"Start saving to last exit...");
                    ManualResetEvent mre = new(false);
                    ThreadingHelper.Instance.StartSyncInvoke(() => {
                        UIRoot.instance.uiGame.escMenu.OnButton6Click(); //ESC Menu - ExitProgram;
                        mre.Set();
                    });
                    mre.WaitOne();
                    Console.WriteLine($"Saving completed!");
                    Thread.Sleep(1000);
                    return false;
            }
            Thread.Sleep(500);
            return false;
        }

        public static void SetConsoleCtrlHandler()
        {
            // if the handler is no longer static, it can get GC'd because nothing is keeping a reference to the delegate.  
            bool result = SetConsoleCtrlHandler(new CtrlHandler(Handler), true);
            Console.WriteLine("SetConsoleCtrlHandler: " + (result ? "Success" : "Fail"));
        }
    }
}
