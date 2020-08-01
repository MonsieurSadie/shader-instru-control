using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "shader-bindings-set", menuName = "ScriptableObjects/ShaderBindings", order = 1)]
public class ShaderBindingSet : ScriptableObject
{
  public ShaderBinding[] bindings;
}

[System.Serializable]
public class ShaderBinding
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
