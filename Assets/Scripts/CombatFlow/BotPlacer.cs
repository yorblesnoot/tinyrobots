using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;

    [SerializeField] List<BotRecord> playerBots;
    [SerializeField] List<BotRecord> enemyBots;

    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;
    [SerializeField] TurnManager turnManager;

    //Dictionary<Allegiance, Color>
    [SerializeField] SerializableDictionary<Allegiance, Color> colorMaps;

    public void PlaceBots()
    {
        List<TinyBot> bots = new();
        botConverter.Initialize();
        SpawnBotList(playerBots, Allegiance.PLAYER);
        SpawnBotList(enemyBots, Allegiance.ENEMY);

        Dictionary<MoveStyle, List<Vector3>> styleNodes = Pathfinder3D.GetStyleNodes();
        foreach(var bot in bots)
        {
            MoveStyle style = bot.PrimaryMovement.Style;
            bot.transform.position = styleNodes[style].GrabRandomly();
            bot.PrimaryMovement.SpawnOrientation();
        }

        void SpawnBotList(List<BotRecord> botRecords, Allegiance allegiance)
        {
            
            foreach (var botRecord in botRecords)
            {
                var tree = botConverter.StringToBot(botRecord.record);
                TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree);
                botUnit.allegiance = allegiance;
                //RecolorOutlines(botUnit, allegiance);
                bots.Add(botUnit);
                turnManager.AddTurnTaker(botUnit);
            }

        }
    }

    private void RecolorOutlines(TinyBot botUnit, Allegiance allegiance)
    {
        Renderer[] renderers = botUnit.GetComponentsInChildren<Renderer>();
        Color newColor = colorMaps[allegiance];
        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetColor(Shader.PropertyToID("_OutlineColor"), newColor);
        }
    }
}
