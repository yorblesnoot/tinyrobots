using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk.Event
{
    [CreateAssetMenu(menuName = "Dialogue/Event/Scene Event")]
    public class SceneEvent : DialogueEventSO
    {
        #region Variables
        public string SceneEventID;
        #endregion

        /// <summary>.
        /// The RunEvent function is called by the Event Node
        /// It can also be called manually
        /// </summary>.
        public override void RunEvent()
        {
            // Invoke Scene Event
            DialogueEventManager.Instance.InvokeSceneEvent(SceneEventID);
        }
    }
}