using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ExternAudioListener : MonoBehaviour
{
  AudioSource source;
  AudioClip microphoneClip;

  const int numFreqBands = 2048;
  float bandwidth;
  float[] spectrumData = new float[numFreqBands];
  public float barMaxHeight = 300;
  public float threshold = 0.5f;

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

    // compute width of each band (in Hz)
    bandwidth = 44100.0f / numFreqBands;
    Debug.LogFormat("bandwidth is {0}", bandwidth);
  }

  void Update()
  {
    source.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
  }

  // given a band interval (in Hz), returns the index of the band with max value inside it
  public int max(float min, float max)
  {
    int minIndex = Mathf.FloorToInt(min/bandwidth);
    int maxIndex = Mathf.FloorToInt(max/bandwidth);
    // Debug.LogFormat("looking for max in [{0}-{1}]", minIndex, maxIndex);
    float maxval = spectrumData[minIndex];
    int maxIndx = 0;
    for (int i = minIndex+1; i < maxIndex; i++)
    {
      if(spectrumData[i] > maxval)
      {
        maxval = spectrumData[i];
        maxIndx = i;
      }
    } 
    return maxIndx;
  }

  public float val(int binIndex)
  {
    return spectrumData[binIndex];
  }

  public float getBinFreq(int binIdx)
  {
    return binIdx * bandwidth;
  }

  public float getBandRangeAverage(float startBand, float endBand)
  {
    int startIndex = Mathf.FloorToInt(startBand/bandwidth);
    int endIndex = Mathf.FloorToInt(endBand/bandwidth);
    float val = 0;
    for (int i = startIndex; i < endIndex; i++)
    {
      val += spectrumData[i];
    }
    val /= (endIndex-startIndex);

    return val;
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

    int thresholdHeight = (int)(threshold * barMaxHeight);
    Rect r = new Rect(0, Screen.height-thresholdHeight, Screen.width, 5);
    GUI.DrawTexture(r, Texture2D.whiteTexture);

    GUI.skin.label.fontSize = 30;
    GUI.skin.label.fontStyle = FontStyle.Bold;
    GUI.contentColor = Color.yellow;
    GUILayout.BeginHorizontal();
    GUILayout.Label(getBinFreq(max(0,22000)).ToString("0.0"));
    GUILayout.Label(" : " + val(max(0,22000)).ToString("0.000"));
    GUILayout.EndHorizontal();
  }
}
