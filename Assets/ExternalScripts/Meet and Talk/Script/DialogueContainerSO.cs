using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.IO;
using System;
#endif
using UnityEngine;
using UnityEngine.UIElements;

using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using MeetAndTalk.Event;

namespace MeetAndTalk
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        // Pro Feature: Load Saved Dialogue
        public bool AllowDialogueSave = false;
        public bool BlockingReopeningDialogue = false;

        public List<NodeLinkData> NodeLinkDatas = new List<NodeLinkData>();

        public List<DialogueChoiceNodeData> DialogueChoiceNodeDatas = new List<DialogueChoiceNodeData>();
        public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
        public List<TimerChoiceNodeData> TimerChoiceNodeDatas = new List<TimerChoiceNodeData>();
        public List<EndNodeData> EndNodeDatas = new List<EndNodeData>();
        public List<EventNodeData> EventNodeDatas = new List<EventNodeData>();
        public List<StartNodeData> StartNodeDatas = new List<StartNodeData>();
        public List<RandomNodeData> RandomNodeDatas = new List<RandomNodeData>();
        public List<CommandNodeData> CommandNodeDatas = new List<CommandNodeData>();
        public List<IfNodeData> IfNodeDatas = new List<IfNodeData>();

        public List<BaseNodeData> AllNodes
        {
            get
            {
                List<BaseNodeData> tmp = new List<BaseNodeData>();
                tmp.AddRange(DialogueNodeDatas);
                tmp.AddRange(DialogueChoiceNodeDatas);
                tmp.AddRange(TimerChoiceNodeDatas);
                tmp.AddRange(EndNodeDatas);
                tmp.AddRange(EventNodeDatas);
                tmp.AddRange(StartNodeDatas);
                tmp.AddRange(RandomNodeDatas);
                tmp.AddRange(CommandNodeDatas);
                tmp.AddRange(IfNodeDatas);

                return tmp;
            }
        }

