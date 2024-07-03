using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthOverlay : MonoBehaviour
{
    [SerializeField] Slider healthOverlay;
    [SerializeField] TMP_Text healthCount;
    public void UpdateHealth(TinyBot bot)
    {
        healthOverlay.maxValue = bot.Stats.Max[StatType.HEALTH];
        int missingHealth = bot.Stats.Max[StatType.HEALTH] - bot.Stats.Current[StatType.HEALTH];
        healthOverlay.value = missingHealth;
        if (healthCount == null) return;

        healthCount.text = $"{bot.Stats.Current[StatType.HEALTH]}/{bot.Stats.Max[StatType.HEALTH]}";
    }
}
