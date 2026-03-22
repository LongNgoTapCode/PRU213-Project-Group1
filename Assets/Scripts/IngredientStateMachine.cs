using UnityEngine;

public enum IngredientState {
    Raw,
    Processed,
    Cooking,
    Burnt
}

public class IngredientStateMachine : MonoBehaviour {
    [SerializeField] private IngredientState currentState = IngredientState.Raw;
    [SerializeField] private float burnAfterSeconds = 6f;
    [SerializeField] private WorldSpaceProgressBar progressBar;
    [SerializeField] private ParticleSystem steamVfx;
    [SerializeField] private bool processedStateIsHot = true;

    private float cookingTimer;

    public IngredientState CurrentState => currentState;

    void Update() {
        if (currentState != IngredientState.Cooking) {
            if (progressBar != null) {
                progressBar.SetVisible(false);
            }
            UpdateSteamState();
            return;
        }

        if (progressBar != null) {
            progressBar.SetVisible(true);
            progressBar.SetProgress(Mathf.Clamp01(cookingTimer / burnAfterSeconds));
        }

        cookingTimer += Time.deltaTime;
        if (cookingTimer >= burnAfterSeconds) {
            SetState(IngredientState.Burnt);
        }

        UpdateSteamState();
    }

    public void SetState(IngredientState newState) {
        currentState = newState;

        if (progressBar != null) {
            progressBar.SetFollowTarget(transform);
            if (newState != IngredientState.Cooking) {
                progressBar.SetVisible(false);
            }
        }

        if (newState != IngredientState.Cooking) {
            cookingTimer = 0f;
        }

        UpdateSteamState();
    }

    public bool TryProcess() {
        if (currentState != IngredientState.Raw) {
            return false;
        }

        SetState(IngredientState.Processed);
        return true;
    }

    public bool TryStartCooking() {
        if (currentState != IngredientState.Processed) {
            return false;
        }

        cookingTimer = 0f;
        SetState(IngredientState.Cooking);
        return true;
    }

    private void UpdateSteamState() {
        if (steamVfx == null) {
            return;
        }

        bool shouldPlay = currentState == IngredientState.Cooking || (processedStateIsHot && currentState == IngredientState.Processed);

        if (shouldPlay && !steamVfx.isPlaying) {
            steamVfx.Play();
        } else if (!shouldPlay && steamVfx.isPlaying) {
            steamVfx.Stop();
        }
    }
}
