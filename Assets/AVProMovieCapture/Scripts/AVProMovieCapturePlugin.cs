using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2014 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

public class AVProMovieCapturePlugin
{
	public enum PixelFormat
	{
		RGBA32,
		BGRA32,				// Note: This is the native format for Unity textures with red and blue swapped.
		YCbCr422_YUY2,
		YCbCr422_UYVY,
		YCbCr422_HDYC,
	}

	// Used by GL.IssuePluginEvent
	public const int PluginID = 0xFA30000;
	public enum PluginEvent
	{
		CaptureFrameBuffer = 0,
	}	

	//////////////////////////////////////////////////////////////////////////
	// Global Init/Deinit
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool Init();

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void Deinit();

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern float GetPluginVersion();

	//////////////////////////////////////////////////////////////////////////
	// Video Codecs
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern int GetNumAVIVideoCodecs();

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool IsConfigureVideoCodecSupported(int index);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif	
	public static extern void ConfigureVideoCodec(int index);
	
	public static string GetAVIVideoCodecName(int index)
	{
		string result = "Invalid";
		StringBuilder nameBuffer = new StringBuilder(256);
		if (GetAVIVideoCodecName(index, nameBuffer, nameBuffer.Capacity))
		{
			result = nameBuffer.ToString();
		}
		return result;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// Audio Codecs
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern int GetNumAVIAudioCodecs();
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif	
	public static extern bool IsConfigureAudioCodecSupported(int index);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void ConfigureAudioCodec(int index);

	public static string GetAVIAudioCodecName(int index)
	{
		string result = "Invalid";
		StringBuilder nameBuffer = new StringBuilder(256);
		if (GetAVIAudioCodecName(index, nameBuffer, nameBuffer.Capacity))
		{
			result = nameBuffer.ToString();
		}
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// Audio Devices

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern int GetNumAVIAudioInputDevices();

	public static string GetAVIAudioInputDeviceName(int index)
	{
		string result = "Invalid";
		StringBuilder nameBuffer = new StringBuilder(256);
		if (GetAVIAudioInputDeviceName(index, nameBuffer, nameBuffer.Capacity))
		{
			result = nameBuffer.ToString();
		}
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the recorder
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern int CreateRecorderAVI([MarshalAs(UnmanagedType.LPWStr)] string filename, uint width, uint height, int frameRate, int format, 
											bool isTopDown, int videoCodecIndex, bool hasAudio, int audioInputDeviceIndex, int audioCodecIndex, bool isRealTime);

	//////////////////////////////////////////////////////////////////////////
	// Update recorder

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void Start(int handle);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool IsNewFrameDue(int handle);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool EncodeFrame(int handle, System.IntPtr data);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool EncodeAudio(int handle, System.IntPtr data, uint length);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern bool EncodeFrameWithAudio(int handle, System.IntPtr videoData, System.IntPtr audioData, uint audioLength);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void Pause(int handle);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void Stop(int handle);
	
	//////////////////////////////////////////////////////////////////////////
	// Destroy recorder
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern void FreeRecorder(int handle);
	
	//////////////////////////////////////////////////////////////////////////
	// Debugging
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern uint GetNumDroppedFrames(int handle);
	
#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern uint GetNumDroppedEncoderFrames(int handle);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern uint GetNumEncodedFrames(int handle);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	public static extern uint GetEncodedSeconds(int handle);

	//////////////////////////////////////////////////////////////////////////
	// Private internal functions

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	private static extern bool GetAVIVideoCodecName(int index, StringBuilder name, int nameBufferLength);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	private static extern bool GetAVIAudioCodecName(int index, StringBuilder name, int nameBufferLength);

#if UNITY_64 && !UNITY_EDITOR
	[DllImport("AVProMovieCapture64")]
#else
	[DllImport("AVProMovieCapture")]
#endif
	private static extern bool GetAVIAudioInputDeviceName(int index, StringBuilder name, int nameBufferLength);
}