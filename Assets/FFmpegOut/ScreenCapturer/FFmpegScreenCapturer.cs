﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;
using System.Text;

namespace FFmpegOut {
    public class FFmpegScreenCapturer {

        public enum Preset {
            H264Default,
            H264Lossless420,
            H264Lossless444
        }

        public class OutputInfo {
            public string dest;
            public Preset preset;
            public bool recordCursor;
        }

        public class InputInfo {

            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            public string[] titleKeywords;
            
            #elif UNITY_STANDALONE_LINUX
            
            public int audioDeviceIndex;

            #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            
            public int screenDeviceIndex;
            public int audioDeviceIndex;
            
            #endif
        }


        public OutputInfo outputInfo {
            get; private set;
        }

        public InputInfo inputInfo {
            get; private set;
        }

        public string ErrorMsg {
            get; private set;
        }

        private Process subProcess;

        public FFmpegScreenCapturer(InputInfo inputInfo, OutputInfo outputInfo) {
            this.outputInfo = outputInfo;
            this.inputInfo = inputInfo;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("-y"); //overwrite existing file without asking
            
            //https://trac.ffmpeg.org/wiki/Capture/Desktop
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            stringBuilder.Append(" -f dshow -i audio=\"virtual-audio-capturer\":video=\"screen-capture-recorder\""); //one of audio recording plugin that can be used with directshow, you can download from here: https://github.com/rdp/virtual-audio-capture-grabber-device
            #elif UNITY_STANDALONE_LINUX
            stringBuilder.Append(String.Format(" -f alsa -i hw:{0}", outputInfo.audioDeviceIndex)); //audio options(details: https://ffmpeg.org/ffmpeg-devices.html#alsa)
            stringBuilder.Append(String.Format(" -f x11grab -draw_mouse {0} -i :0.0", outputInfo.recordCursor? 1:0)); //video options(details: https://ffmpeg.org/ffmpeg-devices.html#x11grab)
            #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            stringBuilder.Append(String.Format(" -f avfoundation -video_device_index {0} -audio_device_index {1} -capture_cursor {2}", inputInfo.screenDeviceIndex, inputInfo.audioDeviceIndex, outputInfo.recordCursor? 1:0)); //details: https://ffmpeg.org/ffmpeg-devices.html#avfoundation
            #endif 
            
            //the encoding parameters here are specialized for FB live.
            /**
            Live Video Specs
            Video Format

            We accept video in maximum 720p (1280 x 720) resolution, at 30 frames per second. (or 1 key frame every 2 seconds).
            You must send an I-frame (keyframe) at least once every two seconds throughout the stream..
            Recommended max bit rate is 4000 Kbps.
            Titles must be less than 255 characters otherwise the stream will fail.
            The Live API accepts H264 encoded video and AAC encoded audio only.
            Video Length

            240 minute maximum length, with the exception of continuous live (see above).
            240 minute maximum length for preview streams (either through Live dialog or publisher tools). After 240 minutes, a new stream key must be generated.
            Advanced Settings

            Pixel Aspect Ratio: Square.
            Frame Types: Progressive Scan.
            Audio Sample Rate: 44.1 KHz.
            Audio Bitrate: 128 Kbps stereo.
            Bitrate Encoding: CBR.
            */

            //about AAC encoding: https://trac.ffmpeg.org/wiki/Encode/AAC
            //about H264 encoding: https://trac.ffmpeg.org/wiki/Encode/H.264

            stringBuilder.Append(String.Format(" -c:v libx264 -vf scale=w=1280:h=720:force_original_aspect_ratio=decrease -framerate 30 -pix_fmt yuv420p -threads 0")); //video encoding parameters(maybe we can tune part of parameters like pix_fmt and crf to adjust quality)
            stringBuilder.Append(String.Format(" -c:a aac -ar 44100 -b:a 128k")); //audio encoding parameter
            stringBuilder.Append(" -f flv ");
            stringBuilder.Append(outputInfo.dest); //it should be an url with rtmp protocol 

            var opt = stringBuilder.ToString();
            UnityEngine.Debug.Log(opt);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = FFmpegConfig.BinaryPath,

                    // Replace Command line arguments here.
                    Arguments = opt,

                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    
                    // Redirect FFMpeg output.
                    RedirectStandardError = true
                },

                // Get notified when ffmpeg writes to error stream.
                EnableRaisingEvents = true
            };

            // Event handler to receive written data.
            process.ErrorDataReceived += (s, e) => {
                UnityEngine.Debug.Log(e);
            };

            process.Start();

            // Start reading error stream.
            process.BeginErrorReadLine();

            subProcess = process;
        }

        public void Close()
        {
            if (subProcess == null) return;

            subProcess.Close();
            subProcess.Dispose();
            subProcess = null;

            KillAllFFMPEG();

            UnityEngine.Debug.Log("ffmpeg close");
        }
        
        private void KillAllFFMPEG()
        {
            Process killFfmpeg = new Process();
            ProcessStartInfo taskkillStartInfo = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM ffmpeg.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            killFfmpeg.StartInfo = taskkillStartInfo;
            killFfmpeg.Start();
        }
        

    }

}
