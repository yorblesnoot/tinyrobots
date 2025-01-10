using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    enum Mode
    {
        CUBE,
        SPHERE
    }

    [SerializeField] Mode mode;
    public Allegiance Allegiance;
    [SerializeField] Transform limitMarker;
    float radius;
    static Dictionary<Allegiance, Dictionary<MoveStyle, List<Vector3>>> styleNodes;
    private void Awake()
    {
        if(styleNodes != null) styleNodes = null;
        Pathfinder3D.MapInitialized.AddListener(CalculateZoneNodes);
        gameObject.SetActive(false);
        radius = Vector3.Distance(transform.position, limitMarker.position);
    }

    void CalculateZoneNodes()
    {
        BuildStyleNodes();
        Vector3 rawSource = transform.position;
        Vector3 checkEnd = limitMarker.position;
        if(mode == Mode.SPHERE)
        {
            Vector3 sphereOffset = Vector3.one * radius;
            rawSource = transform.position - sphereOffset;
            checkEnd = transform.position + sphereOffset;
        }

        Vector3Int checkSource = Vector3Int.RoundToInt(rawSource);
        Vector3 searchDirection = checkEnd - checkSource;
        Vector3Int searchSigns = new(Math.Sign(searchDirection.x), Math.Sign(searchDirection.y), Math.Sign(searchDirection.z));
        //Debug.Log($"finding nodes for {Allegiance} from {checkSource} to {checkEnd}. signs are {searchSigns}");

        for(int x = checkSource.x; ConditionalCompare(x, checkEnd.x, searchSigns.x); x += searchSigns.x)
        {
            for(int y = checkSource.y; ConditionalCompare(y, checkEnd.y, searchSigns.y); y += searchSigns.y)
            {
                for(int z = checkSource.z; ConditionalCompare(z, checkEnd.z, searchSigns.z); z += searchSigns.z)
                {
                    Vector3Int checkedPosition = new(x, y, z);
                    if (mode == Mode.SPHERE && Vector3.Distance(checkedPosition, checkSource) > radius) continue;
                    HashSet<MoveStyle> styles = Pathfinder3D.GetNodeStyles(checkedPosition);
                    foreach (MoveStyle style in styles) 
                    {
                        //Debug.Log($"added {Allegiance} {style} {checkedPosition}");
                        styleNodes[Allegiance][style].Add(checkedPosition);
                    }
                    
                }
            }
        }
    }

    bool ConditionalCompare(float x, float y, int sign)
    {
        if (sign > 0) return x <= y;
        else return x >= y;
    }

    void BuildStyleNodes()
    {
        if (styleNodes != null) return;

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
        if (availableSpaces.Count == 0) Debug.LogError($"{bot.Allegiance} Bot with Movement {bot.MoveStyle} was unable to find space within a Spawn Zone.");
        Vector3 targetSpace =  availableSpaces.GrabRandomly();
        bot.transform.position = bot.PrimaryMovement.SanitizePoint(targetSpace);
        foreach(var allegiance in styleNodes.Values) 
            foreach (var mode in allegiance.Values) mode.Remove(targetSpace);
        bot.gameObject.SetActive(true);
        bot.PrimaryMovement.SpawnOrientation();
        bot.StartCoroutine(bot.PrimaryMovement.NeutralStance());
    }

    private void OnDrawGizmos()
    {
        
        if (mode == Mode.SPHERE)
        {
            radius = Vector3.Distance(transform.position, limitMarker.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else if (mode == Mode.CUBE) GizmoPlus.DrawWireCuboid(transform.position, limitMarker.position, Color.red);

    }
}
