using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] BotPalette palette;
    public TinyBot BuildBotFromPartTree(TreeNode<CraftablePart> treeRoot, Allegiance allegiance)
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

        GameObject DeployOrigin(TreeNode<CraftablePart> treeRoot)
        {
            GameObject spawned = Instantiate(treeRoot.Value.attachableObject);
            AddPartStats(treeRoot.Value);
            spawnedParts = new() { spawned };
            List<TreeNode<CraftablePart>> children = treeRoot.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();
            initialAttachmentPoint = attachmentPoints[0];
            RecursiveConstruction(children[0], initialAttachmentPoint);

            return spawned;
        }

        void RecursiveConstruction(TreeNode<CraftablePart> currentNode, AttachmentPoint attachmentPoint)
        {
            GameObject spawned = Instantiate(currentNode.Value.attachableObject);
            AddPartStats(currentNode.Value);
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            if (modifier.mainRenderers != null)
            {
                foreach (Renderer renderer in modifier.mainRenderers)
                {
                    palette.RecolorPart(renderer, allegiance);
                }
            }

            //if we've placed the primary movement part, flag it for rearrangement
            if (currentNode.Value.primaryLocomotion) locomotion = spawned.GetComponent<PrimaryMovement>();
            spawnedParts.Add(spawned);

            spawned.transform.SetParent(attachmentPoint.transform, false);
            spawned.transform.localRotation = Quaternion.identity;

            List<TreeNode<CraftablePart>> children = currentNode.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            if (children.Count == 0) return;

            for (int i = 0; i < children.Count; i++)
            {
                RecursiveConstruction(children[i], attachmentPoints[i]);
            }

            return;
        }

        static void RestructureHierarchy(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, GameObject bot)
        {
            if (locomotion == null) Debug.LogError("Failed to find primary locomotion.");
            locomotion.transform.SetParent(bot.transform, true);
            initialAttachmentPoint.transform.SetParent(locomotion.sourceBone, true);
        }

        void AddPartStats(CraftablePart part)
        {
            part.InitializeStats();
            foreach (var stat in part.Stats)
            {
                botStats.Max[stat.Key] += stat.Value;
            }
        }
    }

    

    private static void SetBotTallness(PrimaryMovement locomotion, AttachmentPoint initialAttachmentPoint, TinyBot bot)
    {
        Vector3 chassisPosition = initialAttachmentPoint.transform.localPosition;
        chassisPosition.y = locomotion.chassisHeight;
        initialAttachmentPoint.transform.localPosition = chassisPosition;
        bot.GetComponent<CapsuleCollider>().center = chassisPosition;
    }

    private static List<Ability> GetAbilityList(List<GameObject> spawnedParts, TinyBot botUnit)
    {
        List<Ability> abilities = new();
        foreach (var part in spawnedParts)
        {
            if(!part.TryGetComponent(out Ability ability)) continue;
            ability.Initialize(botUnit);
            abilities.Add(ability);
        }

        return abilities;
    }
}
