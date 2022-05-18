using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject target;
    public Vector3 offset;
    public Vector3 rotationOffset;
    [Range(0f, 1f)] public float moveSpeed;
    [Range(0f, 1f)] public float rotateSpeed;
    [Min(0f)] public float shakeMultiplier;
    public float spectateSpeed = 0f;

    private Vector3 position;
    private Quaternion rotation;
    private float shakeStart;
    private float shakeDuration;
    private float shakeStrength;

    private void OnValidate() {
        transform.position = GetTargetLocation();
        transform.rotation = GetTargetRotation();
    }

    public void Start() {
        transform.position = position = GetTargetLocation();
        transform.rotation = rotation = GetTargetRotation();
    }
    
    public void FixedUpdate() {
        if (spectateSpeed > 0f) {
            transform.RotateAround(target.transform.position, Vector3.up, spectateSpeed);
            return;
        }
        
        position = Vector3.Lerp(position, GetTargetLocation(), moveSpeed);
        rotation = Quaternion.Lerp(rotation, GetTargetRotation(), rotateSpeed);
        
        // Handle shaking 
        if (shakeDuration > Time.timeSinceLevelLoad - shakeStart) {
            float t = (Time.timeSinceLevelLoad - shakeStart) / shakeDuration;
            float diffuse = Mathf.Exp(-t) * (1f - t);
            Vector3 random = shakeStrength * shakeMultiplier * diffuse * Random.onUnitSphere;

            transform.position = position + random;
        } else {
            transform.position = position;
        }

        transform.rotation = rotation;
    }

    public Vector3 GetTargetLocation() {
        return target.transform.TransformPoint(offset);
    }

    public Quaternion GetTargetRotation() {
        return target.transform.rotation * Quaternion.Euler(rotationOffset);
    }

    public void Shake(float time, float strength = 1f) {
        shakeStart = Time.timeSinceLevelLoad;
        shakeDuration = time;
        shakeStrength = strength;
    }
}