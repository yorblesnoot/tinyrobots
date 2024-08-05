using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubTreeConsumer 
{
    public List<TreeNode<ModdedPart>> SubTrees { get; set; }
}
