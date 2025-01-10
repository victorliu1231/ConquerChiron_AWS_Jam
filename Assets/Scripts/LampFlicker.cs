using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LampFlicker : MonoBehaviour {
    public List<Light> lights;
    public float lengthOfFlicker = 0.75f;
    private float _flickerTimer = 0f;
    public float minTimeBetweenFlickers = 3f;
    public float maxTimeBetweenFlickers = 10f;
    private bool _timeBetweenFlickersSet;
    private float _timeBetweenFlickers;
    private float _timeBetweenFlickerTimer = 0f;
    public float minTimeBetweenDims = 15f;
    public float maxTimeBetweenDims = 20f;
    private bool _timeBetweenDimsSet;
    private float _timeBetweenDims;
    private float _timeBetweenDimsTimer = 0f;

    void Update(){
        if (GameManager.Instance.gameMode == GameMode.Horror || GameManager.Instance.gameMode == GameMode.Frenzy) {
            if (!_timeBetweenFlickersSet) {
                _timeBetweenFlickers = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
                _timeBetweenFlickersSet = true;
            }
            _timeBetweenFlickerTimer += Time.deltaTime;
            if (_timeBetweenFlickerTimer >= _timeBetweenFlickers){
                _flickerTimer += Time.deltaTime;
                foreach (Light light in lights) light.intensity = Mathf.PerlinNoise(Time.time, 0) * 3;
                if (_flickerTimer >= lengthOfFlicker) {
                    _timeBetweenFlickerTimer = 0f;
                    _timeBetweenFlickersSet = false;
                    _flickerTimer = 0f;
                }
            }
            if (_flickerTimer < lengthOfFlicker){
                if (!_timeBetweenDimsSet) {
                    _timeBetweenDims = Random.Range(minTimeBetweenDims, maxTimeBetweenDims);
                    _timeBetweenDimsSet = true;
                }
                _timeBetweenDimsTimer += Time.deltaTime;
                if (_timeBetweenDimsTimer >= _timeBetweenDims){
                    foreach (Light light in lights) light.DOIntensity(0.5f, 0.5f);
                    _timeBetweenDimsTimer = 0f;
                    _timeBetweenDimsSet = false;
                }
            }
        }
    }
}