using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MidiSynth7.components
{
    internal static class WindowHelper
    {


        private const int SW_RESTORE = 9;
        internal const int WM_CLBUTTONDOWN = 0xA1;
        internal const int HT_CAPTION = 0x2;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hw, int message, int wp, int lp);

        [DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();

        internal static void PostitionWindowOnScreen(Window window, double horizontalShift = 0, double verticalShift = 0)
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
            window.Left = screen.WorkingArea.X + ((screen.WorkingArea.Width - window.ActualWidth) / 2) + horizontalShift;
            window.Top = screen.WorkingArea.Y + ((screen.WorkingArea.Height - window.ActualHeight) / 2) + verticalShift;
        }

        internal static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        internal static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }
    }
}
