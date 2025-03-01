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

    
    IEnumerator DeployUnit(TinyBot bot)
    {
        TinyBot echo = bot.BotEcho;
        Vector3Int lastPosition = default;
        Vector3 facing = default; 
        //foreach (var part in bot.PartModifiers) SceneGlobals.BotPalette.RecolorPart(part, BotPalette.Special.HOLOGRAM);
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
            Vector3 clampedPosition = DeploymentZone.ClampInZone(PrimaryCursor.Transform.position);
            if(Pathfinder3D.GetLandingPointBy(clampedPosition, echo.MoveStyle, out Vector3Int landingPoint))
            {
                if (landingPoint == lastPosition) continue;

                lastPosition = landingPoint;
                facing = SpawnZone.GetCenterColumn(landingPoint) - landingPoint;
                echo.PlaceAt(landingPoint, facing);
            }
            else
            {
                echo.gameObject.SetActive(false);
            }
            
        }

        //foreach (var part in echo.PartModifiers) SceneGlobals.BotPalette.RecolorPart(part, echo.Allegiance);
        echo.gameObject.SetActive(false);
        bot.PlaceAt(lastPosition, facing);
        yield return null;
    }

    
}
