using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public Transform spawnPoint; // 몬스터가 생성될 위치
    public Wave[] waves;         // 웨이브 데이터 배열
    private int currentWave = 0;
    [System.Serializable]
    public class Wave
    {
        public GameObject[] monsterPrefabs; // 이 웨이브에서 등장할 몬스터 프리팹들
        public int[] monsterCounts;         // 각 몬스터별 등장 수
        public float spawnInterval = 1.0f;  // 몬스터 스폰 간격
                                            // 난이도 조절용 파라미터(체력, 속도 등)도 추가 가능
    }
    void Start()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        while (currentWave < waves.Length)
        {
            Wave wave = waves[currentWave];
            for (int i = 0; i < wave.monsterPrefabs.Length; i++)
            {
                for (int j = 0; j < wave.monsterCounts[i]; j++)
                {
                    Instantiate(wave.monsterPrefabs[i], spawnPoint.position, Quaternion.identity);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
            // 웨이브 종료 후 대기(예: 모든 몬스터가 죽을 때까지 기다리기)
            yield return new WaitForSeconds(5f); // 예시: 5초 대기
            currentWave++;
        }
    }
}