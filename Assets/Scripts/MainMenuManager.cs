using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public TMP_InputField inputField;

    void Start()
    {
        inputField.onSubmit.AddListener((string username) => PlayGame(username));
    }
    
    void Update(){
        Cursor.visible = true;
    }

    // add bday place to the Beli
    // Beli Yale leaderboards and put in school email

    public async void PlayGame(string username)
    {
        if (username == "") return;
        var listUserStats = await DynamoDB.Instance.GetAllUsers();
        List<int> existingIDs = new List<int>();
        for (int i = 0; i < listUserStats.Count; i++){
            existingIDs.Add(listUserStats[i].id);
            if (listUserStats[i].username == username){
                DynamoDB.Instance.username = username;
                DynamoDB.Instance.playerID = listUserStats[i].id;
                SceneManager.LoadScene(gameSceneName);
                return;
            }
        }
        DynamoDB.Instance.username = username;
        DynamoDB.Instance.playerID = existingIDs.Count + 1;
        SceneManager.LoadScene(gameSceneName);
    }

    public void PlayGame()
    {
        PlayGame(inputField.text);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}