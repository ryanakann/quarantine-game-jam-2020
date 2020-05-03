using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    // Singleton code
    public static GameTimer instance;

    [HideInInspector]
    public float time = 60f;
    public float max_time = 60f;
    bool toot;
    public GameEvent TimerEnd;

    [HideInInspector]
    public Image timer_bar;
    float original_width;


    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            time = max_time;
            timer_bar = transform.parent.FindDeepChild("TimerBar").GetComponent<Image>();
            original_width = timer_bar.rectTransform.localScale.x;
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
        if (timer_bar)
            timer_bar.rectTransform.localScale = new Vector3(
                Mathf.Lerp(original_width, 0, (max_time-time)/max_time), 
                timer_bar.rectTransform.localScale.y, 
                timer_bar.rectTransform.localScale.z);
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
