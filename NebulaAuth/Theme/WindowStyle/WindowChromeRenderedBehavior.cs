using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using Microsoft.Xaml.Behaviors;

namespace NebulaAuth.Theme.WindowStyle;

public class WindowChromeRenderedBehavior : Behavior<Window>
{
    private Window window;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ContentRendered += OnContentRendered;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.ContentRendered -= OnContentRendered;
    }

    private void OnContentRendered(object sender, EventArgs e)
    {
        window = Window.GetWindow(AssociatedObject);

        if (window == null) return;

        var oldWindowChrome = WindowChrome.GetWindowChrome(window);

        if (oldWindowChrome == null) return;

        var newWindowChrome = new WindowChrome
        {
            CaptionHeight = oldWindowChrome.CaptionHeight,
            CornerRadius = oldWindowChrome.CornerRadius,
            GlassFrameThickness = new Thickness(0, 0, 0, 1),
            NonClientFrameEdges = NonClientFrameEdges.Bottom,
            ResizeBorderThickness = oldWindowChrome.ResizeBorderThickness,
            UseAeroCaptionButtons = oldWindowChrome.UseAeroCaptionButtons
        };

        WindowChrome.SetWindowChrome(window, newWindowChrome);

        var hWnd = new WindowInteropHelper(window).Handle;
        HwndSource.FromHwnd(hWnd)?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NativeMethods.WM_NCPAINT:
                handled = false;
                RemoveFrame();
                break;
            case NativeMethods.WM_NCCALCSIZE:
                handled = true;
                var rcClientArea = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                rcClientArea.Bottom += (int)(WindowChromeHelper.WindowResizeBorderThickness.Bottom / 2);
                Marshal.StructureToPtr(rcClientArea, lParam, false);

                return wParam == new IntPtr(1) ? new IntPtr((int)NativeMethods.WVR.REDRAW) : IntPtr.Zero;
        }

        return IntPtr.Zero;
    }

    private void RemoveFrame()
    {
        if (Environment.OSVersion.Version.Major >= 6 && NativeMethods.IsDwmAvailable())
        {
            if (NativeMethods.DwmIsCompositionEnabled() && SystemParameters.DropShadow)
            {
                // to get the aero shadow back, margins have to be set on at least one side
                // don't use negative values for the margins because it causes flickering when restoring or maximizing the window
                // setting the margin on the left or top sides seem best, since the window never appears to flicker there on resizing
                // but the right and bottom sometimes do, otherwise there will occasionally be white flicker when resizing the window.
                NativeMethods.MARGINS margins;

                margins.bottomHeight = 0;
                margins.leftWidth = 1;
                margins.rightWidth = 0;
                margins.topHeight = 0;

                var helper = new WindowInteropHelper(window);

                NativeMethods.DwmExtendFrameIntoClientArea(helper.Handle, ref margins);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public static RECT Empty;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}