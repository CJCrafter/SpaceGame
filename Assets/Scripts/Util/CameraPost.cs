using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class CameraPost : MonoBehaviour {

    public List<Material> materials;

    private void OnRenderImage(RenderTexture source, RenderTexture dest) {
        RenderTexture currentSource = source;

        for (var i = 0; i < materials.Count; i++) {
            if (materials[i] == null)
                throw new Exception("Null texture " + i + ": " + materials);

            RenderTexture currentDest = i == materials.Count - 1 ? dest : RenderTexture.GetTemporary(source.descriptor);
            Graphics.Blit(currentSource, currentDest, materials[i]);

            if (currentSource != source)
                RenderTexture.ReleaseTemporary(currentSource);

            currentSource = currentDest;
        }
        
        if (!materials.Any())
            Graphics.Blit(source, dest);
    }
}