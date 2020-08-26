using UnityEditor;
using UnityEngine;

// FABRIK solver
// https://www.youtube.com/watch?v=UNoX65PRehA
public class BaseIK : MonoBehaviour {

    public int bonesInChain = 2;

    public Target target;
    public Transform pole;

    [Header("Solver Params")]
    public int Iterations = 10;

    public float solverLenience = 0.001f; // close enough?

    [Range(0, 1)]
    public float snappingStrength = 1f;

    protected float[] bonesLength; //Target to Origin
    protected float fullLength;
    protected Transform[] bones;
    protected Vector3[] positions;
    protected Vector3[] startDir;
    protected Quaternion[] startRotBone;
    protected Quaternion startRotTarget;
    protected Transform root;
    protected Body body;

    public Vector3 restingOffset;
    public Vector3 rotatedOffset;

    public void TryMove() {
        // Raycast ahead to see if there is space to step before trying to step?
        // Rootpos forward and down to check for ledges?
        target.TryMove();
    }

    public void Init(Body b) {
        bones = new Transform[bonesInChain + 1];
        positions = new Vector3[bonesInChain + 1];
        bonesLength = new float[bonesInChain];
        startDir = new Vector3[bonesInChain + 1];
        startRotBone = new Quaternion[bonesInChain + 1];
        body = b;
        rotatedOffset = body.transform.position + (body.transform.forward * restingOffset.z) + (body.transform.right * restingOffset.x);

        //find root
        root = transform;
        for (var i = 0; i <= bonesInChain; i++) {

            root = root.parent;
        }

        //init target    
        if (target == null) {
            GameObject gO = new GameObject(root.name + "Target");
            target = gO.AddComponent<Target>();
            target.Init(this);
        }
        startRotTarget = GetRotationRootSpace(target.transform);

        //init data
        var current = transform;
        fullLength = 0;
        for (var i = bones.Length - 1; i >= 0; i--) {
            bones[i] = current;
            startRotBone[i] = GetRotationRootSpace(current);

            if (i == bones.Length - 1) {
                //leaf
                startDir[i] = GetPositionRootSpace(target.transform) - GetPositionRootSpace(current);
            } else {
                //mid bone
                startDir[i] = GetPositionRootSpace(bones[i + 1]) - GetPositionRootSpace(current);
                bonesLength[i] = startDir[i].magnitude;
                fullLength += bonesLength[i];
            }

            current = current.parent;
        }
    }

    public Transform IKRoot() {
        return root;
    }

    public bool isMoving() {
        return target.isMoving();
    }

    void LateUpdate() {
        rotatedOffset = body.transform.position + (body.transform.forward * restingOffset.z) + (body.transform.right * restingOffset.x);

        ResolveIK();
    }

    private void ResolveIK() {
        if (target == null)
            return;

        if (bonesLength.Length != bonesInChain)
            throw new UnityException("Bone length != chain length");

        //  root
        //  (bone0) (boneLen 0) (bone1) (boneLen 1) (bone2)...
        //   x--------------------x--------------------x---...

        //get position
        for (int i = 0; i < bones.Length; i++)
            positions[i] = GetPositionRootSpace(bones[i]);

        var targetPosition = GetPositionRootSpace(target.transform);
        var targetRotation = GetRotationRootSpace(target.transform);

        //1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(bones[0])).sqrMagnitude >= fullLength * fullLength) {
            //just strech it
            var direction = (targetPosition - positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < positions.Length; i++)
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
        } else {
            for (int i = 0; i < positions.Length - 1; i++)
                positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + startDir[i], snappingStrength);

            for (int iteration = 0; iteration < Iterations; iteration++) {
                //back
                for (int i = positions.Length - 1; i > 0; i--) {
                    if (i == positions.Length - 1)
                        positions[i] = targetPosition; //set it to target
                    else
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i]; //set in line on distance
                }

                //forward
                for (int i = 1; i < positions.Length; i++)
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];

                //close enough?
                if ((positions[positions.Length - 1] - targetPosition).sqrMagnitude < solverLenience * solverLenience)
                    break;
            }
        }

        //move towards pole
        if (pole != null) {
            var polePosition = GetPositionRootSpace(pole);
            for (int i = 1; i < positions.Length - 1; i++) {
                var plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(polePosition);
                var projectedBone = plane.ClosestPointOnPlane(positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < positions.Length; i++) {
            if (i == positions.Length - 1)
                SetRotationRootSpace(bones[i], Quaternion.Inverse(targetRotation) * startRotTarget * Quaternion.Inverse(startRotBone[i]));
            else
                SetRotationRootSpace(bones[i], Quaternion.FromToRotation(startDir[i], positions[i + 1] - positions[i]) * Quaternion.Inverse(startRotBone[i]));
            SetPositionRootSpace(bones[i], positions[i]);
        }
    }

    private Vector3 GetPositionRootSpace(Transform current) {
        if (root == null)
            return current.position;
        else
            return Quaternion.Inverse(root.rotation) * (current.position - root.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position) {
        if (root == null)
            current.position = position;
        else
            current.position = root.rotation * position + root.position;
    }

    private Quaternion GetRotationRootSpace(Transform current) {
        //inverse(after) * before => rot: before -> after
        if (root == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * root.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation) {
        if (root == null)
            current.rotation = rotation;
        else
            current.rotation = root.rotation * rotation;
    }

    void OnDrawGizmos() {
#if UNITY_EDITOR
        var current = this.transform;
        for (int i = 0; i < bonesInChain && current != null && current.parent != null; i++) {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
#endif
    }

}