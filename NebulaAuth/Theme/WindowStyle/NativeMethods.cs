using System;
using System.Runtime.InteropServices;

namespace NebulaAuth.Theme.WindowStyle;

public static class NativeMethods
{
    public const int WM_NCCALCSIZE = 0x83;
    public const int WM_NCPAINT = 0x85;

    [DllImport("kernel32", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    public static extern bool DwmIsCompositionEnabled();

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    public enum DWMWINDOWATTRIBUTE : uint
    {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }

    private delegate int DwmExtendFrameIntoClientAreaDelegate(IntPtr hwnd, ref MARGINS margins);

    public static int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins)
    {
        var hModule = LoadLibrary("dwmapi");

        if (hModule == IntPtr.Zero)
        {
            return 0;
        }

        var procAddress = GetProcAddress(hModule, "DwmExtendFrameIntoClientArea");

        if (procAddress == IntPtr.Zero)
        {
            return 0;
        }

        var delegateForFunctionPointer = (DwmExtendFrameIntoClientAreaDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DwmExtendFrameIntoClientAreaDelegate));

        return delegateForFunctionPointer(hwnd, ref margins);
    }

    public static bool IsDwmAvailable()
    {
        if (LoadLibrary("dwmapi") == IntPtr.Zero)
        {
            return false;
        }
        return true;
    }

    internal enum WVR
    {
        ALIGNTOP = 0x0010,
        ALIGNLEFT = 0x0020,
        ALIGNBOTTOM = 0x0040,
        ALIGNRIGHT = 0x0080,
        HREDRAW = 0x0100,
        VREDRAW = 0x0200,
        VALIDRECTS = 0x0400,
        REDRAW = HREDRAW | VREDRAW
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
}