using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public Allegiance Allegiance;
    public int Radius = 1;
    public Vector3 Position { get { return transform.position; } }
    static Dictionary<Allegiance, Dictionary<MoveStyle, List<Vector3>>> styleNodes;
    private void Awake()
    {
        Pathfinder3D.MapInitialized.AddListener(CalculateZoneNodes);
        gameObject.SetActive(false);
    }

    void CalculateZoneNodes()
    {
        BuildStyleNodes();
        Vector3Int position = Vector3Int.RoundToInt(transform.position);
        for(int x = position.x - Radius; x < position.x + Radius; x++)
        {
            for(int y = position.y - Radius; y < position.y + Radius; y++)
            {
                for(int z = position.z - Radius; z < position.z + Radius; z++)
                {
                    Vector3Int checkedPosition = new(x, y, z);
                    if (Vector3.Distance(checkedPosition, position) > Radius) continue;
                    List<MoveStyle> styles = Pathfinder3D.GetNodeStyles(checkedPosition);
                    foreach(MoveStyle style in styles) styleNodes[Allegiance][style].Add(checkedPosition);
                }
            }
        }
    }

    void BuildStyleNodes()
    {
        if (styleNodes != null) return;

        Debug.Log("built nodes");
        styleNodes = new();
        foreach (Allegiance allegiance in Enum.GetValues(typeof(Allegiance)))
        {
            styleNodes.Add(allegiance, new());
            foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) styleNodes[allegiance].Add(style, new());
        }
    }

    public static void PlaceBot(TinyBot bot)
    {
        List<Vector3> availableSpaces = styleNodes[bot.Allegiance][bot.MoveStyle];
        Debug.Log(bot.Allegiance + "-" +  bot.MoveStyle);
        bot.transform.position = availableSpaces.GrabRandomly();
        bot.PrimaryMovement.SpawnOrientation();
        bot.StartCoroutine(bot.PrimaryMovement.NeutralStance());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Position, Radius);
    }
}
