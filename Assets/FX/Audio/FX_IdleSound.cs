
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX_IdleSound : MonoBehaviour
{
    public List<UnityEngine.GameObject> sounds = new List<UnityEngine.GameObject>();
    public float soundTime = 10f;
    bool playing;

    // Update is called once per frame
    void Update()
    {
        if (!playing && sounds.Count > 0)
        {
            StartCoroutine(PlayIdle());
        }
    }

    IEnumerator PlayIdle()
    {
        playing = true;
        yield return new WaitForSeconds(Random.Range(1f, soundTime));
        UnityEngine.GameObject fx = sounds[Random.Range(0, sounds.Count)];
        FX_Spawner.instance.SpawnFX(fx, transform.position, transform.up);
        playing = false;
    }
}