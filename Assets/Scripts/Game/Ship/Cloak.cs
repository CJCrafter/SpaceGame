using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Cloak : MonoBehaviour {

    public AudioClip cloakHumClip;
    [Range(0f, 1f)] public float distortion;
    public bool active;
}