using UnityEngine;

[CreateAssetMenu(fileName = "SceneRelay", menuName = "ScriptableObjects/Singletons/SceneRelay")]
public class SceneRelay : ScriptableObject
{
    public GameObject battleMap;
    public bool generateNavMap;
    public MissionType MissionType;
}
