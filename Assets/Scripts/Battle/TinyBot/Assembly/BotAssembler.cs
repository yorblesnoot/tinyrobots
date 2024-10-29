using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] float botColliderOffset = 1;
    [SerializeField] BotPalette palette;
    [SerializeField] GameObject suspensionStand;
    [SerializeField] GameObject botBase;

    static BotAssembler instance;
    private void Awake()
    {
        instance = this;
    }

    public static TinyBot BuildBot(TreeNode<ModdedPart> treeRoot, Allegiance allegiance)
    {
        GameObject spawnedBase = Instantiate(instance.botBase);
        TinyBot botUnit = spawnedBase.GetComponent<TinyBot>();
        PrimaryMovement locomotion = null;
        List<PartModifier> spawnedParts = new();
        UnitStats botStats = new();
        GameObject core = RecursiveConstruction(treeRoot);
        core.transform.SetParent(botUnit.TargetPoint, false);
        
        if (SceneGlobals.PlayerData.DevMode && allegiance == Allegiance.PLAYER) botStats.TestMode();
        botStats.MaxAll();
        botUnit.Stats = botStats;
        botUnit.Allegiance = allegiance;
        if (locomotion == null) locomotion = AddImmobileLocomotion(botUnit);
        SetBotTallness(locomotion, botUnit.TargetPoint, botUnit);
        RestructureHierarchy(locomotion, botUnit.TargetPoint, botUnit.transform);
        
        List<Ability> abilities = GetAbilityList(spawnedParts, botUnit);
        botUnit.Initialize(abilities, spawnedParts, locomotion);

        return botUnit;

        GameObject RecursiveConstruction(TreeNode<ModdedPart> currentNode, AttachmentPoint attachmentPoint = null)
        {
            GameObject spawned = currentNode.Value.Sample;
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            AddPartStats(currentNode.Value);
            instance.palette.RecolorPart(modifier, allegiance);
            spawnedParts.Add(modifier);
            if(attachmentPoint != null) spawned.transform.SetParent(attachmentPoint.transform, false);
            spawned.transform.localRotation = Quaternion.identity;
            if (currentNode.Value.BasePart.PrimaryLocomotion) locomotion = spawned.GetComponent<PrimaryMovement>();

            List<TreeNode<ModdedPart>> children = currentNode.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            for (int i = 0; i < children.Count; i++)
            {
                modifier.SubTrees = new();
                if(attachmentPoints[i].ContainsSubTree) modifier.SubTrees.Add(children[i]);
                else RecursiveConstruction(children[i], attachmentPoints[i]);
            }
            return spawned;
        }

        static void RestructureHierarchy(PrimaryMovement locomotion, Transform initialAttachmentPoint, Transform bot)
        {
            locomotion.transform.SetParent(bot, true);
            initialAttachmentPoint.SetParent(locomotion.sourceBone, true);
        }

        void AddPartStats(ModdedPart part)
        {
            foreach (var stat in part.FinalStats)
            {
                botStats.Max[stat.Key] += stat.Value;
            }
        }
    }

    private static PrimaryMovement AddImmobileLocomotion(TinyBot bot)
    {
        bot.Stats.Max[StatType.MOVEMENT] = 0;
        bot.Stats.Current[StatType.MOVEMENT] = 0;
        ImmobileMovement movement = Instantiate(instance.suspensionStand).GetComponent<ImmobileMovement>();
        movement.AttachToChassis(bot);
        return movement;
    }

    static void SetBotTallness(PrimaryMovement locomotion, Transform initialAttachmentPoint, TinyBot bot)
    {
        Vector3 locomotionTarget = bot.transform.position;
        locomotionTarget.y += locomotion.locomotionHeight;
        Vector3 locomotionOffset = locomotion.transform.position - initialAttachmentPoint.position;
        Vector3 chassisPosition = locomotionTarget - locomotionOffset;
        chassisPosition = bot.transform.InverseTransformPoint(chassisPosition);

        initialAttachmentPoint.localPosition = chassisPosition;
        Vector3 colliderCenter = chassisPosition;
        colliderCenter.y -= instance.botColliderOffset;
        bot.GetComponent<CapsuleCollider>().center = colliderCenter;
    }

    static List<Ability> GetAbilityList(List<PartModifier> spawnedParts, TinyBot botUnit)
    {
        List<Ability> abilities = new();
        foreach (var part in spawnedParts)
        {
            Ability[] partAbilities = part.Abilities;
            if(partAbilities == null) continue;
            foreach (var ability in partAbilities)
            {
                if (ability is ISubTreeConsumer consumer) consumer.SubTrees = part.SubTrees;
                ability.Initialize(botUnit);
            }
            abilities.AddRange(partAbilities);
        }

        return abilities;
    }
}
