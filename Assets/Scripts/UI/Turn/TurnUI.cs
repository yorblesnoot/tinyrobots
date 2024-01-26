using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] Camera headshotCam;
    public void AddTurnDisplay(TinyBot bot)
    {
        turnPortraitList[0].Become(bot);
        turnPortraitList.RemoveAt(0);
    }
}
