using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTreeSummon : ActiveAbility, ISubTreeConsumer
{
    [SerializeField] CraftablePart summonOrigin;
    ModdedPart modOrigin;

    public List<TreeNode<ModdedPart>> SubTrees { get; set; }

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        modOrigin = new(summonOrigin);
    }
    protected override IEnumerator PerformEffects()
    {
        TreeNode<ModdedPart> finalTree = new(modOrigin);
        finalTree.AddChild(SubTrees[0]);
        TinyBot summon = BotAssembler.BuildBot(finalTree, Owner.Allegiance);
        yield break;
    }
}
