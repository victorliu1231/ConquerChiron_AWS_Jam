using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Needle : MonoBehaviour {
    public float rotateSpeed = 3000f;
    public float naturalFallbackSpeed = 50f;
    public float maxAngleFromVertical = 90f;
    public float angleOfVertical = 200f;
    public float timeBetweenBenchmarks = 5f;
    public List<float> rotationBenchmarks;
    public float angleErrorMargin = 10f; // goes on both sides of each rotation benchmark
    public int minBenchmarksCompleted = 3; // minimum number of benchmarks that must be completed before task is considered successful
    public int maxBenchmarkAttempts = 5; // maximum number of attempts allowed for task
    public Image shadeImage;
    private float _timer = 0f;
    private int _numBenchmarksCompleted = 0;
    private int _numAttempts = 0;
    private bool _isTaskCompleted = false;

    void Start(){
        rotationBenchmarks = new List<float>();
        for (int i = 0; i < maxBenchmarkAttempts; i++){
            rotationBenchmarks.Add(Random.Range(0, maxAngleFromVertical));
        }
        transform.localRotation = Quaternion.Euler(0, angleOfVertical - maxAngleFromVertical, 0);
        SetRotation(rotationBenchmarks[0]);
    }

    void Update(){
        if (!_isTaskCompleted){
            if (_numAttempts < rotationBenchmarks.Count){
                _timer += Time.deltaTime;
                if (_timer < timeBetweenBenchmarks){
                    if (transform.localRotation.eulerAngles.y >= angleOfVertical - maxAngleFromVertical - 1f && transform.localRotation.eulerAngles.y <= angleOfVertical + maxAngleFromVertical){
                        if (Input.GetKeyDown(KeyCode.Space)){
                            transform.Rotate(0,rotateSpeed*Time.deltaTime,0);
                        } else {
                            transform.Rotate(0, -naturalFallbackSpeed*Time.deltaTime, 0);
                        }
                    } else if (transform.localRotation.eulerAngles.y < angleOfVertical - maxAngleFromVertical){
                        transform.localRotation = Quaternion.Euler(0,angleOfVertical - maxAngleFromVertical,0);
                    } else if (transform.localRotation.eulerAngles.y > angleOfVertical + maxAngleFromVertical){
                        transform.localRotation = Quaternion.Euler(0,angleOfVertical + maxAngleFromVertical,0);
                    }
                } else {
                    if (transform.localRotation.eulerAngles.y >= angleOfVertical - maxAngleFromVertical + rotationBenchmarks[_numAttempts] - angleErrorMargin && transform.localRotation.eulerAngles.y <= angleOfVertical - maxAngleFromVertical + rotationBenchmarks[_numAttempts] + angleErrorMargin){
                        _timer = 0f;
                        _numBenchmarksCompleted++;
                    } else {
                        _timer = 0f;
                    }
                    _numAttempts++;
                    SetRotation(rotationBenchmarks[_numAttempts]);
                }
                
                if (_numBenchmarksCompleted >= minBenchmarksCompleted){
                    _isTaskCompleted = true;
                    Debug.Log("Task completed!");
                    // End task early
                }
            } else {
                Debug.Log("Task failed.");
            }
        }
    }

    public void SetRotation(float angle){
        shadeImage.fillAmount = angleErrorMargin / 180f;
        shadeImage.transform.localRotation = Quaternion.Euler(0, 0, -180 + angleErrorMargin + (90 - angle));
    }
}