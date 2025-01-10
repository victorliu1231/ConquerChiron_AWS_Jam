using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Events;

    //This script contains the all the different item references and different types of item use events.
    //This script must exist in the scene for the saving/loading/item-use system to work properly.
    //NOTE: Only one reference should exist in the scene at once.

    public class ItemManager : MonoBehaviour
    {
        public InteractionSettings interactionSettings;

        //Singleton instance of this script.
        public static ItemManager Instance { get; private set; }

        //List of all the item scriptable obects.
        //Either assign the items manually when created or select the item scriptable object > right click > select Add To Item List 
        public List<Item> itemList = new List<Item>();

        [Header("My Code")]
        public ItemContainer inventory;
        public List<Item> equippedItems;
        public UnityEvent<Item> OnEquip;
        public UnityEvent<Item> OnUnequip;

        private void Awake()
        {
            //Singleton logic
            #region Singleton
            if (Instance == null)
            {
                Instance = this;
                equippedItems = new List<Item>();
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            #endregion

            //Any code in awake should be after the singleton evaluation
        }

        //This function is called when the Use Item button is clicked from one of the inventory items.
        public void UseItem(ItemSlot slot) 
        {
            if (slot.IsEmpty) return;

            //Add custom item functions ###################################################################################################
            switch (slot.slotItem.type) 
            {
                default: DefaultItemUse(slot); break;
                case ItemType.ToolOrWeapon: EquipItem(slot); break;
                case ItemType.Placeable: PlaceItem(slot); break;
                case ItemType.Consumeable: ConsumeItem(slot); break;
                case ItemType:Unusable: break;
            }
        }

        //ADD CUSTOM ITEM TYPE USE METHOD HERE.
        //The custom item use method should take ItemSlot as an argument if you are modifying the item in the slot.
        //Note: This item slot is the slot the item is being held at when the use method is called.

        private void ConsumeItem(ItemSlot slot) 
        {
            Debug.Log("You have consumed " + slot.slotItem.itemName);
            slot.Remove(1);
        }

        public void EquipItem(ItemSlot slot) 
        {
            inventory.CloseSlotOptionsMenu();
            slot.slotItem.isEquipped = true;
            equippedItems.Add(slot.slotItem);
            GameManager.Instance.sfxParent.Find("ItemPickup").GetComponent<AudioSource>().Play();
            GameObject equippedItem = Instantiate(slot.slotItem.prefab, GameManager.Instance.holdObjectTransform, false);
            if (equippedItem.GetComponent<Equippable>() != null){
                equippedItem.transform.localPosition = equippedItem.GetComponent<Equippable>().equippedPosition;
            }
        }

        public void UnequipItem(ItemSlot slot){
            inventory.CloseSlotOptionsMenu();
            slot.slotItem.isEquipped = false;
            equippedItems.Remove(slot.slotItem);
            GameManager.Instance.sfxParent.Find("ItemPickup").GetComponent<AudioSource>().Play();
            // Find a way to find which child is the item and destroy it
            Destroy(GameManager.Instance.holdObjectTransform.GetChild(0).gameObject);
        }

        private void PlaceItem(ItemSlot slot) 
        {
            Debug.Log("Placing " + slot.slotItem.itemName);
        }

        private void DefaultItemUse(ItemSlot slot) 
        {
            Debug.Log($"Using {slot.slotItem.itemName}.");
        }


        //Returns the item from itemList at index.
        public Item GetItemByIndex(int index) 
        {
            return itemList[index];
        }

        //Returns the item from the itemList with the name.
        public Item GetItemByName(string name) 
        {
            
            foreach (Item item in itemList) {
                if (item.itemName == name) return item;
            }
            return null;
        }

        //Returns the index of the passed in item on the itemList.
        //NOTE: Returns -1 if the item does not exist in the list and the item should be added to the list.
        public int GetItemIndex(Item item) 
        {
            for (int i = 0; i < itemList.Count; i++) if (itemList[i] == item) return i;
            return -1;
        }

        // Return the ItemSlot of the item in the inventory.
        public ItemSlot GetItemSlot(Item item) 
        {
            foreach (ItemSlot slot in inventory.slots) if (slot.slotItem == item) return slot;
            return null;
        }
    } 
