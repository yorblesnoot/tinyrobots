using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class VisualizedPartInventory : MonoBehaviour
{
    [HideInInspector] public ModdedPart OriginPart;

    [Header("Components")]
    
    [SerializeField] List<ActivatablePart> partDisplays;
    [SerializeField] PartOverviewPanel partOverviewPanel;
    [SerializeField] FilterControl filterControl;
    List<ModdedPart> inventorySource;

    public ModdedPart ActivePart {  get; private set; }
    public UnityEvent<ModdedPart> PartActivated = new();
    public UnityEvent<ModdedPart> PartDoubleActivated = new();
    public int CostMultiplier = 0;

    void SetActivePart(ModdedPart part)
    {
        ActivePart = part;
        partOverviewPanel.Become(part);
        PartActivated?.Invoke(part);
    }

    public void Initialize(List<ModdedPart> source)
    {
        partOverviewPanel.gameObject.SetActive(false);
        inventorySource = source;
        if(filterControl != null) filterControl.FiltersChanged.AddListener(UpdatePartDisplays);
    }

    private void OnDestroy()
    {
        if(filterControl != null) filterControl.FiltersChanged.RemoveAllListeners();
    }

    public void RemovePart(ModdedPart part)
    {

        if (!SceneGlobals.PlayerData.DevMode) inventorySource.Remove(part);
        SetActivePart(null);

        ActivatablePart.resetActivation.Invoke();
        UpdatePartDisplays();
    }

    public void AddPart(ModdedPart part)
    {
        if (!SceneGlobals.PlayerData.DevMode) inventorySource.Add(part);
        UpdatePartDisplays();
    }

    public void UpdatePartDisplays()
    {
        List<ModdedPart> filteredParts = filterControl == null ? inventorySource : filterControl.GetFilteredParts(inventorySource);
        for (int i = 0; i < filteredParts.Count; i++)
        {
            if(i == partDisplays.Count - 1)
            {
                ActivatablePart display = Instantiate(partDisplays[^1], partDisplays[^1].transform.parent);
                partDisplays.Add(display);
            }
            partDisplays[i].gameObject.SetActive(true);
            ModdedPart part = filteredParts[i];
            int value = CostMultiplier > 0 ? PartEconomy.GetCost(part) * CostMultiplier : part.EnergyCost;
            partDisplays[i].DisplayPart(part, SetActivePart, value, PartDoubleActivated.Invoke);
            partDisplays[i].SetTextColor(part.Rarity.TextColor);
        }

        for(int i = filteredParts.Count; i < partDisplays.Count; i++) partDisplays[i].gameObject.SetActive(false);
    }


}
