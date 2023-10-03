using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public enum GemType
    {
        None,Red,Orange,Yellow,Green,Blue,Purple,White
    }

    public GemType gemType;

    public enum Special
    {
        Normal,Flame,Lightening,Hypercube
    }
    public Special special;
    
    public enum State
    {
        Idle,Falling,Chosen,Swapping,Matching,Removed
    }
    public State state;

    public bool inMatrix;

    public Vector2Int logicPos;
    public Vector3 worldPos;
}