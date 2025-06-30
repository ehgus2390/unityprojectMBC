using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource�� ���ٸ� �ڵ����� �߰�
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = true;
        }
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (musicSource.clip == music) return;

        musicSource.clip = music;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }
}
