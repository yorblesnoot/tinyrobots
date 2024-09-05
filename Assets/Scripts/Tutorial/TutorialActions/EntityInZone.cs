using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInZone : TutorialAction
{
    [SerializeField] GameObject zone;
    bool touched = false;
    public override IEnumerator Execute()
    {
        zone.SetActive(true);
        yield return new WaitUntil(() => touched == true);
        zone.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        touched = true;
    }
}
