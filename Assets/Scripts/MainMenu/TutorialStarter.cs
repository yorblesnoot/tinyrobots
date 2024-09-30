using UnityEngine;

public class TutorialStarter : MonoBehaviour
{
    
    [SerializeField] int startDifficulty = 6;

    [SerializeField] BotCore tutorialCore;
    [SerializeField] BotRecord tutorialRecord;
    [SerializeField] GameObject tutorialMap;

    [Header("Components")]
    [SerializeField] BotConverter botConverter;
    [SerializeField] SceneLoader loader;
    public void RunTutorial()
    {
        PlayerData playerData = SceneGlobals.PlayerData;
        botConverter.Initialize();
        tutorialCore.Initialize();
        tutorialCore.Bot = botConverter.StringToBot(tutorialRecord.record);
        playerData.PartInventory = new();
        playerData.CoreInventory = new() { tutorialCore };
        SceneGlobals.SceneRelay.MissionType = MissionType.TUTORIAL;
        SceneGlobals.SceneRelay.BattleMap = tutorialMap;
        SceneGlobals.SceneRelay.activeSpawnTable = null;
        loader.Load(SceneType.BATTLE);
    }
}
