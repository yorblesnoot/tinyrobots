using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button newGameButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;
    [SerializeField] SceneRelay relay;
    [SerializeField] SceneLoader loader;
    private void Awake()
    {
        newGameButton.onClick.AddListener(NewGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void NewGame()
    {
        relay.generateNavMap = true;
        loader.Load(SceneType.NAVIGATION);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
