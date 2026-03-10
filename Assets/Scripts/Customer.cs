using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour {
    public DrinkData order;
    public float maxPatience = 20f;
    private float currentPatience;

    private SpriteRenderer barBackground;
    private SpriteRenderer barFill;
    private TextMesh orderLabel;
    private float barWidth = 1.2f;
    private float barHeight = 0.15f;

    void Awake() {
        // Tự động thêm Collider nếu chưa có
        if (GetComponent<Collider2D>() == null) {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            Debug.Log("Customer: Auto-added BoxCollider2D");
        }
    }

    void Start() {
        currentPatience = maxPatience;
        CreatePatienceBar();
        CreateOrderLabel();
    }

    void CreatePatienceBar() {
        // Background (xám đen)
        GameObject bgObj = new GameObject("BarBG");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        barBackground = bgObj.AddComponent<SpriteRenderer>();
        barBackground.sprite = CreateSquareSprite();
        barBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        barBackground.sortingOrder = 5;
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // Fill (xanh lá)
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        barFill = fillObj.AddComponent<SpriteRenderer>();
        barFill.sprite = CreateSquareSprite();
        barFill.color = Color.green;
        barFill.sortingOrder = 6;
        fillObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
    }

    void CreateOrderLabel() {
        GameObject textObj = new GameObject("OrderLabel");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 1.1f, 0);
        orderLabel = textObj.AddComponent<TextMesh>();
        orderLabel.alignment = TextAlignment.Center;
        orderLabel.anchor = TextAnchor.MiddleCenter;
        orderLabel.characterSize = 0.12f;
        orderLabel.fontSize = 48;
        orderLabel.color = Color.white;
        orderLabel.GetComponent<MeshRenderer>().sortingOrder = 7;

        if (order != null)
            orderLabel.text = order.drinkName;
    }

    Sprite CreateSquareSprite() {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void Update() {
        currentPatience -= Time.deltaTime;

        if (barFill != null) {
            float ratio = Mathf.Clamp01(currentPatience / maxPatience);
            barFill.transform.localScale = new Vector3(barWidth * ratio, barHeight, 1f);
            // Giữ thanh fill căn trái
            float offset = (barWidth - barWidth * ratio) * 0.5f;
            barFill.transform.localPosition = new Vector3(-offset, 0.8f, 0);

            // Đổi màu
            if (ratio > 0.5f) barFill.color = Color.green;
            else if (ratio > 0.25f) barFill.color = Color.yellow;
            else barFill.color = Color.red;
        }

        if (currentPatience <= 0) {
            LeaveAngry();
        }
    }

    public bool ReceiveDrink(List<string> servedIngredients) {
        if (order == null) return false;
        if (IsMatch(servedIngredients)) {
            Debug.Log("Served " + order.drinkName + " Successfully! +" + order.basePrice + " coins");
            GameManager.Instance?.AddScore((int)order.basePrice);
            Destroy(gameObject);
            return true;
        } else {
            Debug.Log("Wrong order! Customer wants: " + string.Join(", ", order.ingredients));
            return false;
        }
    }

    void LeaveAngry() {
        Debug.Log("Customer left... Reputation down!");
        GameManager.Instance?.LoseReputation();
        Destroy(gameObject);
    }

    bool IsMatch(List<string> served) {
        if (order == null || served.Count != order.ingredients.Count) return false;
        for (int i = 0; i < served.Count; i++) {
            if (served[i] != order.ingredients[i]) return false;
        }
        return true;
    }
}