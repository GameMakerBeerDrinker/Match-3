using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public class MouseCtrl : MonoBehaviour {
    public static MouseCtrl Mouse;

    private void Awake() {
        if (Mouse == null) {
            Mouse = this;
        }
        else {
            DestroyImmediate(this);
        }
    }

    public bool inSwap;
    public Vector2 inSwapPos;
    public Vector2 swapPointer;
    public int swapDir;
    
    public Vector2Int curLogicPos;

    
    
    void Update() {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, -5);
        
        curLogicPos = GameManager.Manager.WorldPosToLogicPos(transform.position);
    }
}
