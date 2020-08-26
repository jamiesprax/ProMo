using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
    public float speed;

    private float horizontal, vertical;
    private float mouseX, mouseY;
    private Camera cam;
    private float jumpPower;

    void Awake() {
        cam = GetComponentInChildren<Camera>();
    }

    private void Start() {
    }


    // Update is called once per frame
    void Update() {
        GetInputs();
        Vector3 moveDir = Vector3.zero;

        moveDir += cam.transform.right * horizontal;
        moveDir += cam.transform.forward * vertical;

        moveDir.Scale(new Vector3(1, 0, 1));
    }

    private void GetInputs() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }
}
