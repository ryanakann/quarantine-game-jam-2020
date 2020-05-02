using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FX_SoundList : FX_Object
{
    public static Dictionary<string, int> soundListIndices;

    public List<AudioClip> clips = new List<AudioClip>();
    public bool randomize = true;
    AudioSource aud;
    int index;

    // Start is called before the first frame update
    void Start() {
        aud = GetComponent<AudioSource>();
        if (!aud || clips.Count == 0) return;

        if (!randomize) {
            if (soundListIndices == null) {
                soundListIndices = new Dictionary<string, int>();
            }
            string objName = gameObject.name.Replace("(Clone)", "");
            if (soundListIndices.ContainsKey(objName)) {
                soundListIndices[objName] += 1;
                index = soundListIndices[objName];
            } else {
                soundListIndices.Add(objName, 0);
                index = 0;
            }
            aud.clip = clips[index % clips.Count];
        } else {
            aud.clip = clips[Random.Range(0, clips.Count)];
        }


        aud.Play();

        if (vol != -1)
        {
            aud.volume = vol;
        }
        aud.pitch += Random.Range(-pitch_range, pitch_range);
        aud.volume += Random.Range(-amp_range, 0);


        Destroy(gameObject, Mathf.Max(lifetime, aud.clip.length));
    }
}