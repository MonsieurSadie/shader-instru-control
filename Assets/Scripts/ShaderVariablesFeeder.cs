using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// feeds the shader variables with data coming from audio
public class ShaderVariablesFeeder : MonoBehaviour
{
  ExternAudioListener audioData;
  Material shaderMat;

  public ShaderBindingSet bindingSet;

  void Start()
  {
    audioData = FindObjectOfType<ExternAudioListener>();
    shaderMat = FindObjectOfType<RaymarchRender>().renderMat;
  }

  void Update()
  {
    if(bindingSet == null) return;
    V2();
  }

  void V1()
  {
    for (int i = 0; i < bindingSet.bindings.Length; i++)
    {
      float minFreq   = bindingSet.bindings[i].startFreq;
      float maxFreq   = bindingSet.bindings[i].endFreq;
      int speedIndex  = audioData.max(minFreq, maxFreq);
      float speedFreq = audioData.getBinFreq(speedIndex);
      float speedVal  = audioData.val(speedIndex);

      float shaderVal = shaderMat.GetFloat(bindingSet.bindings[i].variableName);
      float targetVal = map(speedFreq, minFreq,maxFreq, bindingSet.bindings[i].minVal, bindingSet.bindings[i].maxVal);
      if(speedVal < audioData.threshold)
      {
        targetVal = bindingSet.bindings[i].minVal;
      }else
      {
        Debug.LogFormat("max freq in [{3},{4}] is {0} (bin {2}), with value : {1}", speedFreq, speedVal, speedIndex, minFreq, maxFreq);
      }
      targetVal = Mathf.MoveTowards(shaderVal, targetVal, getLerpSpeed(bindingSet.bindings[i].lerpTime,bindingSet.bindings[i].minVal,bindingSet.bindings[i].maxVal)  * Time.deltaTime);
      shaderMat.SetFloat(bindingSet.bindings[i].variableName, targetVal);
    }
  }

  void V2()
  {
    // a shader variable is bound to a band range and the average of that band gives the value of the variable
    for (int i = 0; i < bindingSet.bindings.Length; i++)
    {
      string varName = bindingSet.bindings[i].variableName;

      float shaderVal = 0;

      // exception for color
      Color color = Color.white;
      bool isColor = false;
      int colorChannel = 0;
      if(varName.LastIndexOf('_') > 1) // color
      {
        isColor = true;
        string[] v = varName.Split('_');
        varName = "_"+v[1];
        if(v[2] == "R") colorChannel = 0;
        if(v[2] == "G") colorChannel = 1;
        if(v[2] == "B") colorChannel = 2;
        color = shaderMat.GetColor(varName);
        shaderVal = color[colorChannel];
      }else
      {
        shaderVal = shaderMat.GetFloat(varName);
      }
        
      float audioVal = audioData.getBandRangeAverage(bindingSet.bindings[i].startFreq, bindingSet.bindings[i].endFreq);
      if(audioVal < audioData.threshold) audioVal = 0;
      Debug.LogFormat("val {0} for property {1}", audioVal, varName);

      float targetVal = map(audioVal, 0, 0.15f, bindingSet.bindings[i].minVal, bindingSet.bindings[i].maxVal);
      Debug.LogFormat("mapped val {0} for property {1}", targetVal, varName);
      // targetVal = Mathf.MoveTowards(shaderVal, targetVal, getLerpSpeed(bindingSet.bindings[i].lerpTime,bindingSet.bindings[i].minVal,bindingSet.bindings[i].maxVal)  * Time.deltaTime);
      targetVal = Mathf.MoveTowards(shaderVal, targetVal, getLerpSpeed(bindingSet.bindings[i].lerpTime,bindingSet.bindings[i].minVal,bindingSet.bindings[i].maxVal)  * Time.deltaTime);
      Debug.LogFormat("lerped target val {0} for property {1}", targetVal, varName);

      if(isColor) // color
      {
        color[colorChannel] = targetVal;
        shaderMat.SetColor(varName, color);
      }else
      {
        shaderMat.SetFloat(varName, targetVal);
      }
    }
  }

  float getLerpSpeed(float lerpTime, float minVal, float maxVal)
  {
    if(lerpTime == 0) return Mathf.Infinity;
    return (maxVal-minVal)/lerpTime;
  }

  float map(float val, float min1, float max1, float min2, float max2)
  {
    float t = Mathf.InverseLerp(min1, max1, val);
    return Mathf.Lerp(min2, max2, t);
  }
    
}
