using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton code
    public static GameManager instance;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            transform.parent.GetComponentInChildren<GameTimer>().TimerEnd += End;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void End()
    {
        SceneManager.LoadScene(1);
    }
}
