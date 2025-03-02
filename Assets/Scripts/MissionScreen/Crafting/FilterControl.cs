using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FilterControl : MonoBehaviour
{
    [SerializeField] List<FilterButton> filters;
    [SerializeField] SmartFilter smartFilter;
    [SerializeField] Color selectedFilterColor = Color.blue;

    List<FilterButton> allFilters;
    FilterButton activeFilter;
    Color unselectedFilterColor;

    [HideInInspector] public UnityEvent FiltersChanged = new();
    void Initialize()
    {
        unselectedFilterColor = smartFilter.Button.image.color;
        allFilters = new(filters);
        if(smartFilter != null) allFilters.Add(smartFilter);

        foreach (var filter in allFilters) filter.Button.onClick.AddListener(() => ApplyInventoryFilter(filter));
        ApplyInventoryFilter(filters[0]);
    }

    void ApplyInventoryFilter(FilterButton incomingFilter)
    {
        activeFilter = incomingFilter;
        
        foreach (var filter in allFilters) filter.Button.image.color = filter == activeFilter ? selectedFilterColor : unselectedFilterColor;
        FiltersChanged.Invoke();
    }

    public List<ModdedPart> GetFilteredParts(List<ModdedPart> incoming)
    {
        //uninitialized serializable filterButton isn't null for some reason... might be a bug
        if (activeFilter.Button == null) Initialize();
        List<ModdedPart> filteredParts = activeFilter.FilterParts(incoming).OrderBy(part => part.BasePart.name).ToList();
        return filteredParts;
    }
}
