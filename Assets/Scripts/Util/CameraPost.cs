using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class CameraPost : MonoBehaviour {

    public List<Material> materials;
    public List<ICustomPostEffect> effects;

    public interface ICustomPostEffect {

        public abstract Material GetMaterial();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture dest) {
        if (materials == null)
            materials = new List<Material>();
        if (effects == null)
            effects = new List<ICustomPostEffect>();

        if (materials.Any()) {
            foreach (var mat in materials) {
                //new AtmosphereSettings().Apply(mat, 2.0f);
                Graphics.Blit(source, dest, mat);
            }
        }

        if (effects.Any()) {
            foreach (var effect in effects) {
                Graphics.Blit(source, dest, effect.GetMaterial());
            }
        }

        if (!materials.Any() && !effects.Any()) {
            Graphics.Blit(source, dest);
        }
    }
}