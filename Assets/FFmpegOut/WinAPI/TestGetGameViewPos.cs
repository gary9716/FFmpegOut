using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestGetGameViewPos : MonoBehaviour {

#if UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(SystemMetric smIndex);
    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
        }
    }
    public enum SystemMetric : int
    {
        SM_CXSCREEN = 0,  // 0x00
        SM_CYSCREEN = 1,  // 0x01
        SM_CXVSCROLL = 2,  // 0x02
        SM_CYHSCROLL = 3,  // 0x03
        SM_CYCAPTION = 4,  // 0x04
        SM_CXBORDER = 5,  // 0x05
        SM_CYBORDER = 6,  // 0x06
                          // [...] shortened ...
    }

    const int whiteBoarderLengthInY = 7;

    public static Vector2 PixelOffsetFromScreen()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        EditorWindow gameview = EditorWindow.GetWindow(T);
        /*    
        System.Reflection.PropertyInfo[] propertyInfo = T.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
        foreach (System.Reflection.PropertyInfo info in propertyInfo)
            Debug.Log("Prop:"+info.Name);
        */

        FieldInfo borderSizeField = T.GetField("kBorderSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
        int borderSize = (int)borderSizeField.GetValue(gameview);
        
        System.Reflection.PropertyInfo RenderRect = T.GetProperty("targetInParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
        Rect ReRe = new Rect();
        if (RenderRect != null)
            ReRe = (Rect)RenderRect.GetValue((object)gameview, null);
        else
            return Vector2.zero;

        Debug.Log(ReRe);

        RECT WinR;
        GetWindowRect(GetActiveWindow(), out WinR);

        Debug.Log(WinR);

        int bw = GetSystemMetrics(SystemMetric.SM_CXBORDER);
        int bh = GetSystemMetrics(SystemMetric.SM_CYBORDER);
        
        // try to retrieve the Window's client rect and its position on screen
        Vector2 Res;
        Res = new Vector2(WinR.Left + ReRe.xMin + bw + borderSize, WinR.Top + ReRe.yMin + bh + whiteBoarderLengthInY);
        return Res;
    }
    
#else
    public static Vector2 PixelOffsetFromScreen()
    {
        return Vector2.zero;
    }

#endif

    // Use this for initialization
    void Start () {
        Debug.Log(PixelOffsetFromScreen());
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
