using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class PlatformSpawner : MonoBehaviour
//{
//    [SerializeField] GameObject platformPrefab;

//    [SerializeField] int count = 3;

//    //������ġ������ �ð����� �ּҰ�
//    private float timeBetSpawnMin = 1.25f;
//    private float timeBetSpawnMax = 2.25f;

//    private float timeBetSpawn;

//    //��ġ�� ��ġ�� �ּ� y��
//    private float yMin = -3.5f;
//    private float yMax = 1.5f;

//    private float xPos = 20.0f;

//    private GameObject[] platforms;

//    private int currentIndex = 0;

//    private Vector2 poolPosition = new Vector2(0, -25f);

//    //������ ��ġ����
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
//        //������ ��ġ�������� timeBetSpawn�̻� �ð��帣��
//        if (Time.time >= lastSpawnTime + timeBetSpawn) 
//        {
//            lastSpawnTime = Time.time;

//            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
//            float yPos = Random.Range(yMin, yMax);

//            platforms[currentIndex].SetActive(false);
//            platforms[currentIndex].SetActive(true);

//            platforms[currentIndex].transform.position = new Vector2 (xPos, yPos);

//            currentIndex++;

//            //�������� �����ϸ� ����
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

    //������ġ������ �ð����� �ּҰ�
    private float timeBetSpawnMin = 1.25f;
    private float timeBetSpawnMax = 2.25f;

    private float timeBetSpawn;
    //������ ��ġ����
    private float lastSpawnTime;

    //��ġ�� ��ġ�� �ּ� y��
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
