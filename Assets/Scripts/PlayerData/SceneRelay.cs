using UnityEngine;

[CreateAssetMenu(fileName = "SceneRelay", menuName = "ScriptableObjects/Singletons/SceneRelay")]
public class SceneRelay : ScriptableObject
{
    public GameObject BattleMap;
    public bool BattleComplete;
    public bool GenerateNavMap;
    public MissionType MissionType;
    public EncounterGenerator ActiveSpawnTable;

    public void PrepareBattle(GameObject map)
    {
        BattleMap = map;
    }
}
