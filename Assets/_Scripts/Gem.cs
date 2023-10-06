using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    public class Gem: MonoBehaviour
    {
        public enum GemType
        {
            Edge=-1,
            Empty=0,
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

        public SpriteRenderer spriteRenderer;
        public Color[] arrColor;

        public void RefreshGemType() {
            spriteRenderer.color = arrColor[(int)gemType + 1];
        }
        
        public float curScale;
        public float tarScale;

        private void Start() {
            curScale = 0f;
            tarScale = 0.8f;
        }

        

        public Vector2[] dir;
        private void Update() {
            curScale.ApproachRef(tarScale, 64f); 

            transform.localScale = curScale * Vector3.one;
            
            tarScale = (gemType == GemType.Empty) ? 0f : 0.8f;
        }

        private float GetMouseDistance() {
            Vector2 disA = transform.position;
            Vector2 disB = MouseCtrl.Mouse.transform.position;
            return (disA - disB).magnitude;
        }
        
    }
}