using UnityEngine;
using System.Collections;

public class Workstation : MonoBehaviour {
    public string ingredientOutput;
    public float processTime = 3f;
    public bool isProcessing = false;
    public bool isReady = false;

    [Header("Output")]
    [SerializeField] private bool spawnPickupItem = true;
    [SerializeField] private PickupItem outputPrefab;
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Ingredient State Actions")]
    [SerializeField] private bool processHeldIngredient;
    [SerializeField] private bool startCookingHeldIngredient;
    [SerializeField] private WorldSpaceProgressBar progressBar;
    [SerializeField] private float milkSpillBrewMultiplier = 1.6f;
    [Header("VFX/SFX & Animation")]
    [SerializeField] private Transform shakeVisual;
    [SerializeField] private float shakeAmount = 0.03f;
    [SerializeField] private float shakeFrequency = 28f;
    [SerializeField] private AudioSource grinderLoopSource;
    [SerializeField] private AudioClip grinderLoopClip;
    [Header("Incident UI & Repair")]
    [SerializeField] private TextMesh incidentIconText;
    [SerializeField] private Vector3 incidentIconOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private float repairHoldSeconds = 2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private WorkstationIncidentType activeIncident = WorkstationIncidentType.None;
    private float activeBrewMultiplier = 1f;
    private Vector3 shakeOriginalLocalPosition;
    private float repairHoldProgress;
    private bool isRepairing;

    public bool HasActiveIncident => activeIncident != WorkstationIncidentType.None;
    public WorkstationIncidentType ActiveIncident => activeIncident;
    public float RepairProgress01 => Mathf.Clamp01(repairHoldProgress / Mathf.Max(0.01f, repairHoldSeconds));

    void Awake() {
        // Tự động thêm Collider nếu chưa có
        if (GetComponent<Collider2D>() == null) {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            Debug.Log("Workstation: Auto-added BoxCollider2D to " + gameObject.name);
        }
    }

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (shakeVisual == null) {
            shakeVisual = transform;
        }
        shakeOriginalLocalPosition = shakeVisual.localPosition;

        if (grinderLoopSource == null) {
            grinderLoopSource = GetComponent<AudioSource>();
        }

        if (grinderLoopSource == null) {
            grinderLoopSource = gameObject.AddComponent<AudioSource>();
            grinderLoopSource.playOnAwake = false;
            grinderLoopSource.loop = true;
            grinderLoopSource.spatialBlend = 1f;
        }

        if (grinderLoopClip == null && AudioFeedbackManager.Instance != null) {
            grinderLoopClip = AudioFeedbackManager.Instance.DefaultGrinderLoop;
        }

        if (progressBar == null) {
            progressBar = GetComponentInChildren<WorldSpaceProgressBar>();
        }

        if (progressBar != null) {
            progressBar.SetFollowTarget(transform);
            progressBar.SetVisible(false);
        }

