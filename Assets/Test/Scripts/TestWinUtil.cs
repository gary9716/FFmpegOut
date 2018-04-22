using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWinUtil : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        var windows = WindowsUtility.FindWindowsWithText("Unity");

        foreach(var winPtr in windows)
        {
            string name = WindowsUtility.GetWindowText(winPtr);
            Debug.Log(name);
        }

    }
	
}
