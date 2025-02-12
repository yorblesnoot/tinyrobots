using System.Collections.Generic;
using System.Linq;

public static class BotRandomizer 
{
    static Dictionary<SlotType, List<CraftablePart>> generableParts;
    static List<CraftablePart> movementOptions;
    static BotConverter converter;
    public static TreeNode<ModdedPart> GeneratePartTree(ModdedPart core, BotConverter library)
    {
        converter = library;
        movementOptions ??= converter.PartLibrary.Where(part => part.PrimaryLocomotion == true).ToList();
        CraftablePart locomotion = movementOptions.GrabRandomly(false);
        TreeNode<ModdedPart> tree = new(core);
        RecursiveGenerate(tree);
        return tree;

        void RecursiveGenerate(TreeNode<ModdedPart> tree)
        {
            PartModifier part = tree.Value.Sample;
            if(part.AttachmentPoints == null) return;
            foreach(AttachmentPoint point in part.AttachmentPoints)
            {
                CraftablePart basePart;
                if (locomotion != null && point.SlotType == locomotion.Type)
                {
                    basePart = locomotion;
                    locomotion = null;
                }
                else basePart = GetGenerables(point.SlotType).GrabRandomly(false);
                ModdedPart modPart = new(basePart);
                TreeNode<ModdedPart> branch = tree.AddChild(modPart);
                RecursiveGenerate(branch);
            }
        }
    }

    static List<CraftablePart> GetGenerables(SlotType slotType)
    {
        generableParts ??= new();
        if(!generableParts.TryGetValue(slotType, out List<CraftablePart> generables))
        {
            generables = converter.PartLibrary.Where(part => PartSlot.PartCanSlot(part.Type, slotType)).ToList();
            generableParts.Add(slotType, generables);
        }
        return generables;
    }
}
