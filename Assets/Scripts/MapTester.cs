using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapTester : MonoBehaviour
{
    Button reset;

    private void Awake()
    {
        reset = GetComponent<Button>();
        reset.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

}
