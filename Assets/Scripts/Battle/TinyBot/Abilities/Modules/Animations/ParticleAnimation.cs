using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleAnimation : AbilityEffect
{
    enum ParticleLocation
    {
        BASE,
        DESTINATION,
        TERRAINPOINT
    }
    [SerializeField] bool unparentOnPlay = false;
    [SerializeField] ParticleLocation location;
    [SerializeField] ParticleSystem[] particles;
    ParticlePosition[] particlePositions;
    struct ParticlePosition
    {
        public ParticleSystem System;
        public Vector3 BasePosition;
        public Quaternion BaseRotation;
        public Transform BaseParent;
    }


    
    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        particlePositions = particles.Select(p => 
        new ParticlePosition() { System = p, BasePosition = p.transform.localPosition, BaseRotation = p.transform.localRotation, BaseParent = p.transform.parent }).ToArray();
    }

    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        
        foreach (var particle in particlePositions)
        {
            if (location == ParticleLocation.BASE)
            {
                particle.System.transform.SetParent(particle.BaseParent, false);
                particle.System.transform.SetLocalPositionAndRotation(particle.BasePosition, particle.BaseRotation);
            }
            else
            {
                particle.System.gameObject.transform.position = trajectory[^1];
                particle.System.transform.rotation = location == ParticleLocation.TERRAINPOINT ? GetTerrainParticleFacing(owner, trajectory) : Quaternion.identity;
            }
            if (unparentOnPlay) particle.System.transform.SetParent(null, true);
            particle.System.Play();
            
        }
        yield break;
    }

    private static Quaternion GetTerrainParticleFacing(TinyBot owner, List<Vector3> trajectory)
    {
        //Pathfinder3D.GetLandingPointBy(trajectory[^1], owner.MoveStyle, out Vector3Int cleanPosition);
        Vector3 up = Pathfinder3D.GetCrawlOrientation(trajectory[^1]);
        if (up == default) up = Vector3.up;
        return Quaternion.LookRotation(owner.transform.forward, up);
    }
}
