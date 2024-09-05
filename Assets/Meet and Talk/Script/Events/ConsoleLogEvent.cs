using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk.Event
{
    [CreateAssetMenu(menuName = "Dialogue/Event/Console Log")]
    public class ConsoleLogEvent : DialogueEventSO
    {
        #region Variables
        public LogType logType;
        public string Content;
        #endregion

        /// <summary>.
        /// The RunEvent function is called by the Event Node
        /// It can also be called manually
        /// </summary>.
        public override void RunEvent()
        {
            // Przyk³¹d Wywo³ania Funkcji Która znajduje siê w DialogueEventManager
            DialogueEventManager.Instance.ConsoleLogEvent(Content, logType);
        }
    }

    public enum LogType
    {
        Info = 0, Warning = 1, Error = 2
    }
}
