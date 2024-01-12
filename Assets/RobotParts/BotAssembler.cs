using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAssembler : MonoBehaviour
{
    public void BuildBotFromTree(SimpleTree<CraftablePart> tree)
    {
        CraftablePart chassis = tree.GetOrigin();
        Instantiate(chassis.attachableObject);

        void RecursiveConstruction()
        {

        }
    }
}
