using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] float botColliderOffset = 1;
    [SerializeField] GameObject suspensionStand;
    [SerializeField] GameObject botBase;
    [SerializeField] List<GameObject> universalAbilityContainers;

    static BotAssembler instance;
    static int[] nameCounts;
    private void Awake()
    {
        instance = this;
        nameCounts = new int[Enum.GetNames(typeof(Allegiance)).Length];
    }

    public static TinyBot BuildBot(TreeNode<ModdedPart> treeRoot, Allegiance allegiance, bool echo = false, bool grantUniversals = true)
    {
        
        GameObject spawnedBase = Instantiate(instance.botBase);
        NameBot(spawnedBase, allegiance);
        spawnedBase.transform.SetParent(instance.transform, false);
        TinyBot botUnit = spawnedBase.GetComponent<TinyBot>();
        
        PrimaryMovement locomotion = null;
        List<PartModifier> spawnedParts = new();
        UnitStats botStats = new();
        GameObject core = RecursiveConstruction(treeRoot, spawnedParts, botStats, ref locomotion);
        core.transform.SetParent(botUnit.TargetPoint, false);

        botStats.MaxAll();
        if (SceneGlobals.PlayerData.DevMode && allegiance == Allegiance.PLAYER) botStats.TestMode();
        
        if (locomotion == null)
        {
            locomotion = AddImmobileLocomotion(botUnit, botStats, out PartModifier stand);
            spawnedParts.Add(stand);
        }
        SetBotTallness(locomotion, botUnit.TargetPoint, botUnit);
        RestructureHierarchy(locomotion, botUnit.TargetPoint, botUnit.transform);
        foreach(PartModifier partModifier in spawnedParts)
        {
            if (echo) SceneGlobals.BotPalette.RecolorPart(partModifier, BotPalette.Special.HOLOGRAM);
            else SceneGlobals.BotPalette.RecolorPart(partModifier, allegiance);
        }
        
        List<Ability> abilities = GetAbilityList(spawnedParts);
        if (grantUniversals) abilities.AddRange(GetUniversals(botUnit));
        botUnit.Initialize(abilities, spawnedParts, locomotion, echo, botStats, allegiance);
        if (allegiance == Allegiance.PLAYER && echo == false) botUnit.BotEcho = CreateEcho(treeRoot, allegiance, botUnit);

        return botUnit;

        

        static void RestructureHierarchy(PrimaryMovement locomotion, Transform initialAttachmentPoint, Transform bot)
        {
            locomotion.transform.SetParent(bot, true);
            initialAttachmentPoint.SetParent(locomotion.sourceBone, true);
        }
    }

    private static void NameBot(GameObject spawnedBase, Allegiance allegiance)
    {
        int allegianceIndex = (int)allegiance;
        nameCounts[allegianceIndex]++;
        spawnedBase.name = $"{allegiance} Bot {nameCounts[allegianceIndex]}";
    }

    public static GameObject RecursiveConstruction(TreeNode<ModdedPart> currentNode, List<PartModifier> spawnedParts, UnitStats statBlock, ref PrimaryMovement locomotion, AttachmentPoint attachmentPoint = null)
    {
        PartModifier modifier = currentNode.Value.Sample;
        GameObject spawned = modifier.gameObject;
        spawned.SetActive(true);

        statBlock.AddPartStats(currentNode.Value);
        spawnedParts.Add(modifier);

        modifier.AttachPart(attachmentPoint != null ? attachmentPoint.transform : null);

        if(currentNode.Value.BasePart.PrimaryLocomotion) locomotion = spawned.GetComponent<PrimaryMovement>();
        List<TreeNode<ModdedPart>> children = currentNode.Children;

        for (int i = 0; i < children.Count; i++)
        {
            if (modifier.AttachmentPoints[i].ContainsSubTree) modifier.AddSubTree(children[i]);
            else RecursiveConstruction(children[i], spawnedParts, statBlock, ref locomotion, modifier.AttachmentPoints[i]);
        }
        return spawned;

        
    }

    private static TinyBot CreateEcho(TreeNode<ModdedPart> treeRoot, Allegiance allegiance, TinyBot botUnit)
    {
        treeRoot.Traverse((part) => part.InitializePart());
        TinyBot echo = BuildBot(treeRoot, allegiance, true);
        echo.DeclareEcho();
        return echo;
    }

    public static TinyBot SummonBot(TreeNode<ModdedPart> tree, TinyBot owner, Vector3 position, Action<TinyBot> botConditioning = null, bool grantUniversals = true)
    {
        tree.Traverse((part) => part.InitializePart());
        TinyBot summon = BuildBot(tree, owner.Allegiance, grantUniversals: grantUniversals);
        summon.Summoned = true;
        Pathfinder3D.GetLandingPointBy(position, summon.MoveStyle, out Vector3Int cleanPosition);
        summon.transform.position = summon.Movement.SanitizePoint(cleanPosition);
        summon.Movement.PivotToFacePosition(owner.transform.position, true);
        botConditioning?.Invoke(summon);
        TurnManager.RegisterSummon(summon);
        summon.StartCoroutine(FinalizeSummon(summon, owner));
        return summon;
    }

    static IEnumerator FinalizeSummon(TinyBot summon, TinyBot owner)
    {
        yield return summon.Fall();
        Pathfinder3D.EvaluateNodeOccupancy(owner.transform.position);
    }

    private static PrimaryMovement AddImmobileLocomotion(TinyBot bot, UnitStats stats, out PartModifier mod)
    {
        stats.Max[StatType.MOVEMENT] = 0;
        stats.Current[StatType.MOVEMENT] = 0;
        GameObject stand = Instantiate(instance.suspensionStand);
        SuspendedMovement movement = stand.GetComponent<SuspendedMovement>();
        mod = stand.GetComponent<PartModifier>();
        movement.AttachToChassis(bot);
        return movement;
    }

    static void SetBotTallness(PrimaryMovement locomotion, Transform initialAttachmentPoint, TinyBot bot)
    {
        Vector3 locomotionTarget = bot.transform.position;
        locomotionTarget.y += locomotion.LocomotionHeight - locomotion.PathHeight;
        Vector3 locomotionOffset =  initialAttachmentPoint.position - locomotion.transform.position;
        Vector3 chassisPosition = locomotionTarget + locomotionOffset;
        chassisPosition = bot.transform.InverseTransformPoint(chassisPosition);

        initialAttachmentPoint.localPosition = chassisPosition;
        Vector3 colliderCenter = chassisPosition;
        colliderCenter.y -= instance.botColliderOffset;
        bot.GetComponent<CapsuleCollider>().center = colliderCenter;
    }

    static List<Ability> GetAbilityList(List<PartModifier> spawnedParts)
    {
        List<Ability> abilities = new();
        foreach (var part in spawnedParts)
        {
            Ability[] partAbilities = part.Abilities;
            if(partAbilities == null) continue;
            abilities.AddRange(partAbilities);
        }

        return abilities;
    }

    static List<Ability> GetUniversals(TinyBot botUnit)
    {
        List<Ability> abilities = new();
        foreach(var container in instance.universalAbilityContainers)
        {
            GameObject spawned = Instantiate(container);
            spawned.name = spawned.name.Replace("(Clone)", "");
            ActiveAbility ability = spawned.GetComponent<ActiveAbility>();
            spawned.transform.SetParent(botUnit.transform, false);
            spawned.transform.position = botUnit.TargetPoint.position;
            abilities.Add(ability);
        }
        return abilities;
    }
}
