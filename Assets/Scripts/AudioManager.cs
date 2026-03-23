using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("---- Audio Sources ----")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("---- Audio Clips ----")]
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip angrySound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ object này khi đổi Scene
        }
        else
        {
            // Nếu đã có một instance khác tồn tại (khi quay lại Menu), xóa bản mới đi
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null || backgroundMusic == null) return;

        // Chỉ phát nếu nhạc chưa được phát (tránh reset nhạc khi chuyển scene)
        if (!musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void OnClickSound()
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void PlayAngrySound()
    {
        if (angrySound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(angrySound);
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    // Thêm hàm này vào cuối class AudioManager
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}