        EnsureIncidentIcon();
        UpdateIncidentIcon();
    }

    // Được gọi từ PlayerController khi click vào
    public void HandleClick() {
        if (activeIncident == WorkstationIncidentType.MachineBroken || activeIncident == WorkstationIncidentType.OutOfBeans) {
            Debug.LogWarning(name + " cannot be used: " + activeIncident);
            return;
        }

        if (TryHandleHeldIngredientState()) {
            return;
        }

        if (!isProcessing && !isReady) {
            StartCoroutine(StartBrewing());
        } else if (isReady) {
            CollectResult();
        }
    }

    IEnumerator StartBrewing() {
        isProcessing = true;
        Debug.Log(ingredientOutput + " processing...");
        // Hiệu ứng: đổi màu vàng khi đang pha
        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;

        float elapsed = 0f;
        float targetDuration = processTime * activeBrewMultiplier;
        if (UpgradeManager.Instance != null) {
            targetDuration *= UpgradeManager.Instance.BrewTimeMultiplier;
        }

        if (progressBar != null) {
            progressBar.SetVisible(true);
            progressBar.SetProgress(0f);
        }

        StartGrinderAudio();

        while (elapsed < targetDuration) {
            if (activeIncident == WorkstationIncidentType.MachineBroken || activeIncident == WorkstationIncidentType.OutOfBeans) {
                isProcessing = false;
                isReady = false;
                if (progressBar != null) {
                    progressBar.SetVisible(false);
                }
                StopGrinderAudio();
                StopShake();
                Debug.LogWarning(name + " brewing interrupted by " + activeIncident);
                if (spriteRenderer != null) {
                    spriteRenderer.color = Color.red;
                }
                yield break;
            }

            elapsed += Time.deltaTime;
            if (progressBar != null) {
                progressBar.SetProgress(Mathf.Clamp01(elapsed / targetDuration));
            }

            UpdateShake();

            yield return null;
        }

        isProcessing = false;
        isReady = true;
        StopGrinderAudio();
        StopShake();
        Debug.Log(ingredientOutput + " ready!");
        // Hiệu ứng: đổi màu xanh khi sẵn sàng lấy
        if (spriteRenderer != null)
            spriteRenderer.color = Color.green;

        if (progressBar != null) {
            progressBar.SetVisible(false);
        }
    }

    void CollectResult() {
        if (spawnPickupItem && outputPrefab != null) {
            Transform targetSpawn = outputSpawnPoint != null ? outputSpawnPoint : transform;
            PickupItem spawned = Instantiate(outputPrefab, targetSpawn.position, Quaternion.identity);
            spawned.SetIngredientName(ingredientOutput);

            IngredientStateMachine stateMachine = spawned.GetComponent<IngredientStateMachine>();
            if (stateMachine != null) {
                stateMachine.SetState(IngredientState.Processed);
            }

            if (PlayerController.Instance != null && !PlayerController.Instance.HasHeldItem()) {
                PlayerController.Instance.TryPickupItem(spawned);
            }
        } else if (PlayerController.Instance != null) {
            PlayerController.Instance.AddIngredient(ingredientOutput);
        } else {
            Debug.LogWarning("PlayerController not found.");
        }

        isReady = false;
        // Trở về màu gốc
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private bool TryHandleHeldIngredientState() {
        if (activeIncident == WorkstationIncidentType.MachineBroken) {
            return true;
        }

        if (PlayerController.Instance == null || !PlayerController.Instance.HasHeldItem()) {
            return false;
        }

        PickupItem held = PlayerController.Instance.GetHeldItem();
        if (held == null) {
            return false;
        }

        IngredientStateMachine stateMachine = held.GetComponent<IngredientStateMachine>();
        if (stateMachine == null) {
            return false;
        }

        if (processHeldIngredient && stateMachine.TryProcess()) {
            Debug.Log("Ingredient processed: " + held.IngredientName);
            return true;
        }

        if (startCookingHeldIngredient && stateMachine.TryStartCooking()) {
            Debug.Log("Ingredient started cooking: " + held.IngredientName);
            return true;
        }

        if (startCookingHeldIngredient && stateMachine.CurrentState == IngredientState.Burnt) {
            Debug.Log("Ingredient is already burnt: " + held.IngredientName);
            return true;
        }

        return false;
    }

    public bool TryTriggerIncident(WorkstationIncidentType incidentType, float durationSeconds) {
        if (incidentType == WorkstationIncidentType.None || activeIncident != WorkstationIncidentType.None) {
            return false;
        }

        StartCoroutine(RunIncident(incidentType, durationSeconds));
        return true;
    }

    private IEnumerator RunIncident(WorkstationIncidentType incidentType, float durationSeconds) {
        activeIncident = incidentType;
        isRepairing = false;
        repairHoldProgress = 0f;

        if (incidentType == WorkstationIncidentType.MilkSpill) {
            activeBrewMultiplier = milkSpillBrewMultiplier;
        } else {
            activeBrewMultiplier = 1f;
        }

        Debug.LogWarning(name + " incident started: " + incidentType);
        AudioFeedbackManager.Instance?.PlayIncidentAlert();
        if (spriteRenderer != null) {
            spriteRenderer.color = new Color(1f, 0.45f, 0.15f, 1f);
        }

        UpdateIncidentIcon();

        float remaining = durationSeconds;
        while (remaining > 0f && activeIncident == incidentType) {
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (activeIncident == incidentType) {
            ResolveIncidentState();
            Debug.Log(name + " incident resolved.");
        }
    }

    public bool CanRepairIncident() {
        return activeIncident != WorkstationIncidentType.None;
    }

    public void BeginRepair() {
        if (!CanRepairIncident()) {
            return;
        }

        isRepairing = true;
        if (!isProcessing && progressBar != null) {
            progressBar.SetVisible(true);
            progressBar.SetProgress(RepairProgress01);
        }
    }

    public void UpdateRepair(float deltaTime) {
        if (!isRepairing || !CanRepairIncident()) {
            return;
        }

        repairHoldProgress += deltaTime;

        if (!isProcessing && progressBar != null) {
            progressBar.SetVisible(true);
            progressBar.SetProgress(RepairProgress01);
        }

        if (repairHoldProgress >= repairHoldSeconds) {
            Debug.Log(name + " repaired by player.");
            ResolveIncidentState();
            AudioFeedbackManager.Instance?.PlayOrderCompleteChime();
        }
    }

    public void CancelRepair() {
        isRepairing = false;
        repairHoldProgress = 0f;
        if (!isProcessing && progressBar != null) {
            progressBar.SetVisible(false);
        }
    }

    private void ResolveIncidentState() {
        activeIncident = WorkstationIncidentType.None;
        activeBrewMultiplier = 1f;
        isRepairing = false;
        repairHoldProgress = 0f;
        UpdateIncidentIcon();

        if (!isProcessing && progressBar != null) {
            progressBar.SetVisible(false);
        }

        if (spriteRenderer != null) {
            if (isReady) {
                spriteRenderer.color = Color.green;
            } else {
                spriteRenderer.color = originalColor;
            }
        }
    }

    private void EnsureIncidentIcon() {
        if (incidentIconText != null) {
            return;
        }

        GameObject iconObj = new GameObject("IncidentIcon");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = incidentIconOffset;
        incidentIconText = iconObj.AddComponent<TextMesh>();
        incidentIconText.alignment = TextAlignment.Center;
        incidentIconText.anchor = TextAnchor.MiddleCenter;
        incidentIconText.characterSize = 0.12f;
        incidentIconText.fontSize = 52;
        incidentIconText.color = new Color(1f, 0.35f, 0.15f, 1f);
        incidentIconText.text = string.Empty;

        MeshRenderer meshRenderer = incidentIconText.GetComponent<MeshRenderer>();
        if (meshRenderer != null) {
            meshRenderer.sortingOrder = 20;
        }
    }

    private void UpdateIncidentIcon() {
        if (incidentIconText == null) {
            return;
        }

        incidentIconText.transform.localPosition = incidentIconOffset;

        if (activeIncident == WorkstationIncidentType.None) {
            incidentIconText.gameObject.SetActive(false);
            incidentIconText.text = string.Empty;
            return;
        }

        incidentIconText.gameObject.SetActive(true);
        if (activeIncident == WorkstationIncidentType.MachineBroken) {
            incidentIconText.text = "W";
        } else if (activeIncident == WorkstationIncidentType.MilkSpill) {
            incidentIconText.text = "M";
        } else if (activeIncident == WorkstationIncidentType.OutOfBeans) {
            incidentIconText.text = "B";
        } else {
            incidentIconText.text = "!";
        }
    }

    private void StartGrinderAudio() {
        if (grinderLoopSource == null) {
            return;
        }

        if (grinderLoopClip != null) {
            grinderLoopSource.clip = grinderLoopClip;
        }

        if (grinderLoopSource.clip != null && !grinderLoopSource.isPlaying) {
            grinderLoopSource.Play();
        }
    }

    private void StopGrinderAudio() {
        if (grinderLoopSource != null && grinderLoopSource.isPlaying) {
            grinderLoopSource.Stop();
        }
    }

    private void UpdateShake() {
        if (shakeVisual == null) {
            return;
        }

        float t = Time.time * shakeFrequency;
        Vector3 offset = new Vector3(Mathf.Sin(t), Mathf.Cos(t * 0.7f), 0f) * shakeAmount;
        shakeVisual.localPosition = shakeOriginalLocalPosition + offset;
    }

    private void StopShake() {
        if (shakeVisual != null) {
            shakeVisual.localPosition = shakeOriginalLocalPosition;
        }
    }
}