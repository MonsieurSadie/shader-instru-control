using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShaderBinding
{
  public string variableName;
  public float startFreq;
  public float endFreq;
  public float minVal;
  public float maxVal;
  public float lerpTime;

  public ShaderBinding(string varName, float startFreq, float endFreq, float minVal, float maxVal, float lerpFactor)
  {
    this.variableName = varName;
    this.startFreq    = startFreq;
    this.endFreq      = endFreq;
    this.minVal       = minVal;
    this.maxVal       = maxVal;
    this.lerpTime     = lerpFactor;
  }
}

// feeds the shader variables with data coming from audio
public class ShaderVariablesFeeder : MonoBehaviour
{
  ExternAudioListener audioData;
  Material shaderMat;

  public ShaderBinding[] shaderBindings;

  void Start()
  {
    audioData = FindObjectOfType<ExternAudioListener>();
    shaderMat = FindObjectOfType<RaymarchRender>().renderMat;
  }

  void Update()
  {
    for (int i = 0; i < shaderBindings.Length; i++)
    {
      float minFreq   = shaderBindings[i].startFreq;
      float maxFreq   = shaderBindings[i].endFreq;
      int speedIndex  = audioData.max(minFreq, maxFreq);
      float speedFreq = audioData.getBinFreq(speedIndex);
      float speedVal  = audioData.val(speedIndex);

      float shaderVal = shaderMat.GetFloat(shaderBindings[i].variableName);
      float targetVal = map(speedFreq, minFreq,maxFreq, shaderBindings[i].minVal, shaderBindings[i].maxVal);
      if(speedVal < audioData.threshold)
      {
        targetVal = shaderBindings[i].minVal;
      }else
      {
        Debug.LogFormat("max freq in [{3},{4}] is {0} (bin {2}), with value : {1}", speedFreq, speedVal, speedIndex, minFreq, maxFreq);
      }
      targetVal = Mathf.MoveTowards(shaderVal, targetVal, getLerpSpeed(shaderBindings[i].lerpTime,shaderBindings[i].minVal,shaderBindings[i].maxVal)  * Time.deltaTime);
      shaderMat.SetFloat(shaderBindings[i].variableName, targetVal);
    }



    // float minFreq = 100;
    // float maxFreq = 600;
    // int speedIndex = audioData.max(minFreq, maxFreq);
    // float speedFreq = audioData.getBinFreq(speedIndex);
    // float speedVal = audioData.val(speedIndex);

    // float shaderVal = shaderMat.GetFloat("_TimeSpeed");
    // float targetVal = map(speedFreq, minFreq,maxFreq, 0.25f, 2 );
    // if(speedVal < audioData.threshold)
    // {
    //   targetVal = 0.25f;
    // }else
    // {
    //   Debug.LogFormat("max freq in [{3},{4}] is {0} (bin {2}), with value : {1}", speedFreq, speedVal, speedIndex, minFreq, maxFreq);
    // }
    // targetVal = Mathf.Lerp(shaderVal, targetVal, valueLerpFactor);
    // shaderMat.SetFloat("_TimeSpeed", targetVal);
  }

  float getLerpSpeed(float lerpTime, float minVal, float maxVal)
  {
    return lerpTime/(maxVal-minVal);
  }

  float map(float val, float min1, float max1, float min2, float max2)
  {
    float t = Mathf.InverseLerp(min1, max1, val);
    return Mathf.Lerp(min2, max2, t);
  }
    
}
