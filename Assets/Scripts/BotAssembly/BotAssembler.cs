using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    [SerializeField] GameObject spawnPoint;
    [SerializeField] AbilityUI abilityUI;
    [SerializeField] TurnUI turnUI;
    [SerializeField] PortraitGenerator portraitGenerator;
    public void BuildBotFromTree(TreeNode<CraftablePart> tree)
    {
        GameObject bot = DeployOrigin(tree, out var objectTree);
        bot.transform.position = ProceduralNoiseVoxelGenerator.mapGrid.FindUnoccupiedCoordinate().ToWorldVector();
        TinyBot botUnit = bot.GetComponent<TinyBot>();

        botUnit.Initialize(objectTree);
        portraitGenerator.AttachPortrait(botUnit);
        
        turnUI.AddTurnDisplay(botUnit);

        GameObject DeployOrigin(TreeNode<CraftablePart> tree, out TreeNode<GameObject> oTree)
        {
            GameObject spawned = Instantiate(tree.Value.attachableObject);
            oTree = new(spawned);
            List<TreeNode<CraftablePart>> children = tree.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            for (int i = 0; i < children.Count; i++)
            {
                RecursiveConstruction(children[i], oTree, attachmentPoints[i]);
            }

            return spawned;
        }

        void RecursiveConstruction(TreeNode<CraftablePart> tree, TreeNode<GameObject> oTree, AttachmentPoint attachmentPoint)
        {
            GameObject spawned = Instantiate(tree.Value.attachableObject);
            TreeNode<GameObject> nextTree = oTree.AddChild(spawned);

            spawned.transform.SetParent(attachmentPoint.transform, false);
            spawned.transform.localRotation = Quaternion.identity;

            List<TreeNode<CraftablePart>> children = tree.Children;
            AttachmentPoint[] attachmentPoints = spawned.GetComponentsInChildren<AttachmentPoint>();

            if (children.Count == 0) return;

            for (int i = 0; i < children.Count; i++)
            {
                RecursiveConstruction(children[i], nextTree, attachmentPoints[i]);
            }

            return;
        }
    }
}
