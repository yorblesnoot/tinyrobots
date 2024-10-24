using UnityEngine;

[CreateAssetMenu(fileName = "BotPalette", menuName = "ScriptableObjects/BotPalette")]
public class BotPalette : ScriptableObject
{
    [SerializeField] SerializableDictionary<Allegiance, Material[]> palettePicker;
    public void RecolorPart(PartModifier part, Allegiance allegiance)
    {
        Material[] activePalette = palettePicker[allegiance];
        RecolorPart(part, activePalette);
    }

    public void RecolorPart(PartModifier part, Material[] activePalette)
    {
        if (part.mainRenderers == null) return;
        foreach (Renderer renderer in part.mainRenderers)
        {
            RecolorRenderer(renderer, activePalette);
        }
    }

    void RecolorRenderer(Renderer renderer, Material[] activePalette)
    {
        Material[] targetPalette = renderer.materials;
        for (int i = 0; i < targetPalette.Length; i++)
        {
            targetPalette[i] = activePalette[i < activePalette.Length ? i : ^1];
        }
        renderer.materials = targetPalette;
    }
}
