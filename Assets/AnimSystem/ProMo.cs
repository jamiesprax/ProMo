using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProMo : MonoBehaviour {

    public float moveSpeed;
    public float rotSpeed;

    private Rigidbody rb;

    private Vector3 moveDir;
    private float horizontal, vertical;

    private Vector3 rotDir;
    private float turnHorizontal;

    private Camera cam;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
    }

    void Update() {
        GetInputs();
        Vector3 tempMovDir = Vector3.zero;

        tempMovDir += cam.transform.right * horizontal;
        tempMovDir += cam.transform.forward * vertical;

        tempMovDir.Scale(new Vector3(1, 0, 1));
        moveDir = tempMovDir;

        rotDir += Vector3.up * turnHorizontal;
    }

    private void GetInputs() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        turnHorizontal = Input.GetAxis("Joy X");
    }

    private void FixedUpdate() {
        Vector3 moveVel = moveDir * moveSpeed;
        Vector3 rotVal = rotDir * rotSpeed;

        rb.velocity = moveVel;
        rb.rotation = Quaternion.Euler(rotVal);
    }

}
