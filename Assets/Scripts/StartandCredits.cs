using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartandCredits : MonoBehaviour
{

    public void StartGame()
    {
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentLevel"));
        }

        else
        {
            SceneManager.LoadScene(1);
        }
        
    }

    public void SeeCredits()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);

    }

    public void BacktoMain()
    {
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
