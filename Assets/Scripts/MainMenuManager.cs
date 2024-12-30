using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public GameObject resumeButton;

    void Awake(){
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            resumeButton.SetActive(true);
        }
        else
        {
            resumeButton.SetActive(false);
        }
    }

    public void NewGame(){
        PlayerPrefs.DeleteAll();
        PlayGame();
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            PlayGame();
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}