using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Web : MonoBehaviour {

    public float maxArea = 256f;
    public float currentArea;
    public bool finished;

    public Transform player;

    public LayerMask groundMask;
    public GameObject placementSound;
    public GameObject destructionSound;

    private void Awake () {
        if (player == null) player = FindObjectOfType<SpiderController>().transform;
        StartWeb();
    }

    private void Update () {
        if (finished) return;
        currentArea = transform.localScale.magnitude;
        if (currentArea > maxArea) {
            FinishWeb();
        } else {
            Vector3 planarPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            float width = (player.position - transform.position).magnitude;
            float height = Mathf.Sign(transform.position.y - player.position.y) * Mathf.Max(4f, Mathf.Abs(player.position.y - transform.position.y));
            transform.LookAt(player.position, Vector3.up);
            transform.localScale = new Vector3(width, width, width);
        }
    }

    public void StartWeb () {
        finished = false;
        transform.position = player.position;
        GetComponentInChildren<Collider>().enabled = false;
    }

    public void FinishWeb () {
        player.GetComponent<SpiderController>().currentWeb = null;

        if (Physics.CheckSphere(player.position, 5f, player.GetComponent<SpiderController>().groundLayer)) RemoveWeb();

        GetComponentInChildren<Collider>().enabled = true;
    }

    public void RemoveWeb () {
        Instantiate(destructionSound, player.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
