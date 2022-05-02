using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Range {

    [SerializeField] public float min;
    [SerializeField] public float max;

    public Range(float min, float max) {
        this.min = min;
        this.max = max;
    }

    public float Lerp(float t) {
        return t * (max - min) + min;
    }
}

public sealed class BoundsAttribute : PropertyAttribute {
    
    public readonly float min;
    public readonly float max;

    public BoundsAttribute(float min, float max) {
        this.min = min;
        this.max = max;
    }
}

[CustomPropertyDrawer(typeof(BoundsAttribute))]
public class RangeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        BoundsAttribute bounds = attribute as BoundsAttribute;
        var target = property.serializedObject.targetObject;
        Range value = fieldInfo.GetValue(target) as Range;
        
        EditorGUI.MinMaxSlider(position, label, ref value.min, ref value.max, bounds.min, bounds.max);
        
        fieldInfo.SetValue(target, value);
    }
}