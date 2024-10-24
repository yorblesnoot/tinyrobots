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
        yield return Tween.Alpha(instance.deploymentBanner, 1f, instance.fadeDuration).ToYieldInstruction();
        foreach (TinyBot bot in playerBots)
        {
            yield return instance.DeployUnit(bot);
        }
        yield return Tween.Alpha(instance.deploymentBanner, 0f, instance.fadeDuration).ToYieldInstruction();
        endCallback();
    }

    Vector3Int lastPosition;
    IEnumerator DeployUnit(TinyBot bot)
    {
        bot.ToggleActiveLayer(true);
        while (!Input.GetMouseButtonDown(0))
        {
            if(Pathfinder3D.GetLandingPointBy(PrimaryCursor.Transform.position, bot.MoveStyle, out Vector3Int landingPoint))
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
        
        bot.StartCoroutine(bot.PrimaryMovement.NeutralStance());
        bot.ToggleActiveLayer(false);
        yield return null;
    }
}
