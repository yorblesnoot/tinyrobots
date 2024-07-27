using UnityEngine;

[CreateAssetMenu(fileName = "SceneRelay", menuName = "ScriptableObjects/Singletons/SceneRelay")]
public class SceneRelay : ScriptableObject
{
    public GameObject BattleMap;
    public bool BattleComplete;
    public bool generateNavMap;
    public MissionType MissionType;
    public SpawnTable activeSpawnTable;

    public void PrepareBattle(GameObject map)
    {
        BattleMap = map;
    }
}
