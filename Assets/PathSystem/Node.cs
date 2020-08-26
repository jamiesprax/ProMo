using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Node : MonoBehaviour {

    public int index;

    private void Update() {
        index = transform.GetSiblingIndex();
    }

    private void OnDrawGizmos() {
        if (index == 0) {
            // Start node
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, .3f);
        } else if (index == transform.parent.childCount - 1) {
            // End node
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, .3f);
        } else {
            // Between nodes
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, .25f);
        }
    }
}
