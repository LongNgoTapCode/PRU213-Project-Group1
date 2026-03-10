using UnityEngine;
using System.Collections;

public class Workstation : MonoBehaviour {
    public string ingredientOutput;
    public float processTime = 3f;
    public bool isProcessing = false;
    public bool isReady = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

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
    }

    // Được gọi từ PlayerController khi click vào
    public void HandleClick() {
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

        yield return new WaitForSeconds(processTime);

        isProcessing = false;
        isReady = true;
        Debug.Log(ingredientOutput + " ready!");
        // Hiệu ứng: đổi màu xanh khi sẵn sàng lấy
        if (spriteRenderer != null)
            spriteRenderer.color = Color.green;
    }

    void CollectResult() {
        if (PlayerController.Instance != null) {
            PlayerController.Instance.AddIngredient(ingredientOutput);
        } else {
            Debug.LogWarning("PlayerController not found.");
        }
        isReady = false;
        // Trở về màu gốc
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}