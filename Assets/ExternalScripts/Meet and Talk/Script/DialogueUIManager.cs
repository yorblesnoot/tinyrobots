using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Text.RegularExpressions;
using MeetAndTalk.GlobalValue;

namespace MeetAndTalk
{
    public class DialogueUIManager : MonoBehaviour
    {
        public static DialogueUIManager Instance;

        [Header("Type Writing")]                    // Premium Feature
        public bool EnableTypeWriting = false;      // Premium Feature
        public float typingSpeed = 50.0f;           // Premium Feature

        [Header("Dialogue UI")]
        public bool showSeparateName = false;
        public bool clearNameColor = false;         // Premium Feature
        public TextMeshProUGUI nameTextBox;
        public TextMeshProUGUI textBox;
        [Space()]
        public GameObject dialogueCanvas;
        public Slider TimerSlider;
        public GameObject SkipButton;
        public GameObject SpriteLeft;
        public GameObject SpriteRight;

        [Header("Dynamic Dialogue UI")]
        public GameObject ButtonPrefab;
        public GameObject ButtonContainer;

        [Header("Hide IF Condition")]
        public List<GameObject> HideIfLeftAvatarEmpty = new List<GameObject>();         // Premium Feature
        public List<GameObject> HideIfRightAvatarEmpty = new List<GameObject>();        // Premium Feature
        public List<GameObject> HideIfChoiceEmpty = new List<GameObject>();             // Premium Feature

        [HideInInspector] public string prefixText;
        [HideInInspector] public string fullText;
        private string currentText = "";
        private int characterIndex = 0;
        private float lastTypingTime;

        private List<Button> buttons = new List<Button>();
        private List<TextMeshProUGUI> buttonsTexts = new List<TextMeshProUGUI>();



        private void Awake()
        {
            if (Instance == null) Instance = this;

            // Premium Feature: Type-Writing
            if(EnableTypeWriting) lastTypingTime = Time.time;
        }

        private void Update()
        {
            // Premium Feature: Type-Writing
            if (characterIndex < fullText.Length && EnableTypeWriting)
            {
                if (Time.time - lastTypingTime > 1.0f / typingSpeed)
                {
                    if (fullText[characterIndex].ToString() == "<")
                    {
                        while(fullText[characterIndex].ToString() != ">")
                        {
                            currentText += fullText[characterIndex];
                            characterIndex++;
                        }
                        currentText += fullText[characterIndex];
                        characterIndex++;
                        textBox.text = currentText;
                    }
                    else { currentText += fullText[characterIndex]; characterIndex++; textBox.text = currentText; }

                    lastTypingTime = Time.time;
                }
            }
            else
            {
                textBox.text = prefixText+fullText;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void UpdateAvatars(DialogueCharacterSO left, DialogueCharacterSO right, AvatarType emotion)
        {
            foreach (GameObject obj in HideIfLeftAvatarEmpty)
            {
                if (obj != null) { obj.SetActive(left!=null); }
            }
            foreach (GameObject obj in HideIfRightAvatarEmpty)
            {
                if (obj != null) { obj.SetActive(right != null); }
            }

            if (left != null) { SpriteLeft.SetActive(true); SpriteLeft.GetComponent<Image>().sprite = left.GetAvatar(AvatarPosition.Left, emotion); }
            else { SpriteLeft.SetActive(false); }

            if (right != null) { SpriteRight.SetActive(true); SpriteRight.GetComponent<Image>().sprite = right.GetAvatar(AvatarPosition.Right, emotion); }
            else { SpriteRight.SetActive(false); }
        }



        public void ResetText(string prefix)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) prefix = RemoveRichTextTags(prefix);

            currentText = prefix;
            prefixText = prefix;
            characterIndex = 0;
        }

        public void SetSeparateName(string name)
        {
            // Premium Feature: Clean Name
            if (clearNameColor) name = RemoveRichTextTags(name);

            nameTextBox.text = name;
        }

        public void SetFullText(string text)
        {
            string newText = text;


            Regex regex = new Regex(@"\{(.*?)\}");
            MatchEvaluator matchEvaluator = new MatchEvaluator(match =>
            {
                string OldText = match.Groups[1].Value;
                return ChangeReplaceableText(OldText); 
            });

            newText = regex.Replace(newText, matchEvaluator);

            fullText = newText;
        }

        public void SetButtons(List<string> _texts, List<UnityAction> _unityActions, bool showTimer)
        {
            // Hide If Choice Empty
            foreach (GameObject obj in HideIfChoiceEmpty)
            {
                if (obj != null && _texts.Count > 0) { obj.SetActive(true); }
                else if (obj != null) { obj.SetActive(false); }
            }

            foreach (Transform child in ButtonContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < _texts.Count; i++)
            {
                GameObject btn = Instantiate(ButtonPrefab, ButtonContainer.transform);
                btn.transform.Find("Text").GetComponent<TMP_Text>().text = _texts[i];
                btn.gameObject.SetActive(true);
                btn.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                btn.GetComponent<Button>().onClick.AddListener(_unityActions[i]);
            }

            TimerSlider.gameObject.SetActive(showTimer);
        }

        string ChangeReplaceableText(string text)
        {
            GlobalValueManager manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            string TextToReplace = "[Error Value]";
            /* Global Value */
            for (int i = 0; i < manager.IntValues.Count; i++) { if (text == manager.IntValues[i].ValueName) TextToReplace = manager.IntValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.FloatValues.Count; i++) { if (text == manager.FloatValues[i].ValueName) TextToReplace = manager.FloatValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.BoolValues.Count; i++) { if (text == manager.BoolValues[i].ValueName) TextToReplace = manager.BoolValues[i].Value.ToString(); } 
            for (int i = 0; i < manager.StringValues.Count; i++) { if (text == manager.StringValues[i].ValueName) TextToReplace = manager.StringValues[i].Value; }

            //
            if(text.Contains(",")) 
            {
                string[] tmp = text.Split(',');
                for (int i = 0; i < manager.IntValues.Count; i++) { if (tmp[0] == manager.IntValues[i].ValueName) TextToReplace = Mathf.Abs(manager.IntValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
                for (int i = 0; i < manager.FloatValues.Count; i++) { if (tmp[0] == manager.FloatValues[i].ValueName) TextToReplace = Mathf.Abs(manager.FloatValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
            }

            return TextToReplace;
        }

        string RemoveRichTextTags(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }

    }
}
