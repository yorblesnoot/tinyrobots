using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentPhase : MonoBehaviour
{
    [SerializeField] CanvasGroup deploymentBanner;
    [SerializeField] float fadeDuration = .5f;

    static DeploymentPhase instance;
    static Queue<TinyBot> deploymentQueue;
    static TinyBot activeDeployment;

    private void Awake()
    {
        instance = this;
        deploymentBanner.alpha = 0;
        gameObject.SetActive(false);
        deploymentQueue = new();
    }
    public static IEnumerator BeginDeployment(List<TinyBot> playerBots, Action endCallback)
    {
        PrimaryCursor.TogglePlayerLockout(false);
        instance.gameObject.SetActive(true);
        DeploymentZone.BeginDeployment();
        deploymentQueue = new();
        foreach (var bot in playerBots) deploymentQueue.Enqueue(bot);
        GetNextDeployment();
        yield return Tween.Alpha(instance.deploymentBanner, 1f, instance.fadeDuration).ToYieldInstruction();
        yield return new WaitUntil(() => activeDeployment == null);
        yield return Tween.Alpha(instance.deploymentBanner, 0f, instance.fadeDuration).ToYieldInstruction();
        DeploymentZone.EndDeployment();
        endCallback();
        instance.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (activeDeployment == null) return;

        activeDeployment.BotEcho.gameObject.SetActive(true);
        Vector3 clampedPosition = DeploymentZone.ClampInZone(PrimaryCursor.Transform.position);
        if (Pathfinder3D.GetLandingPointBy(clampedPosition, activeDeployment.BotEcho.MoveStyle, out Vector3Int landingPoint))
        {
            Vector3 facing = SpawnZone.GetCenterColumn(landingPoint) - landingPoint;
            activeDeployment.BotEcho.PlaceAt(landingPoint, facing);
            if (!Input.GetMouseButtonDown(0)) return;

            activeDeployment.BotEcho.gameObject.SetActive(false);
            activeDeployment.PlaceAt(landingPoint, facing);
            GetNextDeployment();
        }
        else
        {
            activeDeployment.BotEcho.gameObject.SetActive(false);
        }
    }

    static void GetNextDeployment()
    {
        if (deploymentQueue.Count == 0)
        {
            activeDeployment = null;
            return;
        }
        activeDeployment = deploymentQueue.Dequeue();
    }    
}
