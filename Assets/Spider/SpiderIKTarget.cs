using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class SpiderIKTarget : MonoBehaviour {

    public enum SpiderLegStart {
        Forward,
        Backward,
        Neutral
    }

    public Transform spider;
    public LayerMask groundMask;
    public SpiderLegStart startPosition;
    public float maxDistance = 3f;
    public float legMoveSpeedMultiplier = 3f;

    private Vector3 neutralTarget;
    private Vector3 forwardTarget;
    private Vector3 backwardTarget;
    private Vector3 legPosition;
    private Vector3 legOffset;

    private float signedDistance;

    public bool updatingTarget;

    void Start () {
        updatingTarget = false;

        if (spider == null) {
            spider = transform.root;
        }

        legPosition = transform.position;
        legOffset = legPosition - spider.position; //constant

        forwardTarget = legPosition + transform.forward * maxDistance;
        neutralTarget = legPosition;
        backwardTarget = legPosition - transform.forward * maxDistance;

        //Start Leg at appropriate location
        switch (startPosition) {
            case SpiderLegStart.Forward:
                transform.position = forwardTarget;
                break;
            case SpiderLegStart.Backward:
                transform.position = backwardTarget;
                break;
            case SpiderLegStart.Neutral:
                transform.position = neutralTarget;
                break;
            default:
                break;
        }
        legPosition = transform.position;

        signedDistance = Mathf.Sign(Vector3.Dot(neutralTarget, legPosition)) * (neutralTarget - legPosition).magnitude;

        transform.SetParent(null);
    }

    void LateUpdate () {
        //Update leg position
        legPosition = transform.position;

        //Update Target Points
        neutralTarget = spider.TransformPoint(legOffset);
        forwardTarget = spider.TransformPoint(legOffset) + spider.forward * maxDistance;
        backwardTarget = spider.TransformPoint(legOffset) - spider.forward * maxDistance;

        //Update Distance
        signedDistance = Mathf.Sign(Vector3.Dot(transform.forward, neutralTarget-legPosition)) * (neutralTarget - legPosition).magnitude;
        if (updatingTarget) return;

        //If enough distance has been travelled, move leg
        if (signedDistance > maxDistance + 0.1f) {
            StartCoroutine(UpdateTarget(forwardTarget));    
        } else if (signedDistance < -maxDistance - 0.1f) {
            StartCoroutine(UpdateTarget(backwardTarget));
        }
    }

    private IEnumerator UpdateTarget (Vector3 target) {
        updatingTarget = true;


        //Find closest point to target if not already grounded
        /*********************************************/
        if (false == Physics.CheckSphere(target, 0.1f, groundMask)) {
            float raycastInverseResolution = 32;
            Vector3 direction = Vector3.right;
            float radius = 5f;
            RaycastHit hit;
            List<Vector3> points = new List<Vector3>();
            int steps = Mathf.FloorToInt(360f / raycastInverseResolution);
            Quaternion xRotation = Quaternion.Euler(Vector3.right * raycastInverseResolution);
            Quaternion yRotation = Quaternion.Euler(Vector3.up * raycastInverseResolution);
            Quaternion zRotation = Quaternion.Euler(Vector3.forward * raycastInverseResolution);
            for (int x = 0; x < steps / 2; x++) {
                direction = zRotation * direction;
                for (int y = 0; y < steps; y++) {
                    direction = xRotation * direction;

                    if (Physics.Raycast(target, direction, out hit, radius, groundMask)) {
                        points.Add(hit.point);
                    }
                }
            }
            float minDist = float.PositiveInfinity;
            Vector3 closestPoint = target;
            foreach (var point in points) {
                float dist = Vector3.Magnitude(point - target);
                if (dist < minDist) {
                    minDist = dist;
                    closestPoint = point;
                }
            }
            target = closestPoint;
        }
        /*********************************************/


        //Quickly lerp to target
        float t = 0f;
        Vector3 initialPos = transform.position;
        Quaternion initialRotation = transform.rotation;
        while (t < 1f) {
            transform.position = Vector3.Lerp(initialPos, target, t);
            transform.rotation = Quaternion.Lerp(initialRotation, spider.rotation, t);
            t += legMoveSpeedMultiplier * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = target;
        if (Physics.CheckSphere(target, 2f, groundMask)) {
            FX_Spawner.instance.SpawnFX(FXType.FOOTSTEP, transform.position, Vector3.zero);
        }
        updatingTarget = false;
    }

    void OnDrawGizmos () {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(legPosition, 0.25f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(forwardTarget, 0.25f);
        Gizmos.DrawSphere(neutralTarget, 0.25f);
        Gizmos.DrawSphere(backwardTarget, 0.25f);

    }
}