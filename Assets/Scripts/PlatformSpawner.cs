using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class PlatformSpawner : MonoBehaviour
//{
//    [SerializeField] GameObject platformPrefab;

//    [SerializeField] int count = 3;

//    //다음배치까지의 시간간격 최소값
//    private float timeBetSpawnMin = 1.25f;
//    private float timeBetSpawnMax = 2.25f;

//    private float timeBetSpawn;

//    //배치할 위치의 최소 y값
//    private float yMin = -3.5f;
//    private float yMax = 1.5f;

//    private float xPos = 20.0f;

//    private GameObject[] platforms;

//    private int currentIndex = 0;

//    private Vector2 poolPosition = new Vector2(0, -25f);

//    //마지막 배치시점
//    private float lastSpawnTime;

//    // Start is called before the first frame update
//    void Start()
//    {
//        platforms = new GameObject[count];

//        for (int i = 0; i < count; i++)
//        {
//            platforms[i] = Instantiate(platformPrefab, poolPosition, Quaternion.identity);

//            lastSpawnTime = 0.0f;
//            timeBetSpawn = 0.0f;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (GameManager.Instance.IsGameOver) return;
//        //마지막 배치시점에서 timeBetSpawn이상 시간흐르면
//        if (Time.time >= lastSpawnTime + timeBetSpawn) 
//        {
//            lastSpawnTime = Time.time;

//            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
//            float yPos = Random.Range(yMin, yMax);

//            platforms[currentIndex].SetActive(false);
//            platforms[currentIndex].SetActive(true);

//            platforms[currentIndex].transform.position = new Vector2 (xPos, yPos);

//            currentIndex++;

//            //마지막에 도달하면 리셋
//            if(currentIndex>=count)
//            {
//                currentIndex = 0;
//            }
//        }
//    }
//}



public class PlatformSpawner : MonoBehaviour
{
    [SerializeField] private PlatForm platformPrefab;

    public int initCount = 10;
    private ObjectPool<PlatForm> platformPool;

    //다음배치까지의 시간간격 최소값
    private float timeBetSpawnMin = 1.25f;
    private float timeBetSpawnMax = 2.25f;

    private float timeBetSpawn;
    //마지막 배치시점
    private float lastSpawnTime;

    //배치할 위치의 최소 y값
    private float yMin = -3.5f;
    private float yMax = 1.5f;

    private float xPos = 20.0f;

    private void Start()
    {
        platformPool = new ObjectPool<PlatForm>(platformPrefab, initCount, transform);
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = Time.time;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (Time.time >=lastSpawnTime + timeBetSpawn)
        {
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            float yPos = Random.Range(yMin, yMax);
            PlatForm platform = platformPool.Get();
            platform.transform.position = new Vector2(xPos, yPos);
            platform.SetPool(platformPool);
        }
    }
}
