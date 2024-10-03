using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RarityPalette", menuName = "ScriptableObjects/RarityPalette")]
public class PartRarityPalette : ScriptableObject
{
    [SerializeField] List<Entry> paletteEntries;
    bool initialized = false;
    public Color GetModColor(int modCount)
    {
        if(!initialized)
        {
            paletteEntries = paletteEntries.OrderByDescending(e => e.ModThreshold).ToList();
            initialized = true;
        }

        foreach(Entry entry in paletteEntries)
        {
            if(modCount <= entry.ModThreshold) return entry.TextColor;
        }

        return Color.white;
    }

    [System.Serializable] 
    struct Entry
    {
        public int ModThreshold;
        public Color TextColor;
    }
}
