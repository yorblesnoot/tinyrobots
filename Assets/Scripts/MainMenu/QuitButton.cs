using UnityEngine;

public class QuitButton : MonoButton
{
    protected override void OnClick()
    {
        Application.Quit();
    }
}
