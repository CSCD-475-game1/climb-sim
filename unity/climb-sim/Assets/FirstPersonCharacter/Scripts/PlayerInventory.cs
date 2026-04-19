using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int totalSlotCount = 16;
    [SerializeField] private int hotbarSlotCount = 4;

    [Header("Use Item")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private float deetRange = 8f;
    [SerializeField] private ChatUIManager chatUI;
    [SerializeField] private InventoryUIManager inventoryUI;

    [SerializeField] private ParticleSystem deetSprayFX;
    
    private readonly List<InventorySlotData> slotData = new();
    private int selectedHotbarIndex = 0;

    private void Awake()
    {
        if (playerRoot == null)
            playerRoot = transform;
        while (slotData.Count < totalSlotCount)
            slotData.Add(new InventorySlotData());
    }

    public void SelectHotbar(int index)
    {
        if (index < 0 || index >= hotbarSlotCount) return;

        selectedHotbarIndex = index;

        if (inventoryUI != null)
            inventoryUI.RefreshFromInventory();
    }

    public int GetSelectedHotbarIndex()
    {
        return selectedHotbarIndex;
    }

    public int GetTotalSlotCount()
    {
        return slotData.Count;
    }

    public int GetHotbarSlotCount()
    {
        return hotbarSlotCount;
    }

    public InventorySlotData GetSlotData(int index)
    {
        if (index < 0 || index >= slotData.Count) return null;
        return slotData[index];
    }

    public bool AddItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount < 1) return false;

        for (int i = 0; i < slotData.Count; i++)
        {
            if (slotData[i].item == itemDef)
            {
                slotData[i].amount += amount;
                if (inventoryUI != null) inventoryUI.RefreshFromInventory();
                return true;
            }
        }

        for (int i = 0; i < slotData.Count; i++)
        {
            if (slotData[i].item == null)
            {
                slotData[i].item = itemDef;
                slotData[i].amount = amount;
                if (itemDef.displayName.ToLower().Contains("deet"))
                    slotData[i].usesRemaining = 20;

                if (inventoryUI != null) inventoryUI.RefreshFromInventory();
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount < 1) return false;

        for (int i = 0; i < slotData.Count; i++)
        {
            if (slotData[i].item == itemDef)
            {
                if (slotData[i].amount < amount) return false;

                slotData[i].amount -= amount;
                if (slotData[i].amount <= 0)
                {
                    slotData[i].item = null;
                    slotData[i].amount = 0;
                }

                if (inventoryUI != null) inventoryUI.RefreshFromInventory();
                return true;
            }
        }

        return false;
    }

    public void ClearAll()
    {
        for (int i = 0; i < slotData.Count; i++)
        {
            slotData[i].item = null;
            slotData[i].amount = 0;
        }

        if (inventoryUI != null)
            inventoryUI.RefreshFromInventory();
    }

    public ItemDefinition GetSelectedItem()
    {
        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= slotData.Count)
            return null;

        return slotData[selectedHotbarIndex].item;
    }

    public void UseSelectedItem()
    {
        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= slotData.Count)
            return;

        InventorySlotData slot = slotData[selectedHotbarIndex];
        if (slot == null || slot.item == null) return;

        string itemName = slot.item.displayName.ToLowerInvariant();

        if (itemName.Contains("deet"))
        {
            List<SwarmController> swarms = FindSwarmsInRange();

            if (swarms.Count == 0)
            {
                if (chatUI != null) chatUI.ShowSystemMessage("No mosquito swarms in range.");
                return;
            }

            foreach (SwarmController swarm in swarms)
                swarm.Repel();
            deetSprayFX.Play();

            slot.usesRemaining--;

            if (slot.usesRemaining <= 0)
            {
                RemoveItem(slot.item, 1);
            }


            if (chatUI != null)
                chatUI.ShowSystemMessage($"Used DEET. Repelled {swarms.Count} mosquito swarm(s).");
            return;
        }
        if (chatUI != null)
            chatUI.ShowSystemMessage($"Can't use {slot.item.displayName} right now.");
    }
    private List<SwarmController> FindSwarmsInRange()
    {
        SwarmController[] swarms = FindObjectsOfType<SwarmController>(true);
        List<SwarmController> found = new List<SwarmController>();

        if (playerRoot == null || swarms.Length == 0)
            return found;

        foreach (SwarmController swarm in swarms)
        {
            if (!swarm.gameObject.activeInHierarchy)
                continue;

            float d = Vector3.Distance(playerRoot.position, swarm.transform.position);
            if (d <= deetRange)
                found.Add(swarm);
        }

        return found;
    }

    private SwarmController FindNearestSwarm()
    {
        SwarmController[] swarms = FindObjectsOfType<SwarmController>(true);

        Debug.Log($"Found {swarms.Length} swarms total.");

        if (playerRoot == null || swarms.Length == 0) return null;

        SwarmController best = null;
        float bestDist = deetRange;

        foreach (SwarmController swarm in swarms)
        {
            if (!swarm.gameObject.activeInHierarchy) continue;

            float d = Vector3.Distance(playerRoot.position, swarm.transform.position);
            Debug.Log($"Swarm: {swarm.name}, Scene: {swarm.gameObject.scene.name}, Dist: {d}");

            if (d <= bestDist)
            {
                bestDist = d;
                best = swarm;
            }
        }

        if (best != null)
            Debug.Log($"Best swarm: {best.name} in scene {best.gameObject.scene.name}");
        else
            Debug.Log("No swarm found in range.");

        return best;
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public ItemDefinition item = null;
        public int amount = 0;
        public int usesRemaining = 0;
    }
}
