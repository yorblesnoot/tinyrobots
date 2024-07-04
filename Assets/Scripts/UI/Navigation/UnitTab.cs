using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitTab : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text coreName;

    public void AssignTab(UnityAction clickEffect, BotCore core)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickEffect);
        coreName.text = GetCoreName(core);
    }

    public static string GetCoreName(BotCore core)
    {
         return core.displayName == "" ? core.name.Replace("Core", "") : core.displayName;
    }
}
