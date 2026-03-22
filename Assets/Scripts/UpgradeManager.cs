using TMPro;
using UnityEngine;

public class UpgradeManager : MonoBehaviour {
    public static UpgradeManager Instance { get; private set; }

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI upgradeInfoText;

    private const string BrewKey = "upgrade.brew";
    private const string PatienceKey = "upgrade.patience";
    private const string StabilityKey = "upgrade.stability";

    public int BrewLevel { get; private set; }
    public int PatienceLevel { get; private set; }
    public int StabilityLevel { get; private set; }

    public float BrewTimeMultiplier => Mathf.Clamp(1f - BrewLevel * 0.08f, 0.55f, 1f);
    public float CustomerPatienceMultiplier => 1f + PatienceLevel * 0.12f;
    public float ChaosIntervalMultiplier => 1f + StabilityLevel * 0.15f;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BrewLevel = PlayerPrefs.GetInt(BrewKey, 0);
        PatienceLevel = PlayerPrefs.GetInt(PatienceKey, 0);
        StabilityLevel = PlayerPrefs.GetInt(StabilityKey, 0);
    }

    void Start() {
        RefreshUI();
    }

    public int GetBrewUpgradeCost() {
        return 20 + BrewLevel * 10;
    }

    public int GetPatienceUpgradeCost() {
        return 20 + PatienceLevel * 10;
    }

    public int GetStabilityUpgradeCost() {
        return 25 + StabilityLevel * 12;
    }

    public bool BuyBrewUpgrade() {
        int cost = GetBrewUpgradeCost();
        if (!Spend(cost)) {
            return false;
        }

        BrewLevel++;
        PlayerPrefs.SetInt(BrewKey, BrewLevel);
        RefreshUI();
        return true;
    }

    public bool BuyPatienceUpgrade() {
        int cost = GetPatienceUpgradeCost();
        if (!Spend(cost)) {
            return false;
        }

        PatienceLevel++;
        PlayerPrefs.SetInt(PatienceKey, PatienceLevel);
        RefreshUI();
        return true;
    }

    public bool BuyStabilityUpgrade() {
        int cost = GetStabilityUpgradeCost();
        if (!Spend(cost)) {
            return false;
        }

        StabilityLevel++;
        PlayerPrefs.SetInt(StabilityKey, StabilityLevel);
        RefreshUI();
        return true;
    }

    public void RefreshUI() {
        if (upgradeInfoText == null) {
            return;
        }

        upgradeInfoText.text =
            "UPGRADES\n" +
            "Brew Lv." + BrewLevel + " (Cost " + GetBrewUpgradeCost() + ")\n" +
            "Patience Lv." + PatienceLevel + " (Cost " + GetPatienceUpgradeCost() + ")\n" +
            "Stability Lv." + StabilityLevel + " (Cost " + GetStabilityUpgradeCost() + ")";
    }

    private bool Spend(int amount) {
        if (GameManager.Instance == null) {
            return false;
        }

        return GameManager.Instance.SpendCoins(amount);
    }
}
