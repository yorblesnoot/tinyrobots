using UnityEngine;

[CreateAssetMenu(fileName = "BotPalette", menuName = "ScriptableObjects/BotPalette")]
public class BotPalette : ScriptableObject
{
    [SerializeField] SerializableDictionary<Allegiance, Material[]> palettePicker;
    public void RecolorPart(SkinnedMeshRenderer renderer, Allegiance allegiance)
    {
        Material[] activePalette = palettePicker[allegiance];
        Material[] modPalette = renderer.materials;
        for (int i = 0; i < activePalette.Length; i++)
        {
            modPalette[i] = activePalette[i];
        }
        renderer.materials = modPalette;
    }
}
