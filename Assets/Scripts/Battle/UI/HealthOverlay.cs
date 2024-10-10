using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthOverlay : MonoBehaviour
{
    [SerializeField] Slider healthOverlay;
    [SerializeField] TMP_Text healthCount;
    [SerializeField] float changeDuration = .5f;
    public void UpdateHealth(TinyBot bot)
    {
        UpdateHealth(bot.Stats.Current[StatType.HEALTH], bot.Stats.Max[StatType.HEALTH]);
    }
    public void UpdateHealth(int current, int max)
    {
        healthOverlay.maxValue = max;
        int missingHealth = max - current;
        Tween.UISliderValue(healthOverlay, missingHealth, changeDuration);
        if (healthCount == null) return;
        healthCount.text = $"{current}/{max}";
    }
}
