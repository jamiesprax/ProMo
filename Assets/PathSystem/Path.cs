using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Path : MonoBehaviour {

    public bool loop;
    [Range(0.0001f, 0.1f)]
    public float pathStrictness;
    [Range(1, 10)]
    public int pathLength;
    [Range(1, 30)]
    public int pathSplits;


    public GameObject pathFollower;
    private GameObject lem;


    public GameObject nodePrefab;

    private bool settingsUpdated;

    private Vector3 currPos;
    private Vector3 currNode;

    private int pathIndex;
    private float lastMoveTime;
    private float moveTime;
    public float moveSpeed;

    private void Start() {
        print("Start");
        settingsUpdated = true;

        if (Application.isPlaying) {
            lem = Instantiate(pathFollower, transform.GetChild(0).position, Quaternion.identity);
            pathIndex = 0;
            lastMoveTime = 0;
        }
    }

    private void Update() {
        print("Update");
        if (settingsUpdated) Run();

        if (Application.isPlaying) {

            moveTime += Time.deltaTime;
            if (Frac(moveTime) < Frac(lastMoveTime)) pathIndex = (pathIndex + 1) % pathLength;
            lastMoveTime = moveTime;

            Vector3 A = NodePos(pathIndex);
            Vector3 B = NodePos(pathIndex + 1);
            Vector3 C = NodePos(pathIndex + 2);
            Vector3 D = NodePos(pathIndex + 3);

            Vector3 P = Interpolate(A, B, C, D, Frac(moveTime));

            lem.transform.position = Vector3.Lerp(lem.transform.position, P, pathStrictness);
        }

    }

    private float Frac(float f) {
        return Mathf.Repeat(f, 1f);
    }

    private void Run() {
        print("Updating");

        if (pathLength < transform.childCount) {
            for (int i = pathLength; i < transform.childCount; i++) {
                if (Application.isPlaying) {
                    Destroy(transform.GetChild(i).gameObject);
                } else {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }

            }
        } else if (pathLength > transform.childCount) {
            for (int i = transform.childCount; i < pathLength; i++) {
                GameObject node = Instantiate(nodePrefab, transform.position + new Vector3(i, 0, i), Quaternion.identity, transform);
                node.name = "Node" + i;
            }
        }

        settingsUpdated = false;
    }

    private void OnValidate() {
        settingsUpdated = true;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        if (transform.childCount != pathLength) return;

        if (pathLength == 0 || pathLength == 1) return;

        if (pathLength == 2) {
            Vector3 A = NodePos(0);
            Vector3 B = NodePos(1);

            float div = 1f / pathSplits;
            for (float t = div; t < 1; t += div) {
                Vector3 P = Vector3.Lerp(A, B, t);
                Gizmos.DrawSphere(P, 0.1f);
            }
            return;
        }

        if (pathLength == 3) {
            if (!loop) {
                Vector3 A = NodePos(0);
                Vector3 B = NodePos(1);
                Vector3 C = NodePos(2);
                Vector3 P = Vector3.zero;

                float div = 1f / pathSplits;
                for (float t = div; t < 1; t += div) {
                    P = Bez(A, B, C, t);
                    Gizmos.DrawSphere(P, 0.1f);
                }
                Gizmos.DrawLine(B, A);
                Gizmos.DrawLine(B, C);
                return;
            }
        }
        if (loop) {
            for (int i = 0; i < pathLength; i++) {

                Vector3 A = NodePos(i);
                Vector3 B = NodePos(i + 1);
                Vector3 C = NodePos(i + 2);
                Vector3 D = NodePos(i + 3);

                float div = 1f / pathSplits;
                for (float t = div; t < 1; t += div) {
                    Vector3 P = Interpolate(A, B, C, D, t);
                    Gizmos.color = Color.Lerp(Color.red, Color.white, t);
                    Gizmos.DrawSphere(P, 0.1f);
                }
            }
        } else {
            for (int i = 0; i < pathLength - 2; i += 2) {

                Vector3 A = NodePos(i);
                Vector3 B = NodePos(i + 1);
                Vector3 C = NodePos(i + 2);

                float div = 1f / pathSplits;
                for (float t = div; t < 1; t += div) {
                    Vector3 P = Bez(A, B, C, t);
                    Gizmos.color = Color.Lerp(Color.red, Color.white, t);
                    Gizmos.DrawSphere(P, 0.1f);
                }
            }
        }

    }

    private Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u) {
        return (
            0.5f *
            (
                (-a + 3f * b - 3f * c + d) *
                (u * u * u) +
                (2f * a - 5f * b + 4f * c - d) *
                (u * u) +
                (-a + c) *
                u + 2f * b
            )
        );
    }

    private Transform NodeTransform(int index) {
        return transform.GetChild(index % transform.childCount);
    }

    private Vector3 NodePos(int index) {
        return NodeTransform(index).position;
    }

    private Vector3 Bez(Vector3 start, Vector3 control, Vector3 end, float t) {
        return Vector3.Lerp(Vector3.Lerp(start, control, t), Vector3.Lerp(control, end, t), t);
    }
}
