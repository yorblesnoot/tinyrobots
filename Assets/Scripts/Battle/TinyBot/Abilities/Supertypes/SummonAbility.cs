using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SummonAbility : ActiveAbility
{
    protected void SummonBot(TreeNode<ModdedPart> tree, Action<TinyBot> botConditioning = null)
    {
        tree.Traverse((part) => part.InitializePart());
        TinyBot summon = BotAssembler.BuildBot(tree, Owner.Allegiance);
        Pathfinder3D.GetLandingPointBy(CurrentTrajectory[^1], summon.MoveStyle, out Vector3Int cleanPosition);
        summon.transform.position = summon.PrimaryMovement.SanitizePoint(cleanPosition);
        summon.PrimaryMovement.SpawnOrientation();
        botConditioning?.Invoke(summon);
        TurnManager.RegisterSummon(summon);
    }
}
