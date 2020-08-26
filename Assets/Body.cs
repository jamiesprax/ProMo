using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour {
    public BaseIK frontLeft;
    public BaseIK backLeft;
    public BaseIK frontRight;
    public BaseIK backRight;

    public LayerMask terrainLayerMask;

    [Range(.5f, 1.15f)]
    public float height;
    public float heightSlerpVal;

    private Vector3 idealPosition;
    private Quaternion idealRot;
    private float heightDiff;
    private float posDiff;
    public Vector3 footOffsets;

    public Target focusTarget;
    public float idealDistanceFromFocus;

    private Vector3 LEFT = new Vector3(-1, 1, 1);
    private Vector3 BACK = new Vector3(1, 1, -1);

    private void Awake() {
        frontLeft.restingOffset = Vector3.Scale(footOffsets, LEFT);
        backLeft.restingOffset = Vector3.Scale(Vector3.Scale(footOffsets, LEFT), BACK);

        frontRight.restingOffset = footOffsets;
        backRight.restingOffset = Vector3.Scale(footOffsets, BACK);

        frontLeft.Init(this);
        backLeft.Init(this);
        frontRight.Init(this);
        backRight.Init(this);

        StartCoroutine(LegUpdateCoroutine());
    }

    // Update is called once per frame
    void Update() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, LayerMask.GetMask("Terrain"))) {
            heightDiff = height - hit.distance;
        }

        idealPosition = new Vector3(transform.position.x, transform.position.y + heightDiff, transform.position.z);

        if (focusTarget != null) HandleFocusTarget();


        transform.position = Vector3.Lerp(transform.position, idealPosition, heightSlerpVal);
    }

    void HandleFocusTarget() {
        Vector3 currPos = transform.position;
        Vector3 focusPos = focusTarget.transform.position;
        Vector3 dir = (currPos - focusPos).normalized;

        float distance = Vector3.Distance(currPos, focusPos);

        //currPos -= dir * (distance - idealDistanceFromFocus); // Test this at some point instead of the if below

        if (distance > idealDistanceFromFocus) {
            currPos -= dir * (distance - idealDistanceFromFocus);
        } else {
            currPos += dir * (idealDistanceFromFocus - distance);
        }

        idealPosition.x = currPos.x;
        idealPosition.z = currPos.z;

        transform.LookAt(new Vector3(focusPos.x, currPos.y, focusPos.z));
    }

    IEnumerator LegUpdateCoroutine() {
        while (true) {
            do {
                frontLeft.TryMove();
                backRight.TryMove();
                yield return null;

            } while (frontLeft.isMoving() || backRight.isMoving());

            do {
                frontRight.TryMove();
                backLeft.TryMove();
                yield return null;
            } while (frontRight.isMoving() || backLeft.isMoving());
        }
    }

    private void OnDrawGizmos() {
        if (idealPosition != null) {

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.forward, 0.025f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(idealPosition + transform.forward, 0.05f);
        }
    }
}
