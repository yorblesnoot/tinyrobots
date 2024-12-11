using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneGlobals : MonoBehaviour
{
    static SceneGlobals instance;

    [SerializeField] PlayerData playerData;
    public static PlayerData PlayerData { get { return instance.playerData; } }
    
    [SerializeField] SceneRelay sceneRelay;
    public static SceneRelay SceneRelay {  get { return instance.sceneRelay; } }

    [SerializeField] BotPalette botPalette;
    public static BotPalette BotPalette { get { return instance.botPalette; } }

    private void Awake()
    {
        instance = this;
    }
}
