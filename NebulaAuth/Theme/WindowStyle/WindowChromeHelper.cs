using System;
using System.Runtime.InteropServices;
using System.Windows;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace NebulaAuth.Theme.WindowStyle;

public static class WindowChromeHelper
{
    public static Thickness LayoutOffsetThickness => new(0d, 0d, 0d, SystemParameters.WindowResizeBorderThickness.Bottom);

    /// <summary>
    /// Gets the properly adjusted window resize border thickness from system parameters.
    /// </summary>
    public static Thickness WindowResizeBorderThickness
    {
        get
        {
            var dpix = GetDpi(GetDeviceCapsIndex.LOGPIXELSX);
            var dpiy = GetDpi(GetDeviceCapsIndex.LOGPIXELSY);

            var dx = GetSystemMetrics(GetSystemMetricsIndex.CXFRAME);
            var dy = GetSystemMetrics(GetSystemMetricsIndex.CYFRAME);

            // This adjustment is needed since .NET 4.5 
            var d = GetSystemMetrics(GetSystemMetricsIndex.SM_CXPADDEDBORDER);
            dx += d;
            dy += d;

            var leftBorder = dx / dpix;
            var topBorder = dy / dpiy;

            return new Thickness(leftBorder, topBorder, leftBorder, topBorder);
        }
    }

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private static float GetDpi(GetDeviceCapsIndex index)
    {
        var desktopWnd = IntPtr.Zero;
        var dc = GetDC(desktopWnd);
        float dpi;
        try
        {
            dpi = GetDeviceCaps(dc, (int)index);
        }
        finally
        {
            ReleaseDC(desktopWnd, dc);
        }
        return dpi / 96f;
    }

    private enum GetDeviceCapsIndex
    {
        LOGPIXELSX = 88,
        LOGPIXELSY = 90
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(GetSystemMetricsIndex nIndex);

    private enum GetSystemMetricsIndex
    {
        CXFRAME = 32,
        CYFRAME = 33,
        SM_CXPADDEDBORDER = 92
    }
}