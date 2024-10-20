using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] float botColliderOffset = 1;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] BotPalette palette;
    [SerializeField] GameObject suspensionStand;

    static BotAssembler instance;
    private void Awake()
    {
        instance = this;
    }

    public static TinyBot BuildBot(TreeNode<ModdedPart> treeRoot, Allegiance allegiance)
    {
        PrimaryMovement locomotion = null;
        AttachmentPoint initialAttachmentPoint;
        List<PartModifier> spawnedParts = new();
        UnitStats botStats = new();
        GameObject bot = RecursiveConstruction(treeRoot);
        initialAttachmentPoint = bot.GetComponentInChildren<AttachmentPoint>();
        TinyBot botUnit = bot.GetComponent<TinyBot>();
        if (SceneGlobals.PlayerData.DevMode && allegiance == Allegiance.PLAYER) botStats.TestMode();
        botStats.MaxAll();
        botUnit.Stats = botStats;
        botUnit.Allegiance = allegiance;
        if (locomotion == null) locomotion = AddImmobileLocomotion(botUnit);
        SetBotTallness(locomotion, initialAttachmentPoint, botUnit);
        RestructureHierarchy(locomotion, initialAttachmentPoint, bot);
        
        List<Ability> abilities = GetAbilityList(spawnedParts, botUnit);
        botUnit.Initialize(abilities, spawnedParts, locomotion);
        instance.portraitGenerator.AttachPortrait(botUnit);

        return botUnit;

        GameObject RecursiveConstruction(TreeNode<ModdedPart> currentNode, AttachmentPoint attachmentPoint = null)
        {
            GameObject spawned = currentNode.Value.Sample;
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            AddPartStats(currentNode.Value);
            if (modifier.mainRenderers != null) 
                foreach (Renderer renderer in modifier.mainRenderers) 
                    instance.palette.RecolorPart(renderer, allegiance);
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

        static void RestructureHierarchy(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, GameObject bot)
        {
            locomotion.transform.SetParent(bot.transform, true);
            initialAttachmentPoint.transform.SetParent(locomotion.sourceBone, true);
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

    static void SetBotTallness(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, TinyBot bot)
    {
        Vector3 locomotionTarget = bot.transform.position;
        locomotionTarget.y += locomotion.locomotionHeight;
        Vector3 locomotionOffset = locomotion.transform.position - initialAttachmentPoint.transform.position;
        Vector3 chassisPosition = locomotionTarget - locomotionOffset;
        chassisPosition = bot.transform.InverseTransformPoint(chassisPosition);

        initialAttachmentPoint.transform.localPosition = chassisPosition;
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
