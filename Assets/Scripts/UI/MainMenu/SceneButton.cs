using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    [SerializeField] Button button;

    private void Awake()
    {
        button.onClick.AddListener(LoadTargetScene);
    }

    void LoadTargetScene()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
