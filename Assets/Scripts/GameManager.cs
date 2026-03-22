using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int score = 0;
    public int coins = 0; // Tiền tệ chính cho upgrade
    public int reputation = 5; // Bắt đầu với 5 mạng
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI holdingsText; // Hiển thị nguyên liệu đang cầm
    public GameObject gameOverPanel;
    public TextMeshProUGUI runSummaryText;
    public TextMeshProUGUI upgradeResultText;
    [SerializeField] private float holdingsRefreshInterval = 0.2f;

    private float runTimer;
    private float holdingsRefreshTimer;
    private string lastHoldingsText = string.Empty;

    void Awake() {
        Instance = this;
    }

    void Start() {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (OrderManager.Instance != null) {
            OrderManager.Instance.ResetRunStats();
        }

        UpdateUI();
    }

    void Update() {
        runTimer += Time.deltaTime;
        holdingsRefreshTimer -= Time.deltaTime;

        if (holdingsRefreshTimer <= 0f) {
            holdingsRefreshTimer = holdingsRefreshInterval;
            UpdateHoldingsUI();
        }
    }

    private void UpdateHoldingsUI() {
        if (holdingsText != null && PlayerController.Instance != null) {
            var holdings = PlayerController.Instance.currentHoldings;
            string newText;
            if (holdings.Count > 0)
                newText = "Holding: " + string.Join(" + ", holdings);
            else
                newText = "Holding: (empty)";

            if (newText != lastHoldingsText) {
                lastHoldingsText = newText;
                holdingsText.text = newText;
            }
        }
    }

    public void AddScore(int amount) {
        score += amount;
        Debug.Log("Score: " + score);
        UpdateUI();
    }

    public void AddCoins(int amount) {
        coins += Mathf.Max(0, amount);
        Debug.Log("Coins: " + coins);
        UpdateUI();
    }

    public bool SpendCoins(int amount) {
        if (coins < amount) {
            return false;
        }

        coins -= amount;
        UpdateUI();
        return true;
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
        if (coinsText != null)
            coinsText.text = "Coins: " + coins;
        if (reputationText != null)
            reputationText.text = "Rep: " + reputation + " / 5";
    }

    void GameOver() {
        Debug.Log("GAME OVER! Final Score: " + score);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        BuildRunSummary();
        Time.timeScale = 0f; // Dừng game
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void BuyUpgradeBrew() {
        TryBuyUpgrade(UpgradeManager.Instance != null && UpgradeManager.Instance.BuyBrewUpgrade(), "Brew speed upgrade purchased.");
    }

    public void BuyUpgradePatience() {
        TryBuyUpgrade(UpgradeManager.Instance != null && UpgradeManager.Instance.BuyPatienceUpgrade(), "Customer patience upgrade purchased.");
    }

    public void BuyUpgradeStability() {
        TryBuyUpgrade(UpgradeManager.Instance != null && UpgradeManager.Instance.BuyStabilityUpgrade(), "Stability upgrade purchased.");
    }

    private void BuildRunSummary() {
        if (runSummaryText == null) {
            return;
        }

        int completed = OrderManager.Instance != null ? OrderManager.Instance.CompletedOrders : 0;
        int failed = OrderManager.Instance != null ? OrderManager.Instance.FailedOrders : 0;
        int total = completed + failed;
        float accuracy = total > 0 ? (completed * 100f) / total : 0f;
        int runSeconds = Mathf.FloorToInt(runTimer);

        runSummaryText.text =
            "Run Result\n" +
            "Coins: " + coins + "\n" +
            "Completed: " + completed + "\n" +
            "Failed: " + failed + "\n" +
            "Accuracy: " + accuracy.ToString("F0") + "%\n" +
            "Time: " + runSeconds + "s";
    }

    private void TryBuyUpgrade(bool success, string successMessage) {
        if (upgradeResultText == null) {
            return;
        }

        upgradeResultText.text = success ? successMessage : "Not enough score for upgrade.";
    }
}
