using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour {

    public Transform target;
    public Vector3 camOffset;
    public float cameraSpeed;
    public float slerpVal = 0.2f;
    public bool invertY;
    public AnimationCurve mouseSensitivityCurve;

    float yaw;
    float pitch;

    void Start() {
        camOffset = transform.position - target.position;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.L)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        // Hide and lock cursor when right mouse button pressed
        Vector2 mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));
        Vector3 controllerMovement = new Vector3(Input.GetAxis("Joy X"), Input.GetAxis("Joy Y") * (invertY ? 1 : -1));

        float xMov = mouseMovement.x;
        if (Mathf.Abs(controllerMovement.x) > Mathf.Abs(mouseMovement.x)) {
            xMov = controllerMovement.x;
        }

        Quaternion camTurnAngle = Quaternion.AngleAxis(xMov * cameraSpeed, Vector3.up);
        camOffset = camTurnAngle * camOffset;

        Vector3 newPos = target.position + camOffset;
        transform.position = Vector3.Slerp(transform.position, newPos, slerpVal);

        transform.LookAt(target);
    }
}
