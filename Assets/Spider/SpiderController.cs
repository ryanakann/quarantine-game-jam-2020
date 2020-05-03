﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderController : MonoBehaviour {
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float speed = 5f;
    [Tooltip("Degrees per second")]
    public float rotSpeed = 90f;

    [Header("Jump")]
    public float jumpForce;
    public bool airControl = false;
    public bool grounded;
    private bool groundedLF;
    public bool jumping;
    private float timeSinceLanded;
    public float landingAdjustTime = 1f;
    public float landingAdjustDamp = 0.2f;

    

    [Header("Snap to Ground Settings")]
    public float groundedColliderRadius = 5f;
    public float airborneColliderRadius = 15f;
    private float colliderRadius;
    public float targetDistanceFromSurface = 0.1f;
    public LayerMask groundLayer;
    public float raycastInverseResolution = 16;

    [Range(0f, 100f)]
    [Tooltip("How quickly to smoothly damp to target normal")]
    public float snappiness = 100f;


    [Header("Debug")]
    public Vector3 velocity;
    public Vector3 gravVelocity;
    public float x;
    public float y;

    public Vector3 up;
    private Vector3 smoothUp;
    private Vector3 upVelocityRef;

    public Vector3 forward;
    private Vector3 smoothForward;
    private Vector3 forwardVelocityRef;

    public Vector3 right;

    private Vector3 posVelRef;
    private float distance;

    private RaycastHit hit;

    private Rigidbody rb;
    private BoxCollider col;

    void Awake () {
        distance = 0f;
        timeSinceLanded = 9999f;

        col = GetComponent<BoxCollider>();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Update () {
        Move();
    }

    private void Move () {
        Vector3 direction = Vector3.right;
        colliderRadius = groundedColliderRadius;

        int steps = Mathf.FloorToInt(360f / raycastInverseResolution);
        //print("Steps: " + steps);
        Quaternion xRotation = Quaternion.Euler(Vector3.right * raycastInverseResolution);
        Quaternion yRotation = Quaternion.Euler(Vector3.up * raycastInverseResolution);
        Quaternion zRotation = Quaternion.Euler(Vector3.forward * raycastInverseResolution);


        //Average sphere of raycasts to determine up direction
        up = Vector3.zero;
        int hitCount = 0;
        if (!jumping) {
            distance = 0f;
            for (int x = 0; x < steps / 2; x++) {
                direction = zRotation * direction;
                for (int y = 0; y < steps; y++) {
                    direction = xRotation * direction;

                    if (Physics.Raycast(transform.position, direction, out hit, groundedColliderRadius, groundLayer)) {
                        Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
                        up += hit.normal / hit.distance;
                        hitCount++;
                    }
                }
            }
            up.Normalize();
        }
        if (hitCount == 0) {
            up = Vector3.up;
        }


        if (grounded || airControl) {
            x = Input.GetAxisRaw("Horizontal") * rotSpeed;
            y = Input.GetAxis("Vertical") * speed;
        } else {
            y = 0f; //Reset rotation prevent endless air spinning
        }
        right = Quaternion.Euler(up * x * (100f / snappiness) * Time.deltaTime) * transform.right;
        forward = Vector3.Cross(right, up);

        smoothUp = Vector3.SmoothDamp(transform.up, up, ref upVelocityRef, 1f / (snappiness));
        smoothForward = Vector3.SmoothDamp(transform.forward, forward, ref forwardVelocityRef, 1 / (snappiness));

        rb.rotation = Quaternion.LookRotation(smoothForward, smoothUp);

        velocity = forward * y;

        //Jump handling
        if (!jumping) {
            if (!grounded) {
                if (rb.velocity.y < 0f) {
                    gravVelocity += Physics.gravity * 3f * Time.deltaTime;
                } else {
                    if (Input.GetButton("Jump")) {
                        gravVelocity += Physics.gravity * Time.deltaTime;
                    } else {
                        gravVelocity += Physics.gravity * 4f * Time.deltaTime;
                    }
                }

                if (Physics.CheckBox(transform.position + col.center, col.bounds.extents, transform.rotation, groundLayer)) {
                    grounded = true;
                }
            } else {
                gravVelocity = Vector3.zero;

                if (Input.GetButtonDown("Jump") && !jumping) {
                    Jump();
                }
            }
        }

        //Apply velocity
        velocity += gravVelocity;
        rb.velocity = velocity;

        //Prevent spider from floating away from ground
        if (!jumping && timeSinceLanded < landingAdjustTime && Physics.Raycast(rb.position, -up, out hit, groundedColliderRadius, groundLayer)) {
            distance = hit.distance;
            if (distance > targetDistanceFromSurface) {
                Vector3 target = rb.position - up * (distance - targetDistanceFromSurface);
                rb.position = Vector3.SmoothDamp(rb.position, target, ref posVelRef, landingAdjustDamp);
            }
        }

        if (grounded) {
            timeSinceLanded += Time.deltaTime;
        } else {
            timeSinceLanded = 0f;
        }

        groundedLF = grounded;
    }

    private void Jump () {
        grounded = false;
        gravVelocity = transform.up * jumpForce;
        StartCoroutine(JumpBuffer());
    }

    IEnumerator JumpBuffer () {
        jumping = true;
        yield return new WaitForSeconds(0.1f);
        jumping = false;
    }

    void OnDrawGizmos () {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, groundedColliderRadius);
        if (col)
            Gizmos.DrawCube(transform.position+col.center, col.bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, up * 2 * groundedColliderRadius);
        Gizmos.DrawRay(transform.position, smoothUp * 2 * groundedColliderRadius);
        Gizmos.DrawSphere(transform.position + up * 2 * groundedColliderRadius, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, forward * 2 * groundedColliderRadius);
        Gizmos.DrawRay(transform.position, smoothForward * 2 * groundedColliderRadius);
        Gizmos.DrawSphere(transform.position + forward * 2 * groundedColliderRadius, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right * 2 * groundedColliderRadius);
        Gizmos.DrawSphere(transform.position + right * 2 * groundedColliderRadius, 0.3f);

    }
}