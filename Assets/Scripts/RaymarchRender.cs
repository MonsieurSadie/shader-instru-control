using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymarchRender : MonoBehaviour
{
  public Material renderMat;

  void Awake()
  {
    renderMat = new Material(renderMat);
  }

  void OnDestroy()
  {
    Destroy(renderMat);
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    Graphics.Blit(source, destination, renderMat);
  }
}
