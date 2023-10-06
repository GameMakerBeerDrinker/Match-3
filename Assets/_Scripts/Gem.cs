using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts
{
    public enum GemState
    {
        Idle,
        Falling,
        Chosen,
        Swapping,
        Matching,
        Removed
    }

    public enum GemType {
        Edge = -1,
        Empty = 0,
        Red = 1,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        White,
    }
    public enum Special
    {
        Normal,
        Flame,
        Lightening,
        Hypercube,
    }
    

    public class Gem: MonoBehaviour
    {
        public GemType gemType;
        public Special gemSp;
        public GemState gemState;

        public SpriteRenderer spriteRenderer;
        public TMP_Text colorText;
        public Color[] arrColor;

        public void RefreshGemType() {
            spriteRenderer.color = arrColor[(int)gemType + 1];
            colorText.text = ((int)gemType).ToString();
        }
        
        public float curScale;
        public float tarScale;

        public Vector2Int curLogicPos;
        public Vector2Int tarLogicPos;

        private void FixedUpdate() {
            curScale.ApproachRef(tarScale, 12f,0.01f);
            
            if (gemState == GemState.Swapping || gemState == GemState.Falling) {
                transform.position = transform.position.ApproachValue(GameManager.LogicPosToWorldPos(tarLogicPos),
                    12f * Vector3.one, 0.01f);

                if (transform.position.Equal(GameManager.LogicPosToWorldPos(tarLogicPos))) {
                    gemState = GemState.Idle;
                }
            }
        }

        private void Update() {
            transform.localScale = curScale * Vector3.one;
            tarScale = (gemType == GemType.Empty) ? 0f : 0.8f;
            
            
            if (gemType == GemType.Empty)
                transform.position = transform.position.SetZ(1f);
            
            if(gemType == GemType.Empty && curScale.Equal(0f,0.01f))
                Destroy(gameObject);
        }

        private float GetMouseDistance() {
            Vector2 disA = transform.position;
            Vector2 disB = MouseCtrl.Mouse.transform.position;
            return (disA - disB).magnitude;
        }
        
    }
}