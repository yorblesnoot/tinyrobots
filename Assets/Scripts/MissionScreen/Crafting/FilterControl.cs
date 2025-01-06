using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FilterControl : MonoBehaviour
{
    [SerializeField] List<FilterButton> filters;
    [SerializeField] SmartFilter smartFilter;
    List<FilterButton> allFilters;
    FilterButton activeFilter;
    [SerializeField] Color selectedFilterColor = Color.blue;
    Color unselectedFilterColor;

    [HideInInspector] public UnityEvent FiltersChanged = new();

    void Initialize()
    {
        activeFilter = filters[0];
        unselectedFilterColor = smartFilter.Button.image.color;

        allFilters = new(filters);
        if(smartFilter != null) allFilters.Add(smartFilter);


        foreach (var filter in allFilters) filter.Button.onClick.AddListener(() => ApplyInventoryFilter(filter));
    }

    void ApplyInventoryFilter(FilterButton incomingFilter)
    {
        activeFilter = incomingFilter;
        List<ModdedPart> filteredParts = activeFilter.FilterParts(SceneGlobals.PlayerData.PartInventory).OrderBy(part => part.BasePart.name).ToList();
        foreach (var filter in allFilters) filter.Button.image.color = filter == activeFilter ? selectedFilterColor : unselectedFilterColor;
        FiltersChanged.Invoke();
    }

    public List<ModdedPart> GetFilteredParts(List<ModdedPart> incoming)
    {
        //logic
        return incoming;
    }
}
