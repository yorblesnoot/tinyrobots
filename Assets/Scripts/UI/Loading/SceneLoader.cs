using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    MAINMENU,
    NAVIGATION,
    BATTLE,
    CREDITS
}
public class SceneLoader : MonoBehaviour
{
    [SerializeField] Image loadingScreen;
    [SerializeField] Slider loadingBar;

    static SceneLoader instance;
    private void Awake()
    {
        instance = this;
    }

    public static void Load(SceneType sceneType)
    {
        instance.StartCoroutine(instance.LoadScene(sceneType));
    }

    private IEnumerator LoadScene(SceneType sceneType)
    {
        loadingScreen.gameObject.SetActive(true);
        loadingBar.value = 0;
        int sceneIndex = (int)sceneType;
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneIndex);
        while (!loading.isDone)
        {
            loadingBar.value = loading.progress;
            yield return null;
        }
    }
}
