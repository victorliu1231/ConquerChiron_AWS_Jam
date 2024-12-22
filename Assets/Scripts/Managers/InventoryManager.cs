using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager Instance;
    public List<InventoryItem> inventoryItems;

    void Awake(){
        inventoryItems = new List<InventoryItem>();
    }

    public void AddItem(InventoryItem inventoryItem){
        inventoryItems.Add(inventoryItem);
    }
}