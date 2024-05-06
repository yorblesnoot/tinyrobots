using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotConverter", menuName = "ScriptableObjects/Singletons/BotConverter")]
public class BotConverter : ScriptableObject
{
    [SerializeField] List<CraftablePart> parts;
    Dictionary<string, CraftablePart> partMap;

    public void Initialize()
    {
        BuildPartMap();
    }
    public static string BotToString(TreeNode<CraftablePart> botTreeOriginNode)
    {
        string output = "";
        ConvertNode(botTreeOriginNode);

        void ConvertNode(TreeNode<CraftablePart> node)
        {
            output += node.Value.Id;
            if (node.Children.Count == 0) return;
            output += "{";
            foreach (var child in node.Children)
            {
                ConvertNode(child);
            }
            output += "}";
        }

        return output;
    }

    readonly int guidSkip = 36;
    public TreeNode<CraftablePart> StringToBot(string input)
    {
        if (partMap == null) Initialize();
        string guid = input[..guidSkip];

        TreeNode<CraftablePart> output = new(partMap[guid]);
        TreeNode<CraftablePart> last = output;

        Stack<TreeNode<CraftablePart>> depthRecord = new();
        
        int textCursor = guidSkip;
        while (textCursor < input.Length)
        {
            if (input[textCursor] == '{')
            {
                textCursor++;
                depthRecord.Push(last);
            }
            else if(input[textCursor] ==  '}')
            {
                depthRecord.Pop();
                textCursor++;
            }
            else
            {
                string sub = input.Substring(textCursor, guidSkip);
                last = depthRecord.Peek().AddChild(partMap[sub]);
                textCursor += guidSkip;
            }
        }
        return output;
    }

    void BuildPartMap()
    {
        partMap = new();
        foreach (var part in parts)
        {
            partMap.Add(part.Id, part);
        }
    }



}
