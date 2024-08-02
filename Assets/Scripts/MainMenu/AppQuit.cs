using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppQuit : MonoButton
{
    protected override void OnClick()
    {
        Application.Quit();
    }
}
