using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum FXType
{
    Default,
    PLATFORM_BREAK,
    FOOTSTEP
}

public class FX_Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct SerializedDict
    {
        public FXType key;
        public UnityEngine.GameObject value;
    }

    public AudioMixerGroup mixer;
    private UnityEngine.GameObject holder;

    public List<SerializedDict> Serialized_FX_Dict = new List<SerializedDict>();
    public Dictionary<FXType, UnityEngine.GameObject> FX_Dict = new Dictionary<FXType, UnityEngine.GameObject>();

    // Singleton code
    public static FX_Spawner instance;
    private void Awake() {
        if (null == instance) {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }

        foreach (var entry in Serialized_FX_Dict)
            FX_Dict[entry.key] = entry.value;
        if (FX_Dict.ContainsKey(FXType.Default))
            FX_Dict[FXType.Default] = null;
        holder = new UnityEngine.GameObject("FX Objects");
    }


    public UnityEngine.GameObject SpawnFX(UnityEngine.GameObject fx, Vector3 position, Vector3 rotation, float vol = -1, Transform parent = null) {
        if (fx == null) return null;
        UnityEngine.GameObject spawned_fx = Instantiate(fx, position, Quaternion.identity);

        if (spawned_fx == null) return null;

        spawned_fx.transform.parent = (parent == null ? parent : holder.transform);

        if (rotation != Vector3.zero)
            spawned_fx.transform.forward = rotation;
        FX_Object fx_obj = spawned_fx.GetComponent<FX_Object>();
        fx_obj.vol = vol;
        fx_obj.mixerGroup = mixer;

        return spawned_fx;
    }

    public UnityEngine.GameObject SpawnFX(FXType effectName, Vector3 position, Vector3 rotation, float vol = -1, Transform parent = null) {
        return SpawnFX(FX_Dict[effectName], position, rotation, vol, parent);
        //return SpawnFX(FX_Dict.GetValueOrDefault(effectName, FX_Dict[FXType.Default]), position, rotation, vol, parent);
    }
}