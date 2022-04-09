using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMover : MonoBehaviour
{
    public class Position {
        public Vector3 position;
        public Quaternion rotation;
    }

    [Range(0f, 10f)]
    public float dragSpeed = 2f;

    [HideInInspector]
    public List<Position> positions;
    private Vector3 drag;

    private void Update() {

        if (Input.GetMouseButtonDown(1)) {
            drag = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - drag);

        Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

        transform.Translate(move, Space.World);
    }
}