#if UNITY_EDITOR
        public void GenerateCSV(string filePath, DialogueContainerSO SO)
        {
            // List to store TSV content
            List<string> tsvContent = new List<string>();

            // Define file path for saving TSV file

            /* GENERATING HEADER */
            // List to store header texts
            List<string> headerTexts = new List<string>();
            headerTexts.Add("GUID ID"); // Add GUID ID as the first header
                                        // Loop through each language enum and add it to the header
            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                headerTexts.Add(language.ToString());
            }
            // Concatenate header texts with tab separators
            string finalHeader = string.Join("\t", headerTexts);

            // Write header to file
            TextWriter tw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            tw.WriteLine(finalHeader);
            tw.Close();
            /* GENERATING HEADER */

            /* GENERATING TEXT CONTENT */
            // Loop through each type of dialogue node to extract text content
            // Dialogue Node
            for (int i = 0; i < SO.DialogueNodeDatas.Count; i++)
            {
                List<string> dialogueNodeContent = new List<string>();
                dialogueNodeContent.Add(SO.DialogueNodeDatas[i].NodeGuid); // Add Node GUID
                                                                           // Loop through each text type for the dialogue node
                for (int j = 0; j < SO.DialogueNodeDatas[i].TextType.Count; j++)
                {
                    dialogueNodeContent.Add(SO.DialogueNodeDatas[i].TextType[j].LanguageGenericType);
                }
                // Concatenate dialogue node content with tab separators
                string dialogueNodeFinal = string.Join("\t", dialogueNodeContent);
                tsvContent.Add(dialogueNodeFinal); // Add dialogue node content to TSV content list
            }

            // Choice Dialogue Node
            for (int i = 0; i < SO.DialogueChoiceNodeDatas.Count; i++)
            {
                List<string> choiceNodeContent = new List<string>();
                choiceNodeContent.Add(SO.DialogueChoiceNodeDatas[i].NodeGuid); // Add Node GUID
                                                                               // Loop through each text type for the choice dialogue node
                for (int j = 0; j < SO.DialogueChoiceNodeDatas[i].TextType.Count; j++)
                {
                    choiceNodeContent.Add(SO.DialogueChoiceNodeDatas[i].TextType[j].LanguageGenericType);
                }
                // Concatenate choice dialogue node content with tab separators
                string choiceNodeFinal = string.Join("\t", choiceNodeContent);
                tsvContent.Add(choiceNodeFinal); // Add choice dialogue node content to TSV content list

                // Loop through each dialogue node port for the choice dialogue node
                for (int j = 0; j < SO.DialogueChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                {
                    List<string> choiceNodeChoiceContent = new List<string>();
                    choiceNodeChoiceContent.Add(SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid); // Add Port GUID
                                                                                                              // Loop through each text language for the dialogue node port
                    for (int k = 0; k < SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage.Count; k++)
                    {
                        choiceNodeChoiceContent.Add(SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType);
                    }
                    // Concatenate choice dialogue node port content with tab separators
                    string choiceNodeChoiceFinal = string.Join("\t", choiceNodeChoiceContent);
                    tsvContent.Add(choiceNodeChoiceFinal); // Add choice dialogue node port content to TSV content list
                }
            }

            // Timer Choice Node
            for (int i = 0; i < SO.TimerChoiceNodeDatas.Count; i++)
            {
                List<string> choiceNodeContent = new List<string>();
                choiceNodeContent.Add(SO.TimerChoiceNodeDatas[i].NodeGuid); // Add Node GUID
                                                                            // Loop through each text type for the timer choice node
                for (int j = 0; j < SO.TimerChoiceNodeDatas[i].TextType.Count; j++)
                {
                    choiceNodeContent.Add(SO.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType);
                }
                // Concatenate timer choice node content with tab separators
                string choiceNodeFinal = string.Join("\t", choiceNodeContent);
                tsvContent.Add(choiceNodeFinal); // Add timer choice node content to TSV content list

                // Loop through each dialogue node port for the timer choice node
                for (int j = 0; j < SO.TimerChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                {
                    List<string> choiceNodeChoiceContent = new List<string>();
                    choiceNodeChoiceContent.Add(SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid); // Add Port GUID
                                                                                                           // Loop through each text language for the dialogue node port
                    for (int k = 0; k < SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage.Count; k++)
                    {
                        choiceNodeChoiceContent.Add(SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType);
                    }
                    // Concatenate timer choice node port content with tab separators
                    string choiceNodeChoiceFinal = string.Join("\t", choiceNodeChoiceContent);
                    tsvContent.Add(choiceNodeChoiceFinal); // Add timer choice node port content to TSV content list
                }
            }
            /* GENERATING TEXT CONTENT */

            // Append content to file
            tw = new StreamWriter(filePath, true, System.Text.Encoding.UTF8);
            // Write each line of TSV content to file
            foreach (string line in tsvContent)
            {
                tw.WriteLine(line);
            }
            tw.Close();

            // Log file path
            Debug.Log("TSV file generated at: " + filePath);
        }
        public void ImportText(string filePath, DialogueContainerSO SO)
        {
            // Define the file path for the text file
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                // Log an error if the file does not exist
                Debug.LogError("File does not exist at path: " + filePath);
                return;
            }

            try
            {
                // Open the file for reading
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    bool headerSkipped = false;
                    // Read each line of the file
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Skip the header line
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            continue;
                        }

                        // Split the line into fields
                        string[] fields = line.Split('\t');

                        // Get GUID
                        string nodeGuid = fields[0];

                        // Update Dialogue Node data
                        for (int i = 0; i < SO.DialogueNodeDatas.Count; i++)
                        {
                            if (nodeGuid == SO.DialogueNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.DialogueNodeDatas[i].TextType[j].LanguageGenericType = fields[j + 1];
                                }
                            }
                        }

                        // Update Choice Node data
                        for (int i = 0; i < SO.DialogueChoiceNodeDatas.Count; i++)
                        {
                            // Update text for Choice Node
                            if (nodeGuid == SO.DialogueChoiceNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.DialogueChoiceNodeDatas[i].TextType[j].LanguageGenericType = fields[j + 1];
                                }
                            }
                            // Update text for Answer Nodes
                            for (int j = 0; j < SO.DialogueChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                            {
                                if (SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid == nodeGuid)
                                {
                                    // Update text for each language
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        SO.DialogueChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }

                        // Update Timer Choice Node data
                        for (int i = 0; i < SO.TimerChoiceNodeDatas.Count; i++)
                        {
                            // Update text for Choice Node
                            if (nodeGuid == SO.TimerChoiceNodeDatas[i].NodeGuid)
                            {
                                // Update text for each language
                                for (int j = 0; j < fields.Length - 1; j++)
                                {
                                    SO.TimerChoiceNodeDatas[i].TextType[j].LanguageGenericType = fields[j + 1];
                                }
                            }
                            // Update text for Answer Nodes
                            for (int j = 0; j < SO.TimerChoiceNodeDatas[i].DialogueNodePorts.Count; j++)
                            {
                                if (SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].PortGuid == nodeGuid)
                                {
                                    // Update text for each language
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        SO.TimerChoiceNodeDatas[i].DialogueNodePorts[j].TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }
                    }
                }

                // Log success message
                Debug.Log("Text imported successfully.");
            }
            catch (Exception e)
            {
                // Log error message if an exception occurs
                Debug.LogError("Error while importing text: " + e.Message);
            }
        }
