using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAnimation : AbilityEffect
{
    enum ParticleLocation
    {
        BASE,
        DESTINATION,
        TERRAINPOINT
    }
    [SerializeField] ParticleLocation location;
    [SerializeField] ParticleSystem[] particles;

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        
        foreach (var particle in particles)
        {
            if (location == ParticleLocation.DESTINATION || location == ParticleLocation.TERRAINPOINT) particle.gameObject.transform.position = trajectory[^1];
            if (location == ParticleLocation.TERRAINPOINT) particle.gameObject.transform.rotation = GetTerrainParticleFacing(owner, trajectory);
            particle.Play();
        }
        yield break;
    }

    private static Quaternion GetTerrainParticleFacing(TinyBot owner, List<Vector3> trajectory)
    {
        Pathfinder3D.GetLandingPointBy(trajectory[^1], owner.MoveStyle, out Vector3Int cleanPosition);
        Vector3 up = Pathfinder3D.GetCrawlOrientation(cleanPosition);
        if (up == default) up = Vector3.up;
        return Quaternion.LookRotation(owner.transform.forward, up);
    }
}
