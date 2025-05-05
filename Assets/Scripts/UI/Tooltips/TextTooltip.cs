using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTooltip : TooltipBase
{
    static TextTooltip instance;
    [Header("Components")]
    [SerializeField] TMP_Text text;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }
    public static void Show(string text, Vector3 position)
    {
        instance.gameObject.SetActive(true);
        instance.text.text = text;
        instance.SetPosition(position);
    }
    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}