#endif
    }
    [System.Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string TargetNodeGuid;
    }

    [System.Serializable]
    public class BaseNodeData
    {
        public string NodeGuid;
        public Vector2 Position;
    }

    [System.Serializable]
    public class DialogueChoiceNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public DialogueCharacterSO Character;
        public AvatarPosition AvatarPos;
        public AvatarType AvatarType;
        public List<LanguageGeneric<string>> TextType;
        public float Duration;
    }

    [System.Serializable]
    public class TimerChoiceNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public DialogueCharacterSO Character;
        public AvatarPosition AvatarPos;
        public AvatarType AvatarType;
        public List<LanguageGeneric<string>> TextType;
        public float Duration;
        public float time;
    }

    [System.Serializable]
    public class RandomNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        public List<DialogueNodePort> DialogueNodePorts;
        public List<LanguageGeneric<AudioClip>> AudioClips;
        public DialogueCharacterSO Character;
        public AvatarPosition AvatarPos;
        public AvatarType AvatarType;
        public List<LanguageGeneric<string>> TextType;
        public float Duration;
    }

    [System.Serializable]
    public class EndNodeData : BaseNodeData
    {
        public EndNodeType EndNodeType;
        public DialogueContainerSO Dialogue;
    }

    [System.Serializable]
    public class StartNodeData : BaseNodeData
    {
        public string startID;
    }


    [System.Serializable]
    public class EventNodeData : BaseNodeData
    {
        public List<EventScriptableObjectData> EventScriptableObjects;
    }
    [System.Serializable]
    public class EventScriptableObjectData
    {
        public DialogueEventSO DialogueEventSO;
    }

    [System.Serializable]
    public class CommandNodeData : BaseNodeData
    {
        public string commmand;
    }

    [System.Serializable]
    public class IfNodeData : BaseNodeData
    {
        public string ValueName;
        public GlobalValueIFOperations Operations;
        public string OperationValue;

        public string TrueGUID;
        public string FalseGUID;
    }


    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public LocalizationEnum languageEnum;
        public T LanguageGenericType;
    }

    [System.Serializable]
    public class DialogueNodePort
    {
        public string PortGuid; //NOWE
        public string InputGuid;
        public string OutputGuid;
#if UNITY_EDITOR
        [HideInInspector] public Port MyPort;
#endif
        public TextField TextField;
        public List<LanguageGeneric<string>> TextLanguage = new List<LanguageGeneric<string>>();
    }

    [System.Serializable]
    public enum EndNodeType
    {
        End,
        Repeat,
        GoBack,
        ReturnToStart,
        StartDialogue
    }
}