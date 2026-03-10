using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int score = 0;
    public int reputation = 5; // Bắt đầu với 5 mạng
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI holdingsText; // Hiển thị nguyên liệu đang cầm
    public GameObject gameOverPanel;

    void Awake() {
        Instance = this;
    }

    void Start() {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        UpdateUI();
    }

    void Update() {
        // Cập nhật hiển thị nguyên liệu đang cầm
        if (holdingsText != null && PlayerController.Instance != null) {
            var holdings = PlayerController.Instance.currentHoldings;
            if (holdings.Count > 0)
                holdingsText.text = "Holding: " + string.Join(" + ", holdings);
            else
                holdingsText.text = "Holding: (empty)";
        }
    }

    public void AddScore(int amount) {
        score += amount;
        Debug.Log("Score: " + score);
        UpdateUI();
    }

    public void LoseReputation() {
        reputation--;
        Debug.Log("Reputation: " + reputation);
        UpdateUI();

        if (reputation <= 0) {
            GameOver();
        }
    }

    void UpdateUI() {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        if (reputationText != null)
            reputationText.text = "Rep: " + reputation + " / 5";
    }

    void GameOver() {
        Debug.Log("GAME OVER! Final Score: " + score);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Dừng game
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
