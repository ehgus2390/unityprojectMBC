using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] cubePrefabs;
    [SerializeField] private Transform[] points;

    [Header("BPM")]
    public double BPM = 158d;

    private double Beat
    {
        get
        {
            return 60d / BPM;
        }
    }

    public double Timer;

    private void Start()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
        Timer = 0d;

    }

    private void Update()
    {
        if (Timer > Beat)
        {
            int randomIndex = Random.Range(0, 2);
            GameObject cube = Instantiate(cubePrefabs[randomIndex]);

            float y = Random.Range(0.5f, 1.5f);

            cube.transform.position = new Vector3
                (points[randomIndex].transform.position.x, y,
                points[randomIndex].transform.position.z);

            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));

            Timer -= Beat;

            AudioManager.Instance.PlayBGM();
        }
        Timer += Time.deltaTime;
    }
}