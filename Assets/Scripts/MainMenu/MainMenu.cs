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
        newGameButton.onClick.AddListener(newGameStarter.NewGame);
        quitButton.onClick.AddListener(Application.Quit);
    }
}
