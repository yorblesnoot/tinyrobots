using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    MAINMENU,
    WORLD,
    BATTLE,
    CREDITS
}
public class SceneLoader : MonoBehaviour
{
    [SerializeField] Image loadingScreen;
    [SerializeField] Slider loadingBar;

    
    public void Change(SceneType sceneType)
    {
        loadingScreen.gameObject.SetActive(true);
        loadingBar.value = 0;
        StartCoroutine(LoadScene(sceneType));
    }

    private IEnumerator LoadScene(SceneType sceneType)
    {
        int sceneIndex = (int)sceneType;
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneIndex);
        while (!loading.isDone)
        {
            loadingBar.value = loading.progress;
            yield return null;
        }
    }
}
