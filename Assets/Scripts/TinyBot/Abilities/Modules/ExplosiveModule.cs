using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveModule : MonoBehaviour
{
    [SerializeField] GameObject baseIndicator;
    [SerializeField] float explosionRadius = 1f;
    [SerializeField] GameObject explodeVfx;
    [SerializeField] float damageDelay = .3f;

    int unitMask;
    int terrainMask;
    private void Awake()
    {
        unitMask = LayerMask.GetMask("Default");
        terrainMask = LayerMask.GetMask("Terrain", "Shield");
        baseIndicator.SetActive(false);
    }
    public List<Targetable> CheckExplosionZone(Vector3 explosionPosition, bool aiMode)
    {
        List<Targetable> hitUnits = new();
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius, unitMask);
        if(colliders != null && colliders.Length > 0)
        {
            foreach(Collider collider in colliders)
            {
                if (!collider.TryGetComponent(out Targetable bot)) continue;
                Vector3 direction = bot.TargetPoint.position - explosionPosition;
                float distance = direction.magnitude;
                if (Physics.Raycast(explosionPosition, direction, distance, terrainMask)) continue;
                hitUnits.Add(bot);
            }
        }
        if (!aiMode)
        {
            baseIndicator.SetActive(true);
            baseIndicator.transform.position = explosionPosition;
            baseIndicator.transform.localScale = 2 * explosionRadius * Vector3.one;
        }
        return hitUnits;
    }
    
    public IEnumerator Detonate(Vector3 explosionPosition, int damage)
    {
        GameObject effect = Instantiate(explodeVfx, explosionPosition, Quaternion.identity);
        Destroy(effect, 2f);
        yield return new WaitForSeconds(damageDelay);

        List<Targetable> hit = CheckExplosionZone(explosionPosition, true);
        foreach(Targetable target in hit)
        {
            target.ReceiveHit(damage, explosionPosition, target.TargetPoint.position);
        }
    }


    public void HideIndicator()
    {
        baseIndicator.SetActive(false);
    }
}
