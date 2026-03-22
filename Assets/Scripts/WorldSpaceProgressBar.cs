using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceProgressBar : MonoBehaviour {
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(100f, 16f);

    private Canvas canvas;
    private Image fillImage;
    private Camera cachedCamera;
    private bool isVisible;

    void Awake() {
        BuildUI();
        SetVisible(false);
    }

    void LateUpdate() {
        if (followTarget == null) {
            return;
        }

        transform.position = followTarget.position + worldOffset;

        if (cachedCamera == null) {
            cachedCamera = Camera.main;
        }

        if (cachedCamera != null) {
            transform.forward = cachedCamera.transform.forward;
        }
    }

    public void SetFollowTarget(Transform target) {
        followTarget = target;
    }

    public void SetVisible(bool isVisible) {
        if (canvas == null || this.isVisible == isVisible) {
            return;
        }

        this.isVisible = isVisible;
        canvas.enabled = isVisible;
    }

    public void SetProgress(float value01) {
        if (fillImage != null) {
            fillImage.fillAmount = Mathf.Clamp01(value01);
        }
    }

    private void BuildUI() {
        canvas = GetComponent<Canvas>();
        if (canvas == null) {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 20;

        if (GetComponent<CanvasScaler>() == null) {
            gameObject.AddComponent<CanvasScaler>();
        }

        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = barSize;

        GameObject background = new GameObject("BarBackground", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(transform, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0f);
        backgroundRect.anchorMax = new Vector2(1f, 1f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject fill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(background.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        fillImage = fill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.color = new Color(0.26f, 0.86f, 0.38f, 1f);
        fillImage.fillAmount = 1f;
    }
}
