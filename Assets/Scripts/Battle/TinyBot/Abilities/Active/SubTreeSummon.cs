using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class SubTreeSummon : AbilityEffect, ISubTreeConsumer
{
    [SerializeField] CraftablePart summonOrigin;
    ModdedPart modOrigin;
    TinyBot Owner;

    public List<TreeNode<ModdedPart>> SubTrees { get; set; }

    public override void Initialize(Ability ability)
    {
        Owner = ability.Owner;
        modOrigin = new(summonOrigin);
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        TreeNode<ModdedPart> finalTree = new(modOrigin);
        finalTree.AddChild(SubTrees[0]);
        TinyBot summon = BotAssembler.BuildBot(finalTree, Owner.Allegiance);
        yield break;
    }
}
