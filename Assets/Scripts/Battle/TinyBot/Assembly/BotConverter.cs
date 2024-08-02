using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "BotConverter", menuName = "ScriptableObjects/Singletons/BotConverter")]
public class BotConverter : ScriptableObject
{
    public List<BotCore> CoreLibrary;
    public List<CraftablePart> PartLibrary;
    public List<PartMutator> MutatorLibrary;
    Dictionary<string, CraftablePart> partMap;
    Dictionary<string, PartMutator> mutatorMap;
    Dictionary<string, BotCore> coreMap;

    public void Initialize()
    {
        BuildConversionDictionaries();
    }
    public static string BotToString(TreeNode<ModdedPart> botTreeOriginNode)
    {
        string output = "";
        ConvertNode(botTreeOriginNode);

        void ConvertNode(TreeNode<ModdedPart> node)
        {
            output += PartToString(node.Value);
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

    public static string PartToString(ModdedPart part)
    {
        string output = part.BasePart.Id;
        foreach (var mutator in part.Mutators)
        {
            output += "!";
            output += mutator.Id;
        }
        return output;
    }

    readonly int guidSkip = 36;
    public TreeNode<ModdedPart> StringToBot(string input)
    {
        if (partMap == null) Initialize();
        string guid = input[..guidSkip];
        CraftablePart basePart = partMap[guid];
        ModdedPart moddedPart = new(basePart);
        TreeNode<ModdedPart> output = new(moddedPart);
        TreeNode<ModdedPart> last = output;

        Stack<TreeNode<ModdedPart>> depthRecord = new();
        
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
                textCursor = GetPartFromSequence(input, textCursor, out var modPart);
                last = depthRecord.Peek().AddChild(modPart);
            }
        }
        return output;
    }

    int GetPartFromSequence(string input, int textCursor, out ModdedPart modPart)
    {
        string sub = input.Substring(textCursor, guidSkip);
        modPart = new(partMap[sub]);
        textCursor += guidSkip;
        while(input[textCursor] == '!')
        {
            textCursor++;
            string modSub = input.Substring(textCursor, guidSkip);
            PartMutator mutator = mutatorMap[modSub];
            modPart.Mutators.Add(mutator);
            textCursor += guidSkip;
        }
        modPart.InitializePart();
        return textCursor;
    }

    void BuildConversionDictionaries()
    {
        partMap = PartLibrary.ToDictionary(p => p.Id, p => p);
        mutatorMap = MutatorLibrary.ToDictionary(m => m.Id, m => m);
        coreMap = CoreLibrary.ToDictionary(c => c.Id, c => c);
    }



}
