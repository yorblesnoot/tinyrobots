using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] float botColliderOffset = 1;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] BotPalette palette;
    public TinyBot BuildBotFromPartTree(TreeNode<ModdedPart> treeRoot, Allegiance allegiance)
    {
        PrimaryMovement locomotion = null;
        AttachmentPoint initialAttachmentPoint;
        List<GameObject> spawnedParts;
        //this function sets the above variables
        BotStats botStats = new();
        GameObject bot = DeployOrigin(treeRoot);
        TinyBot botUnit = bot.GetComponent<TinyBot>();
        botStats.MaxAll();
        botUnit.Stats = botStats;
        SetBotTallness(locomotion, initialAttachmentPoint, botUnit);
        RestructureHierarchy(locomotion, initialAttachmentPoint, bot);

        
        List<Ability> abilities = GetAbilityList(spawnedParts, botUnit);
        botUnit.Initialize(abilities, spawnedParts, locomotion);
        portraitGenerator.AttachPortrait(botUnit);

        return botUnit;

        GameObject DeployOrigin(TreeNode<ModdedPart> treeRoot)
        {
            treeRoot.Value.InitializePart();
            GameObject spawned = treeRoot.Value.Sample;
            AddPartStats(treeRoot.Value);
            spawnedParts = new() { spawned };
            List<TreeNode<ModdedPart>> children = treeRoot.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();
            initialAttachmentPoint = attachmentPoints[0];
            RecursiveConstruction(children[0], initialAttachmentPoint);

            return spawned;
        }

        void RecursiveConstruction(TreeNode<ModdedPart> currentNode, AttachmentPoint attachmentPoint)
        {
            currentNode.Value.InitializePart();
            GameObject spawned = currentNode.Value.Sample;
            AddPartStats(currentNode.Value);
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            if (modifier.mainRenderers != null)
            {
                foreach (Renderer renderer in modifier.mainRenderers)
                {
                    palette.RecolorPart(renderer, allegiance);
                }
            }
            spawnedParts.Add(spawned);

            spawned.transform.SetParent(attachmentPoint.transform, false);
            spawned.transform.localRotation = Quaternion.identity;

            if (currentNode.Value.BasePart.PrimaryLocomotion) locomotion = spawned.GetComponent<PrimaryMovement>();
            List<TreeNode<ModdedPart>> children = currentNode.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            if (children.Count == 0) return;
            for (int i = 0; i < children.Count; i++)
            {
                RecursiveConstruction(children[i], attachmentPoints[i]);
            }
        }

        static void RestructureHierarchy(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, GameObject bot)
        {
            if (locomotion == null) Debug.LogError("Failed to find primary locomotion.");
            locomotion.transform.SetParent(bot.transform, true);
            initialAttachmentPoint.transform.SetParent(locomotion.sourceBone, true);
        }

        void AddPartStats(ModdedPart part)
        {
            foreach (var stat in part.Stats)
            {
                botStats.Max[stat.Key] += stat.Value;
            }
        }
    }

    

    void SetBotTallness(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, TinyBot bot)
    {
        Vector3 chassisPosition = initialAttachmentPoint.transform.localPosition;
        chassisPosition.y = locomotion.chassisHeight;
        initialAttachmentPoint.transform.localPosition = chassisPosition;
        Vector3 colliderCenter = chassisPosition;
        colliderCenter.y -= botColliderOffset;
        bot.GetComponent<CapsuleCollider>().center = colliderCenter;
    }

    List<Ability> GetAbilityList(List<GameObject> spawnedParts, TinyBot botUnit)
    {
        List<Ability> abilities = new();
        foreach (var part in spawnedParts)
        {
            Ability[] partAbilities = part.GetComponents<Ability>();
            if(partAbilities == null || partAbilities.Length == 0) continue;
            foreach(var ability in partAbilities) ability.Initialize(botUnit);
            abilities.AddRange(partAbilities);
        }

        return abilities;
    }
}
