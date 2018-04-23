#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Text;

public class WindowsUtility  {

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowText(IntPtr hwnd, string lpString);
    
    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary> Get the text for the window pointed to by hWnd </summary>
    public static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return String.Empty;
    }

    /// <summary> Find all windows that match the given filter </summary>
    /// <param name="filter"> A delegate that returns true for windows
    ///    that should be returned and false for windows that should
    ///    not be returned </param>
    public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
    {
        List<IntPtr> windows = new List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            if (filter(wnd, param))
            {
                // only add the windows that pass the filter
                windows.Add(wnd);
            }

            // but return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary> Find all windows that contain the given title text </summary>
    /// <param name="titleText"> The text that the window title must contain. </param>
    public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
    {
        return FindWindows(delegate (IntPtr wnd, IntPtr param)
        {
            return GetWindowText(wnd).Contains(titleText);
        });
    }

    public static RECT GetWindowBound(IntPtr hwnd) {
        RECT bound;
        if(GetClientRect(hwnd, out bound)) {
            return bound;
        }
        else {
            return default(RECT);
        }
    }

    public static bool SetWindowsTextWrapper(IntPtr hwnd, string text) {
        return SetWindowText(hwnd, text);
    }

}
#endif
