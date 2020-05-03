using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class End : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.FindDeepChild("Score").GetComponent<TextMeshProUGUI>().text = $"Score:{BoidManager.score}";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
