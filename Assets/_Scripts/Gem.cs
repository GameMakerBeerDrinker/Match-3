using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    public class Gem: MonoBehaviour
    {
        public enum GemType
        {
            None=0,
            Red=1,
            Orange,
            Yellow,
            Green,
            Blue,
            Purple,
            White
        }

        public GemType gemType;

        public enum Special
        {
            Normal,
            Flame,
            Lightening,
            Hypercube
        }

        public Special special;

        public enum State
        {
            Idle,
            Falling,
            Chosen,
            Swapping,
            Matching,
            Removed
        }

        public State state;

        public bool inMatrix;

        public Vector2Int logicPos;
        public Vector3 worldPos;
        
        /*public Gem()
        {
            gemType = 0;
            special = 0;
            state = 0;
            inMatrix = false;
            logicPos.Set(0, 0);
            worldPos.Set(0, 0, 0);
        }*/
    }
}