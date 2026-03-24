using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject rankPanel;

    [Header("Option Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider clickVolumeSlider;

    [Header("History Display (Optional)")]
    [SerializeField] private RankHistoryUI rankHistoryUI;
    [SerializeField] private BackgroundHistoryPreview backgroundHistoryPreview;

    void Start()
    {
        rankHistoryUI?.Refresh();
        backgroundHistoryPreview?.Refresh();
        SetupVolumeSliders();
        ValidateReferences();
    }

    // 1. Chức năng Play: Chuyển sang Scene tiếp theo
    public void PlayGame()
    {
        // Load scene có index là 1 trong Build Settings
        // Hoặc thay bằng tên Scene: SceneManager.LoadScene("GameScene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
       // SceneManager.LoadScene("SampleScene");
    }

    // 2. Chức năng Settings: Mở bảng cài đặt
    public void OpenSettings()
    {
        SetPanelActive(settingsPanel, true, nameof(settingsPanel));
        SyncSliderValuesFromAudio();
    }


    // 3. Chức năng Back: Quay lại Menu chính từ Settings
    public void CloseSettings()
    {
        SetPanelActive(settingsPanel, false, nameof(settingsPanel));
    }

    // 5. Chức năng Rank: Mở bảng xếp hạng Top 10
    public void OpenRank()
    {
        SetPanelActive(rankPanel, true, nameof(rankPanel));
        rankHistoryUI?.Refresh();
    }

    // 6. Chức năng Back: Quay lại Menu chính từ Rank
    public void CloseRank()
    {
        SetPanelActive(rankPanel, false, nameof(rankPanel));
    }

    // 4. Chức năng Quit: Thoát game
    public void QuitGame()
    {
        Debug.Log("Game đã thoát!"); // Kiểm tra trong Console khi đang chạy thử
        Application.Quit();
    }

    private void SetPanelActive(GameObject panel, bool isActive, string fieldName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"[MainMenu] Missing reference: {fieldName}. Please assign it in Inspector.", this);
            return;
        }

        panel.SetActive(isActive);
    }

    private void ValidateReferences()
    {
        if (settingsPanel == null)
            Debug.LogWarning("[MainMenu] settingsPanel is not assigned.", this);
        if (rankPanel == null)
            Debug.LogWarning("[MainMenu] rankPanel is not assigned.", this);
    }

    private void SetupVolumeSliders()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (clickVolumeSlider != null)
        {
            clickVolumeSlider.minValue = 0f;
            clickVolumeSlider.maxValue = 1f;
            clickVolumeSlider.onValueChanged.AddListener(OnClickVolumeChanged);
        }

        SyncSliderValuesFromAudio();
    }

    private void SyncSliderValuesFromAudio()
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(AudioManager.instance.GetMusicVolume());
        }

        if (clickVolumeSlider != null)
        {
            clickVolumeSlider.SetValueWithoutNotify(AudioManager.instance.GetSFXVolume());
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.instance?.SetMusicVolume(value);
    }

    public void OnClickVolumeChanged(float value)
    {
        AudioManager.instance?.SetSFXVolume(value);
    }
}