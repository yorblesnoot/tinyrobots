using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTreeSummon : AbilityEffect
{
    [SerializeField] CraftablePart summonOrigin;
    ModdedPart modOrigin;
    public override string Description => " Summon Health";

    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        modOrigin = new(summonOrigin);
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        TreeNode<ModdedPart> finalTree = new(modOrigin);
        finalTree.AddChild(Ability.SubTrees[0]);
        TinyBot summon = BotAssembler.SummonBot(finalTree, owner, trajectory[^1], ConditionSummon, false);
        yield break;
    }

    void ConditionSummon(TinyBot bot)
    {
        foreach (var part in bot.PartModifiers)
        {
            SceneGlobals.BotPalette.RecolorPart(part, BotPalette.Special.HOLOGRAM);
        }
        bot.Stats.Max[StatType.HEALTH] = FinalEffectiveness;
        bot.Stats.Current[StatType.HEALTH] = FinalEffectiveness;
    }
}
