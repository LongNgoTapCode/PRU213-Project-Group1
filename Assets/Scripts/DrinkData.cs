using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDrink", menuName = "DesolateCoffee/Drink")]
public class DrinkData : ScriptableObject {
    public string drinkName;
    public List<string> ingredients; // VD: "Espresso", "Milk", "Sugar"
    public float basePrice;
    public Sprite icon;
}