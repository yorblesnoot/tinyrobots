using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class PlayerNavigator : MonoBehaviour
{
    [SerializeField] float moveTime = 1f;
    public static PlayerNavigator Instance { get; private set; }
    [HideInInspector] public TowerNavigableZone occupiedZone;

    bool moving;

    private void Awake()
    {
        Instance = this;
    }

    public void TryMoveToZone(TowerNavigableZone zone)
    {
        if (moving) return;
        if(!occupiedZone.neighbors.Contains(zone)) return;
        moving = true;
        Tween.Position(transform, endValue: zone.unitPosition, duration: moveTime).OnComplete(() => FinishMove(zone));
    }

    void FinishMove(TowerNavigableZone zone)
    {
        moving = false;
        occupiedZone = zone;
    }
}
