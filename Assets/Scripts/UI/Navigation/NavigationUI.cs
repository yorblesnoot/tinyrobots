using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationUI : MonoBehaviour
{ 
    [SerializeField] Button _BlueprintButton;
    [SerializeField] BlueprintControl _BlueprintControl;

    private void Awake()
    {
        _BlueprintButton.onClick.AddListener(OpenBlueprint);
    }

    void OpenBlueprint()
    {
        _BlueprintControl.gameObject.SetActive(true);
    }
}
