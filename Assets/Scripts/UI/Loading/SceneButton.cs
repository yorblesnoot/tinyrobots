using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    [SerializeField] SceneType type;
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeScene); 
    }

    private void ChangeScene()
    {
        SceneLoader.Load(type);
    }
}
