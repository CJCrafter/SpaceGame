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
        GUIContent serialized = EditorGUI.BeginProperty(position, label, property);
        
        BoundsAttribute bounds = attribute as BoundsAttribute;
        var target = property.serializedObject.targetObject;
        Range value = fieldInfo.GetValue(target) as Range;

        // Determine the widths of the label, minField, slider, and maxField (in that order)
        int controlId = GUIUtility.GetControlID("EditorMinMaxSlider".GetHashCode(), FocusType.Passive);
        Rect remaining = EditorGUI.PrefixLabel(position, controlId, label);
        Rect b = new Rect(remaining.x, remaining.y, remaining.width * 0.25f, remaining.height);
        Rect c = new Rect(b.xMax, remaining.y, remaining.width * 0.50f, remaining.height);
        Rect d = new Rect(c.xMax, remaining.y, remaining.width * 0.25f, remaining.height);

        // Actually show the fields
        value.min = EditorGUI.FloatField(b, value.min);
        EditorGUI.MinMaxSlider(c, ref value.min, ref value.max, bounds.min, bounds.max);
        value.max = EditorGUI.FloatField(d, value.max);
        
        // Apply the min-max bounds to make sure the user cannot slide past
        // the boundaries. 
        value.min = Mathf.Max(value.min, bounds.min);
        value.max = Mathf.Min(value.max, bounds.max);
        
        fieldInfo.SetValue(target, value);
        EditorGUI.EndProperty();
    }
}