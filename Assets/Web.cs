using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Web : MonoBehaviour {

    public float maxArea = 256f;
    public float currentArea;
    public bool finished;

    public Transform player;

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
            transform.localScale = new Vector3(1f, width * 2f / 3f, width);
        }
    }

    public void StartWeb () {
        finished = false;
        transform.position = player.position;
        GetComponentInChildren<Collider>().enabled = false;
    }

    public void FinishWeb () {
        player.GetComponent<SpiderController>().currentWeb = null;
        
        GetComponentInChildren<Collider>().enabled = true;
    }
}
