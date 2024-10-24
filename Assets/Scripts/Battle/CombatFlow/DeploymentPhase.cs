using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentPhase : MonoBehaviour
{
    [SerializeField] CanvasGroup deploymentBanner;
    [SerializeField] float fadeDuration = .5f;
    [SerializeField] BotPalette palette;
    [SerializeField] Material hologramMaterial;
    Material[] hologramProfile;

    static DeploymentPhase instance;

    private void Awake()
    {
        instance = this;
        deploymentBanner.alpha = 0;
        hologramProfile = new Material[] { hologramMaterial};
    }
    public static IEnumerator BeginDeployment(List<TinyBot> playerBots, Action endCallback)
    {
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
        foreach (var part in bot.PartModifiers) palette.RecolorPart(part, hologramProfile);
        while (!Input.GetMouseButtonDown(0))
        {
            Vector3 clampedPosition = DeploymentZone.ClampInZone(PrimaryCursor.Transform.position);
            if(Pathfinder3D.GetLandingPointBy(clampedPosition, bot.MoveStyle, out Vector3Int landingPoint))
            {
                if (landingPoint != lastPosition)
                {
                    lastPosition = landingPoint;
                    bot.gameObject.SetActive(true);
                    Vector3 cleanPosition = bot.PrimaryMovement.SanitizePoint(landingPoint);
                    bot.transform.position = cleanPosition;
                    bot.PrimaryMovement.SpawnOrientation();
                }
            }
            else
            {
                bot.gameObject.SetActive(false);
            }
            yield return null;
        }

        foreach (var part in bot.PartModifiers) palette.RecolorPart(part, bot.Allegiance);
        bot.ToggleActiveLayer(false);
        yield return null;
    }
}
