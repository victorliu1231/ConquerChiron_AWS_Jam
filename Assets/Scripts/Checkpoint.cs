using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint {
    public Vector3 position;
    public Quaternion rotation;
    public Task task;
    public List<Item> equippedItems;
    public List<int> itemCounts;
    public List<Item> items;

    public Checkpoint(Vector3 position, Quaternion rotation, Task task, List<Item> equippedItems, ItemSlot[] inventoryItemSlots) {
        this.position = position + Vector3.zero; // make Vector3 clone
        this.rotation = rotation * Quaternion.identity; // make Quaternion clone
        this.task = task;
        this.equippedItems = new List<Item>(equippedItems); // clone of old list
        this.itemCounts = new List<int>();
        this.items = new List<Item>();
        for (int i = 0; i < inventoryItemSlots.Length; i++) {
            itemCounts.Add(inventoryItemSlots[i].itemCount);
            items.Add(inventoryItemSlots[i].slotItem);
        }
    }
}