using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour {
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float speed = 5f;
    [Tooltip("Degrees per second")]
    public float rotSpeed = 90f;

    [Header("Jump")]
    public float jumpForce;
    public float gravityScale = 9.81f;
    public bool jumping;
    private Vector3 velocity;

    [Header("Snap to Ground Settings")]
    public float colliderRadius = 5f;
    public float targetDistanceFromSurface = 0.1f;
    public LayerMask groundLayer;

    [Range(0f, 100f)]
    [Tooltip("How quickly to smoothly damp to target normal")]
    public float snappiness = 100f;

    public float x;
    public float y;

    private Vector3 up;
    private Vector3 smoothUp;
    private Vector3 upVelocityRef;

    private Vector3 forward;
    private Vector3 smoothForward;
    private Vector3 forwardVelocityRef;

    private Vector3 right;

    private Vector3 posVelRef;
    private float distance;

    private RaycastHit hit;

    void Awake () {
        distance = 0f;
    }

    private void Update () {
        if (!jumping) {
            Move();

            if (Input.GetButtonDown("Jump")) {
                jumping = true;
                velocity = transform.forward * y + Vector3.up * jumpForce;
            }
        } else {
            transform.position += velocity * Time.deltaTime;
            velocity += Vector3.down * gravityScale * Time.deltaTime;

            if (Physics.OverlapBox(transform.position, new Vector3(1f, 1f, 1f), transform.rotation, groundLayer).Length > 0f) {
                if (velocity.y < 0f) {
                    transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
                    jumping = false;
                }
            }
        }
    }

    private void Move () {
        float inverseResolution = 16f;
        Vector3 direction = Vector3.right;
        int steps = Mathf.FloorToInt(360f / inverseResolution);
        Quaternion xRotation = Quaternion.Euler(Vector3.right * inverseResolution);
        Quaternion yRotation = Quaternion.Euler(Vector3.up * inverseResolution);
        Quaternion zRotation = Quaternion.Euler(Vector3.forward * inverseResolution);


        //Average sphere of raycasts to determine up direction
        up = Vector3.zero;
        int hitCount = 0;
        distance = 0f;
        for (int x = 0; x < steps / 2; x++) {
            direction = zRotation * direction;
            for (int y = 0; y < steps; y++) {
                direction = xRotation * direction;
                if (Physics.Raycast(transform.position, direction, out hit, colliderRadius, groundLayer)) {
                    up += hit.normal / hit.distance;
                    hitCount++;
                }
            }
        }
        up.Normalize();
        x = Input.GetAxisRaw("Horizontal") * rotSpeed;
        y = Input.GetAxis("Vertical") * speed;
        right = Quaternion.Euler(up * x * Time.deltaTime) * transform.right;
        forward = Quaternion.Euler(up * x / (snappiness / 10f) * Time.deltaTime) * Vector3.Cross(right, up);

        smoothUp = Vector3.SmoothDamp(transform.up, up, ref upVelocityRef, 1f / snappiness / 10f);
        smoothForward = Vector3.SmoothDamp(transform.forward, forward, ref forwardVelocityRef, 1 / snappiness / 10f);

        transform.rotation = Quaternion.LookRotation(smoothForward, smoothUp);
        transform.position += forward * y * Time.deltaTime;

        //Prevent spider from floating away from ground
        if (Physics.Raycast(transform.position, -up, out hit, colliderRadius, groundLayer)) {
            distance = hit.distance;
            if (distance > targetDistanceFromSurface) {
                Vector3 target = transform.position - up * (distance - targetDistanceFromSurface);
                transform.position = Vector3.SmoothDamp(transform.position, target, ref posVelRef, 2f);
            }
        }
    }

    void OnDrawGizmos () {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, colliderRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, up * 2 * colliderRadius);
        Gizmos.DrawRay(transform.position, smoothUp * 2 * colliderRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, forward * 2 * colliderRadius);
        Gizmos.DrawRay(transform.position, smoothForward * 2 * colliderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right * 2 * colliderRadius);
    }
}