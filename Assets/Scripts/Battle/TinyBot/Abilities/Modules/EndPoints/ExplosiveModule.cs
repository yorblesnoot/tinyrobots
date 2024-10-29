using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveModule : MonoBehaviour
{
    [SerializeField] GameObject explodeVfx;
    [SerializeField] float damageDelay = .3f;

    int terrainMask;
    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain", "Shield");
    }
    
    public IEnumerator Detonate(TinyBot source, List<Targetable> targets, Vector3 explosionPosition, int damage)
    {
        GameObject effect = Instantiate(explodeVfx, explosionPosition, Quaternion.identity);
        Destroy(effect, 2f);
        yield return new WaitForSeconds(damageDelay);
        foreach(Targetable target in targets)
        {
            Vector3 direction = target.TargetPoint.position - explosionPosition;
            float distance = direction.magnitude;
            if (Physics.Raycast(explosionPosition, direction, distance, terrainMask)) continue;
            target.ReceiveHit(damage, source, target.TargetPoint.position);
        }
    }
}
