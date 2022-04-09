using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessingCamera : MonoBehaviour {

    public List<Material> materials;

    private void OnRenderImage(RenderTexture source, RenderTexture dest) {
        foreach (Material mat in materials) {
            Graphics.Blit(source, dest, mat);
        }
    }
}
