using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] GameObject spawnPoint;
    [SerializeField] UnitControl abilityUI;
    [SerializeField] PortraitGenerator portraitGenerator;
    public TinyBot BuildBotFromPartTree(TreeNode<CraftablePart> tree)
    {
        GameObject locomotion = null;
        AttachmentPoint initialAttachmentPoint;
        List<GameObject> spawnedParts;
        GameObject bot = DeployOrigin(tree);
        List<Ability> abilities = spawnedParts.Select(part => part.GetComponent<Ability>()).ToList();
        RestructureHierarchy(locomotion, initialAttachmentPoint, bot);

        TinyBot botUnit = bot.GetComponent<TinyBot>();

        botUnit.Initialize(abilities, GetComponent<PrimaryMovement>());
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

            //if we've placed the primary movement part, flag it for rearrangement
            if (currentNode.Value.primaryLocomotion) locomotion = spawned;
            else spawnedParts.Add(spawned);

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

        static void RestructureHierarchy(GameObject locomotion, AttachmentPoint initialAttachmentPoint, GameObject bot)
        {
            if (locomotion == null) Debug.LogError("Failed to find primary locomotion.");
            locomotion.transform.SetParent(bot.transform, true);
            SourceBone bodyPoint = locomotion.GetComponent<SourceBone>();
            initialAttachmentPoint.transform.SetParent(bodyPoint.bone, true);
        }
    }
}
