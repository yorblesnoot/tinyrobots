using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitTab : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text coreName;
    [SerializeField] Color highlightColor;

    public void AssignTab(UnityAction clickEffect, BotCharacter core)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickEffect);
        coreName.text = core.CoreName;
    }

    

    public void Highlight(bool on = true)
    {
        button.image.color = on ? highlightColor : Color.white;
    }
}
