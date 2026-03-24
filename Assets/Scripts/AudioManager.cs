using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private const string MusicVolumeKey = "audio.musicVolume";
    private const string SfxVolumeKey = "audio.sfxVolume";
    private const float DefaultVolume = 1f;

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
        LoadSavedVolumes();
        PlayBackgroundMusic();
    }

    private void LoadSavedVolumes()
    {
        float music = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        float sfx = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
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
        volume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }

        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
    }

    public float GetMusicVolume()
    {
        if (musicSource != null)
        {
            return musicSource.volume;
        }

        return PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
    }

    public float GetSFXVolume()
    {
        if (sfxSource != null)
        {
            return sfxSource.volume;
        }

        return PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);
    }
}