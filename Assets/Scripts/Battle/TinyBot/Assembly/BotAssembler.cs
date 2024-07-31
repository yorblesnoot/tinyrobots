using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] float botColliderOffset = 1;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] BotPalette palette;

    static BotAssembler instance;
    private void Awake()
    {
        instance = this;
    }

    public static TinyBot BuildBot(TreeNode<ModdedPart> treeRoot, Allegiance allegiance)
    {
        PrimaryMovement locomotion = null;
        AttachmentPoint initialAttachmentPoint;
        List<GameObject> spawnedParts = new();
        UnitStats botStats = new();
        GameObject bot = RecursiveConstruction(treeRoot);
        initialAttachmentPoint = bot.GetComponentInChildren<AttachmentPoint>();
        TinyBot botUnit = bot.GetComponent<TinyBot>();
        botStats.MaxAll();
        botUnit.Stats = botStats;
        botUnit.Allegiance = allegiance;
        SetBotTallness(locomotion, initialAttachmentPoint, botUnit);
        RestructureHierarchy(locomotion, initialAttachmentPoint, bot);
        
        List<Ability> abilities = GetAbilityList(spawnedParts, botUnit);
        botUnit.Initialize(abilities, spawnedParts, locomotion);
        instance.portraitGenerator.AttachPortrait(botUnit);

        return botUnit;

        GameObject RecursiveConstruction(TreeNode<ModdedPart> currentNode, AttachmentPoint attachmentPoint = null)
        {
            currentNode.Value.InitializePart();
            GameObject spawned = currentNode.Value.Sample;
            AddPartStats(currentNode.Value);
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            if (modifier.mainRenderers != null)
            {
                foreach (Renderer renderer in modifier.mainRenderers)
                {
                    instance.palette.RecolorPart(renderer, allegiance);
                }
            }
            spawnedParts.Add(spawned);

            if(attachmentPoint != null) spawned.transform.SetParent(attachmentPoint.transform, false);
            spawned.transform.localRotation = Quaternion.identity;

            if (currentNode.Value.BasePart.PrimaryLocomotion) locomotion = spawned.GetComponent<PrimaryMovement>();
            List<TreeNode<ModdedPart>> children = currentNode.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            for (int i = 0; i < children.Count; i++)
            {
                RecursiveConstruction(children[i], attachmentPoints[i]);
            }
            return spawned;
        }

        static void RestructureHierarchy(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, GameObject bot)
        {
            if (locomotion == null) Debug.LogError("Failed to find primary locomotion.");
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

    static void SetBotTallness(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, TinyBot bot)
    {
        Vector3 chassisPosition = initialAttachmentPoint.transform.localPosition;
        chassisPosition.y = locomotion.chassisHeight;
        initialAttachmentPoint.transform.localPosition = chassisPosition;
        Vector3 colliderCenter = chassisPosition;
        colliderCenter.y -= instance.botColliderOffset;
        bot.GetComponent<CapsuleCollider>().center = colliderCenter;
    }

    static List<Ability> GetAbilityList(List<GameObject> spawnedParts, TinyBot botUnit)
    {
        List<Ability> abilities = new();
        foreach (var part in spawnedParts)
        {
            Ability[] partAbilities = part.GetComponent<PartModifier>().Abilities;
            if(partAbilities == null || partAbilities.Length == 0) continue;
            foreach(var ability in partAbilities) ability.Initialize(botUnit);
            abilities.AddRange(partAbilities);
        }

        return abilities;
    }
}
