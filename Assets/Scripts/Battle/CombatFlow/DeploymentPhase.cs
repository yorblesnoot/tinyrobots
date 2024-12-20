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

    private void Awake()
    {
        instance = this;
        deploymentBanner.alpha = 0;
    }
    public static IEnumerator BeginDeployment(List<TinyBot> playerBots, Action endCallback)
    {
        PrimaryCursor.TogglePlayerLockout(false);
        DeploymentZone.BeginDeployment();
        yield return Tween.Alpha(instance.deploymentBanner, 1f, instance.fadeDuration).ToYieldInstruction();
        foreach (TinyBot bot in playerBots)
        {
            yield return instance.DeployUnit(bot);
        }
        yield return Tween.Alpha(instance.deploymentBanner, 0f, instance.fadeDuration).ToYieldInstruction();
        DeploymentZone.EndDeployment();
        endCallback();
    }

    Vector3Int lastPosition;
    IEnumerator DeployUnit(TinyBot bot)
    {
        bot.ToggleActiveLayer(true);
        foreach (var part in bot.PartModifiers) SceneGlobals.BotPalette.RecolorPart(part, BotPalette.Special.HOLOGRAM);
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
            Vector3 clampedPosition = DeploymentZone.ClampInZone(PrimaryCursor.Transform.position);
            if(Pathfinder3D.GetLandingPointBy(clampedPosition, bot.MoveStyle, out Vector3Int landingPoint))
            {
                if (landingPoint == lastPosition) continue;

                lastPosition = landingPoint;
                bot.gameObject.SetActive(true);
                Vector3 cleanPosition = bot.PrimaryMovement.SanitizePoint(landingPoint);
                bot.transform.position = cleanPosition;
                bot.PrimaryMovement.SpawnOrientation();
            }
            else
            {
                bot.gameObject.SetActive(false);
            }
            
        }

        foreach (var part in bot.PartModifiers) SceneGlobals.BotPalette.RecolorPart(part, bot.Allegiance);
        bot.ToggleActiveLayer(false);
        yield return null;
    }
}
