using UnityEngine;

[CreateAssetMenu(fileName = "NewCup", menuName = "DesolateCoffee/Cup")]
public class CupData : ScriptableObject {
    public string cupName = "Basic Cup";
    public Sprite cupSprite;
    public float capacity = 1f; // Số lượng ml hoặc unit
    public Color cupColor = Color.white;
}
