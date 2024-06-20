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
    [SerializeField] NewGameStarter newGameStarter;
    
    private void Awake()
    {
        newGameButton.onClick.RemoveAllListeners();
        newGameButton.onClick.AddListener(NewGame);
        quitButton.onClick.AddListener(Quit);
    }

    void NewGame()
    {
        Debug.Log(newGameStarter);
        newGameStarter.NewGame();
    }

    void Quit()
    {
        Application.Quit();
    }
}
