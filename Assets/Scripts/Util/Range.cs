using System;
using MK.Glow;
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

public sealed class RangeAttribute : PropertyAttribute {
    
    public float min;
    public float max;

    public RangeAttribute(float min, float max) {
        this.min = min;
        this.max = max;
    }
}

[CustomPropertyDrawer(typeof(RangeAttribute))]
public class RangeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        RangeAttribute range = attribute as RangeAttribute;
        var target = property.serializedObject.targetObject;
        Range value = fieldInfo.GetValue(target) as Range;
        
        EditorGUI.MinMaxSlider(position, label, ref value.min, ref value.max, range.min, range.max);
        
        fieldInfo.SetValue(target, value);
    }
}