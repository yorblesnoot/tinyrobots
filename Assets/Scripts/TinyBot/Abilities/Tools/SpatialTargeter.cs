using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialTargeter : MonoBehaviour
{
    [HideInInspector] public List<TinyBot> intersectingBots = new();
    private void OnDisable()
    {
        //intersectingBots = new();
    }

    public List<TinyBot> GetIntersectingBots()
    {
        List<TinyBot> output = new(intersectingBots);
        intersectingBots = new();
        return output;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out TinyBot bot))
        {
            intersectingBots.Add(bot);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out TinyBot bot))
        {
            intersectingBots.Remove(bot);
        }

    }
}
