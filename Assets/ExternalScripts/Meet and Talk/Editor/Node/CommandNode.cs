using System;
using UnityEngine;
using UnityEngine.UIElements;
using MeetAndTalk.Editor;

namespace MeetAndTalk.Nodes
{
    public class CommandNode : BaseNode
    {
        public string command;
        private TextField textField;

        public CommandNode()
        {

        }

        public CommandNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Notes";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            textField = new TextField("");
            textField.RegisterValueChangedCallback(value =>
            {
                command = value.newValue;
            });

            textField.SetValueWithoutNotify(command);

            textField.multiline = true;
            textField.AddToClassList("TextBox");

            mainContainer.Add(textField);
        }

        public override void LoadValueInToField()
        {
            textField.SetValueWithoutNotify(command);
        }
    }
}
