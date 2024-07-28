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
    
    private void Awake()
    {
        instance = this;
    }
}
