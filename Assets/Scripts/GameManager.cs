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
    public TextMeshProUGUI totalCountdownText; // Hiển thị thời gian còn lại của cả ván
    public GameObject gameOverPanel;
    public TextMeshProUGUI runSummaryText;
    public TextMeshProUGUI highestRecordText;
    public TextMeshProUGUI newScoreText;
    public TextMeshProUGUI upgradeResultText;
    [SerializeField] private float holdingsRefreshInterval = 0.2f;
    [Header("Run Time Limit")]
    [SerializeField] private bool useRunTimeLimit = true;
    [SerializeField] private float runTimeLimitSeconds = 180f;

    private float runTimer;
    private float remainingRunTime;
    private float holdingsRefreshTimer;
    private string lastHoldingsText = string.Empty;
    private string lastCountdownText = string.Empty;
    private bool isGameOver;

    void Awake() {
        Instance = this;
    }

    void Start() {
        isGameOver = false;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (OrderManager.Instance != null) {
            OrderManager.Instance.ResetRunStats();
        }

        remainingRunTime = Mathf.Max(0f, runTimeLimitSeconds);
        UpdateUI();
    }

    void Update() {
        if (isGameOver) {
            return;
        }

        runTimer += Time.deltaTime;
        UpdateRunTimeLimit();
        holdingsRefreshTimer -= Time.deltaTime;

        if (holdingsRefreshTimer <= 0f) {
            holdingsRefreshTimer = holdingsRefreshInterval;
            UpdateHoldingsUI();
        }
    }

    private void UpdateRunTimeLimit() {
        if (!useRunTimeLimit) {
            if (totalCountdownText != null && lastCountdownText != string.Empty) {
                lastCountdownText = string.Empty;
                totalCountdownText.text = string.Empty;
            }
            return;
        }

        remainingRunTime = Mathf.Max(0f, remainingRunTime - Time.deltaTime);
        int seconds = Mathf.CeilToInt(remainingRunTime);
        int minutesPart = seconds / 60;
        int secondsPart = seconds % 60;
        string next = string.Format("Time: {0:00}:{1:00}", minutesPart, secondsPart);

        if (totalCountdownText != null && next != lastCountdownText) {
            lastCountdownText = next;
            totalCountdownText.text = next;
        }

        if (remainingRunTime <= 0f && Time.timeScale > 0f) {
            GameOver();
        }
    }

    private void UpdateHoldingsUI() {
        if (holdingsText != null && PlayerController.Instance != null) {
            string newText;
            Cup heldCup = PlayerController.Instance.GetHeldCup();
            if (heldCup != null) {
                var cupContents = heldCup.Contents;
                if (cupContents.Count > 0) {
                    newText = "Holding Cup: " + string.Join(" + ", cupContents);
                } else {
                    newText = "Holding Cup: (empty)";
                }
            } else {
                var holdings = PlayerController.Instance.currentHoldings;
                if (holdings.Count > 0)
                    newText = "Holding: " + string.Join(" + ", holdings);
                else
                    newText = "Holding: (empty)";
            }

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
        if (isGameOver) {
            return;
        }

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
        if (isGameOver) {
            return;
        }

        isGameOver = true;
        Debug.Log("GAME OVER! Final Score: " + score);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Capture highest record before saving this run,
        // so it does not include the run that just ended.
        int highestRecordBeforeThisRun = LocalScoreStorage.GetHighestScore();

        // Save this run to rank history
        int runSeconds = Mathf.FloorToInt(runTimer);
        RankHistory.SaveCurrentRun(score, runSeconds);

        BuildRunSummary(highestRecordBeforeThisRun);
        Time.timeScale = 0f; // Dừng game
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void BackToBackground() {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("BackgroundScene");
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

    private void BuildRunSummary(int highestRecordBeforeThisRun) {
        if (highestRecordText != null) {
            highestRecordText.text = "Highest Record: " + highestRecordBeforeThisRun;
        }

        if (newScoreText != null) {
            newScoreText.text = "New Score: " + score;
        }

        if (runSummaryText == null) {
            return;
        }

        int completed = OrderManager.Instance != null ? OrderManager.Instance.CompletedOrders : 0;
        int failed = OrderManager.Instance != null ? OrderManager.Instance.FailedOrders : 0;
        int total = completed + failed;
        float accuracy = total > 0 ? (completed * 100f) / total : 0f;
        int runSeconds = Mathf.FloorToInt(runTimer);

        runSummaryText.text =
            "Highest Record: " + highestRecordBeforeThisRun + "\n" +
            "New Score: " + score + "\n" +
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

        upgradeResultText.text = success ? successMessage : "Not enough coins for upgrade.";
    }
}
