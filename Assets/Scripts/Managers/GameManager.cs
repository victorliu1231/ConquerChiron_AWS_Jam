using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;

    void Awake(){
        tasksCompleted = new List<string>();
    }

    public void GoToCheckpoint(){
        
    }
}