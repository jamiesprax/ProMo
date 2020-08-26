
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Path))]
[CanEditMultipleObjects]
public class PathEditor : Editor {

    public SerializedProperty
        pathLength,
        loop,
        pathStrictness,
        pathSplits,
        pathFollower,
        nodePrefab,
        moveSpeed
        ;


    SerializedProperty[] properties = new SerializedProperty[99];
    void OnEnable() {
        pathLength = serializedObject.FindProperty("pathLength");
        loop = serializedObject.FindProperty("loop");
        pathStrictness = serializedObject.FindProperty("pathStrictness");
        pathSplits = serializedObject.FindProperty("pathSplits");
        pathFollower = serializedObject.FindProperty("pathFollower");
        nodePrefab = serializedObject.FindProperty("nodePrefab");
        moveSpeed = serializedObject.FindProperty("moveSpeed");

        // Reorder editor window here
        properties = new SerializedProperty[] {
            pathLength,
            pathSplits,
            loop,
            pathStrictness,
            pathFollower,
            nodePrefab,
            moveSpeed,
        };
    }


    public override void OnInspectorGUI() {
        serializedObject.Update();

        foreach (SerializedProperty sp in properties) {
            if (sp != null) EditorGUILayout.PropertyField(sp);
        }

        serializedObject.ApplyModifiedProperties();

    }
}
