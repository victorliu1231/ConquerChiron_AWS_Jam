using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGenerator : MonoBehaviour
{
    public List<GameObject> asteroidPrefabs;
    public Transform BottomLeftFrontVertex;
    public Transform TopRightBackVertex;
    public Transform asteroidsParent;
    public int numAsteroidsToGenerate = 100;

    [ContextMenu("Generate Asteroids")]
    void GenerateAsteroids(){
        for (int i = 0; i < numAsteroidsToGenerate; i++){
            Vector3 position = new Vector3(Random.Range(BottomLeftFrontVertex.position.x, TopRightBackVertex.position.x),
                                           Random.Range(BottomLeftFrontVertex.position.y, TopRightBackVertex.position.y),
                                           Random.Range(BottomLeftFrontVertex.position.z, TopRightBackVertex.position.z));
            Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)], position, Quaternion.identity, asteroidsParent);
        }
    }
}