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

        if (signedDistance > maxDistance) {
            StartCoroutine(UpdateTarget(forwardTarget));    
        } else if (signedDistance < -maxDistance) {
            StartCoroutine(UpdateTarget(backwardTarget));
        }
    }

    private IEnumerator UpdateTarget (Vector3 target) {
        updatingTarget = true;

        int iteration = 0;
        int maxIterations = 256;

        while (Physics.OverlapSphere(transform.position, 1 / 16f, groundMask).Length > 0 && iteration++ < maxIterations) {
            transform.position += spider.transform.up * Time.deltaTime;
        }

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