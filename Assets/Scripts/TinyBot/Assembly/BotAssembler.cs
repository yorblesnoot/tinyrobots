using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] GameObject spawnPoint;
    [SerializeField] UnitControl abilityUI;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] BotPalette palette;
    public TinyBot BuildBotFromPartTree(TreeNode<CraftablePart> tree, Allegiance allegiance)
    {
        PrimaryMovement locomotion = null;
        AttachmentPoint initialAttachmentPoint;
        List<GameObject> spawnedParts;
        //this function sets the above variables
        GameObject bot = DeployOrigin(tree);
        TinyBot botUnit = bot.GetComponent<TinyBot>();
        SetBotTallness(locomotion, initialAttachmentPoint, botUnit);

        RestructureHierarchy(locomotion, initialAttachmentPoint, bot);

        
        List<Ability> abilities = GetAbilityList(spawnedParts, botUnit);
        botUnit.Initialize(abilities, spawnedParts, locomotion);
        portraitGenerator.AttachPortrait(botUnit);

        return botUnit;

        GameObject DeployOrigin(TreeNode<CraftablePart> tree)
        {
            GameObject spawned = Instantiate(tree.Value.attachableObject);
            spawnedParts = new() { spawned };
            List<TreeNode<CraftablePart>> children = tree.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();
            initialAttachmentPoint = attachmentPoints[0];
            RecursiveConstruction(children[0], initialAttachmentPoint);

            return spawned;
        }

        void RecursiveConstruction(TreeNode<CraftablePart> currentNode, AttachmentPoint attachmentPoint)
        {
            GameObject spawned = Instantiate(currentNode.Value.attachableObject);
            PartModifier modifier = spawned.GetComponent<PartModifier>();
            if(modifier.mainRenderer != null) palette.RecolorPart(modifier.mainRenderer, allegiance);


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
