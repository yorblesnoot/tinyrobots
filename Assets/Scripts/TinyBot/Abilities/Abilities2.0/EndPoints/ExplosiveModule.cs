using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveModule : MonoBehaviour
{
    [SerializeField] GameObject baseIndicator;
    [SerializeField] GameObject explodeVfx;
    [SerializeField] float damageDelay = .3f;

    int terrainMask;
    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain", "Shield");
        baseIndicator.SetActive(false);
    }
    
    public IEnumerator Detonate(List<Targetable> targets, Vector3 explosionPosition, int damage)
    {
        GameObject effect = Instantiate(explodeVfx, explosionPosition, Quaternion.identity);
        Destroy(effect, 2f);
        yield return new WaitForSeconds(damageDelay);
        foreach(Targetable target in targets)
        {
            Vector3 direction = target.TargetPoint.position - explosionPosition;
            float distance = direction.magnitude;
            if (Physics.Raycast(explosionPosition, direction, distance, terrainMask)) continue;
            target.ReceiveHit(damage, explosionPosition, target.TargetPoint.position);
        }
    }
}
