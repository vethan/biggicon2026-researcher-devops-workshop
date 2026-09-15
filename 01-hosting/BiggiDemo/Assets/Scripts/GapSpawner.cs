using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GapSpawner : MonoBehaviour
{
    public int gapDistance = 4;

    public PillarGap original;    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int countToSpawn = 20 / gapDistance;

        for(int i = 1; i < countToSpawn; i++)
        {
            GameObject.Instantiate<PillarGap>(original,new Vector3(i*gapDistance,Mathf.Lerp(-2, 2, Random.value),0f),Quaternion.identity);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
