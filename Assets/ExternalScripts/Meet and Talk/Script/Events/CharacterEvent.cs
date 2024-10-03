using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk.Event
{
    [CreateAssetMenu(menuName = "Dialogue/Event/Character Event")]
    public class CharacterEvent : DialogueEventSO
    {
        #region Variables
        public DialogueCharacterSO Character;
        #endregion

        /// <summary>.
        /// The RunEvent function is called by the Event Node
        /// It can also be called manually
        /// </summary>.
        public override void RunEvent()
        {
            DialogueEventManager.Instance.CharacterEvent(Character);
        }
    }
}
