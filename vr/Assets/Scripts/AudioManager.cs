using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] private GameObject BGM_g;

    private AudioSource audio_BGM;

    private void Start()
    {
        audio_BGM = BGM_g.GetComponent<AudioSource>();
    }

    public void PlayBGM()
    {
        if (!audio_BGM.isPlaying)
        {
            audio_BGM.Play();
        }
    }
}