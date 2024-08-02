using UnityEngine;
using UnityEngine.UI;

public class SaveTestButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(SaveGame);
    }

    private void SaveGame()
    {
        SaveContainer saveContainer = new(SceneGlobals.PlayerData);
        saveContainer.SaveGame();
    }
}
