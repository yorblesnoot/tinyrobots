using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVOBuilder : MonoBehaviour
{
    void Sparseify(byte[,,] map)
    {
        //break up a given cube of voxels into 8 regions
        //go through each region and check contents
        //if they all match, go to the next region
        //if they dont all match, add subregions
    }
}
