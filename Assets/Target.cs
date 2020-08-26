using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    public BaseIK IK;
    public bool armTarget;
    private Vector3 restingPos;

    private bool moving;
    private float stepDistance = .4f;
    private float stepTime = .1f;
    private float overShoot = 1.5f;


    public void Init(BaseIK ik) {
        IK = ik;
        restingPos = IK.rotatedOffset;
        RaycastHit hit;
        if (Physics.Raycast(restingPos, Vector3.down, out hit, LayerMask.GetMask("Terrain"))) {
            transform.position = hit.point;
        }
    }

    public void TryMove() {
        if (moving) return;

        restingPos = IK.rotatedOffset;

        Debug.DrawLine(IK.IKRoot().position, IK.IKRoot().position + Vector3.down * 3f, Color.red);
        Debug.DrawLine(restingPos, restingPos + Vector3.down * 3f, Color.green);

        RaycastHit hit;
        if (Physics.Raycast(restingPos, Vector3.down, out hit, LayerMask.GetMask("Terrain"))) {
            float distFromHome = Vector3.Distance(transform.position, hit.point);
            Debug.DrawLine(transform.position, hit.point, Color.green);

            if (distFromHome > stepDistance) {
                StartCoroutine(MoveToRootRestingPosition(hit.point));
            }
        }
    }


    IEnumerator MoveToRootRestingPosition(Vector3 restingPos) {
        moving = true;

        Vector3 startPoint = transform.position;

        // dir vector - NOT NORMALISED
        Vector3 towardHome = (restingPos - transform.position);

        float overshootDistance = stepDistance * overShoot;
        Vector3 overshootVector = towardHome * overshootDistance;

        // Simple move forward overshoot on the same plane. Add overshoot to root pos and cast down to get real point later
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        Vector3 endPoint = restingPos + overshootVector;

        // Find middle distance of step and keyframe height there
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // But also lift off, so we move it up by half the step distance (arbitrarily)
        centerPoint += Vector3.up * Vector3.Distance(startPoint, endPoint) / 2f;

        // main movement loop
        float timeElapsed = 0;
        while (timeElapsed < stepTime) {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / stepTime;

            // lerp that lerp (ref bezier curves)
            transform.position =
              Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
              );

            yield return null; //wait a frame
        }

        moving = false;
    }

    public bool isMoving() {
        return moving;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}
