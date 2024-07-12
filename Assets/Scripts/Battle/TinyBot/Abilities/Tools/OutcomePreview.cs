using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OutcomePreview : MonoBehaviour
{
    [SerializeField] List<TMP_Text> availableIndicators;
    List<TMP_Text> unavailableIndicators = new();
    public static OutcomePreview Main;

    private void Awake()
    {
        Main = this;
    }

    public static void DisplayPreviewNumber(int number, Vector3 location)
    {
        TMP_Text indicator = Main.availableIndicators[0];
        Main.availableIndicators.Remove(indicator);
        indicator.gameObject.SetActive(true);
        indicator.text = number.ToString();

    }

    private void Update()
    {
        foreach (var indicator in unavailableIndicators)
        {
            indicator.transform.LookAt(Camera.main.transform.position);
        }
    }

}
