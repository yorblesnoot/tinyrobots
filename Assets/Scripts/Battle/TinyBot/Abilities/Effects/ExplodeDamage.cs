using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeDamage : AbilityEffect
{
    [SerializeField] GameObject explodeVfx;
    [SerializeField] float damageDelay = .3f;
    public override string Description => " Damage";

    int terrainMask;
    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain", "Shield");
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        Vector3 explosionPosition = trajectory[^1];
        GameObject effect = Instantiate(explodeVfx, explosionPosition, Quaternion.identity);
        Destroy(effect, 2f);
        yield return new WaitForSeconds(damageDelay);
        foreach (Targetable target in targets)
        {
            Vector3 direction = target.TargetPoint.position - explosionPosition;
            float distance = direction.magnitude;
            if (Physics.Raycast(explosionPosition, direction, distance, terrainMask)) continue;
            target.ReceiveHit(FinalEffectiveness, owner, target.TargetPoint.position);
        }
    }
}
