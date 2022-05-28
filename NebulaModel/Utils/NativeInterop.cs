using System;
using System.Runtime.InteropServices;

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
    }
}
