using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour {

    public bool drawBones;

    [Range(0, 0.1f)]
    public float max = 0.06f;
    public int count = 20;

    private void OnDrawGizmos() {

        float halfDens = count / 2f;

        if (transform.parent != null && drawBones) {

            float dist = Vector3.Distance(transform.position, transform.parent.position);

            for (float i = 1; i < count; i++) {
                float t = i / count;
                Vector3 gizmoPos = Vector3.Slerp(transform.position, transform.parent.position, t);

                float sizeT = 1 - Mathf.Abs(t - 0.5f);

                float size = Mathf.Lerp(0.02f, max, sizeT);
                float finalSize = size * (1 - (dist / 3f));

                Gizmos.color = Color.Lerp(Color.red, Color.blue, size / max);
                Gizmos.DrawSphere(gizmoPos, finalSize);
            }
        }

    }
}
