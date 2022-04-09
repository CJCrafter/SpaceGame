using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HealthbarTemp : MonoBehaviour {

    public Gradient gradient;
    public Image image;
    public Slider health;

    private void Update() {
        image.color = gradient.Evaluate(health.value);
    }

}
