using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ExternAudioListener : MonoBehaviour
{
  AudioSource source;
  AudioClip microphoneClip;

  const int numFreqBands = 512;
  float[] spectrumData = new float[numFreqBands];
  public float barMaxHeight = 300;

  void Awake()
  {
    source = GetComponent<AudioSource>();
    string[] devices = Microphone.devices;
    if(devices.Length == 0)
    {
      Debug.LogWarning("no microphone found");
    }

    Debug.LogFormat("selecting microphone {0}", devices[0]);
    microphoneClip = Microphone.Start(devices[0], true, 10, 44100);
    source.clip = microphoneClip;
    source.Play();
  }

  void Update()
  {
    source.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
  }

  void OnGUI()
  {
    int screenW = Screen.width;
    int halfLength = spectrumData.Length / 4;
    int freqBarWidth = screenW / halfLength;

    for (int i = 0; i < halfLength; i++)
    {
      int freqBarHeight = (int)(spectrumData[i] * barMaxHeight);
      Rect rect = new Rect(i*freqBarWidth, Screen.height - freqBarHeight, freqBarWidth, freqBarHeight);
      GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }
  }
}
