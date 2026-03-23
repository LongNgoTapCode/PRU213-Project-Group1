using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

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
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // 3. Chức năng Back: Quay lại Menu chính từ Settings
    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // 4. Chức năng Quit: Thoát game
    public void QuitGame()
    {
        Debug.Log("Game đã thoát!"); // Kiểm tra trong Console khi đang chạy thử
        Application.Quit();
    }
}