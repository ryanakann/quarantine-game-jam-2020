using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    // Singleton code
    public static GameTimer instance;

    [HideInInspector]
    public float time = 60f;
    bool toot;
    public GameEvent TimerEnd;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else if (!toot)
        {
            toot = true;
            TimerEnd?.Invoke();
        }
    }
}
