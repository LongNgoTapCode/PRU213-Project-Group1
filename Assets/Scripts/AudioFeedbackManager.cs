using System.Collections;
using UnityEngine;

public class AudioFeedbackManager : MonoBehaviour {
    public static AudioFeedbackManager Instance { get; private set; }

    [Header("Global SFX")]
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private AudioClip defaultGrinderLoop;
    [SerializeField] private AudioClip orderCompleteChime;
    [SerializeField] private AudioClip incidentAlert;

    public AudioClip DefaultGrinderLoop => defaultGrinderLoop;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiSfxSource == null) {
            uiSfxSource = gameObject.AddComponent<AudioSource>();
            uiSfxSource.playOnAwake = false;
            uiSfxSource.spatialBlend = 0f;
        }
    }

    public void PlayOrderCompleteChime() {
        if (orderCompleteChime == null || uiSfxSource == null) {
            return;
        }

        StartCoroutine(PlayDoubleChime());
    }

    public void PlayIncidentAlert() {
        if (incidentAlert == null || uiSfxSource == null) {
            return;
        }

        uiSfxSource.PlayOneShot(incidentAlert);
    }

    private IEnumerator PlayDoubleChime() {
        uiSfxSource.PlayOneShot(orderCompleteChime);
        yield return new WaitForSeconds(0.14f);
        uiSfxSource.PlayOneShot(orderCompleteChime);
    }
}
