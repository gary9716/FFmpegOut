using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;

public class TestScreenCapturer : MonoBehaviour {

	FFmpegScreenCapturer capturer;

	// Use this for initialization
	void Start () {
		FFmpegScreenCapturer.InputInfo inputInfo = new FFmpegScreenCapturer.InputInfo();
		inputInfo.titleKeywords = new string[]{"Unity", "Standalone"};

		FFmpegScreenCapturer.OutputInfo outputInfo = new FFmpegScreenCapturer.OutputInfo();
		outputInfo.recordCursor = false;
		outputInfo.dest = Application.dataPath + "/../test.mp4";
		Debug.Log("output path:" + outputInfo.dest);

		capturer = new FFmpegScreenCapturer(inputInfo, outputInfo);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.S)) {
			capturer.Close();
		}
	}
}
