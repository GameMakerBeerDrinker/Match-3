using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private const int N = 10;
    
    public static GameManager Instance;

    public Gem[,] Gems;

    private int[,] _gemTypes;

    public int gemCount;

    public bool canMove = true;
    
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    
    private void Start()
    {
        NewGame();
    }

    
    private void Update()
    {
        
    }
    
    
    /// <summary>
    /// 交换两颗宝石，并判断是否为有效交换，有效则执行消除，无效则换回
    /// </summary>
    /// <param name="gem1"></param>
    /// <param name="gem2"></param>
    public void Swap(Gem gem1,Gem gem2)   //Remove和Fall函数未完成，已注释掉
    {
        if (!canMove)
        {
            Debug.Log("动画阶段，无法移动");
            return;
        }

        int x1 = gem1.logicPos.x;
        int y1 = gem1.logicPos.y;
        int x2 = gem2.logicPos.x;
        int y2 = gem2.logicPos.y;
        
        if (!IsAdjacent(x1,y1,x2,y2))
        {
            Debug.Log("不能交换两个不相邻的宝石");
            return;
        }

        Gem temp = Gems[x1, y1];
        Gems[x1, y1] = Gems[x2, y2];
        Gems[x2, y2] = temp;
        
        if (!(HasMatch(x1, y1) && HasMatch(x2, y2)))   //判断是否为有效交换
        {
            Debug.Log("无效交换，执行换回");
            SwapBack(x1, y1, x2, y2); //这里需要等待交换的动画放完再换回
            return;
        }

        do
        {
            //Remove(SearchMatch());   //检测配对的宝石并消除,这里需要等待上一次消除掉落完毕
            //Fall();   //宝石掉落填补空的位置
            Debug.Log("消除一次");
        } while (HasMatch());
    }
    
    
    /// <summary>
    /// 判断两个坐标是否相邻
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    private bool IsAdjacent(int x1,int y1,int x2,int y2)
    {
        return (Mathf.Abs(x1 - x2) == 1 && y1 == y2) 
               || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
    }

    
    /// <summary>
    /// 仅交换两个相邻的宝石，用于无效交换后换回
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    private void SwapBack(int x1,int y1,int x2,int y2)
    {
        if (!IsAdjacent(x1,y1,x2,y2))
        {
            Debug.Log("不能换回两个不相邻的宝石");
            return;
        }
        
        Gem temp = Gems[x1, y1];
        Gems[x1, y1] = Gems[x2, y2];
        Gems[x2, y2] = temp;
    }

    
    /// <summary>
    /// 生成一个无配对且有有效交换的盘面，并赋给当前盘面
    /// </summary>
    private void NewGame()
    {
        Gems = new Gem[N, N];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Gems[i, j].logicPos = new Vector2Int(i, j);
            }
        }

        do
        {
            Debug.Log("生成一次盘面");
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    Gems[i, j].gemType = (Gem.GemType)Random.Range(1, 7);
                    //Debug.Log(Gems[i, j].gemType);
                }
            }
        } while (HasMatch() || HasMove() == 0);
    }


    /// <summary>
    /// 将当前盘面的宝石类型信息更新到_gemTypes[N,N]中
    /// </summary>
    private void Update_gemTypes()
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                _gemTypes[i, j] = (int)Gems[i, j].gemType;
            }
        }
    }
    
    
    /// <summary>
    /// 判断当前盘面有无配对，时间复杂度 O(N²)
    /// </summary>
    /// <returns></returns>
    private bool HasMatch()
    {
        Update_gemTypes();
        
        for (int i = 1; i < N - 1; i++)
        {
            int temp = _gemTypes[i, 1];
            int count = 1;
            for (int j = 2; j < N - 1; j++)
            {
                if (temp == _gemTypes[i, j])
                {
                    count++;
                    if (count == 3)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    temp = _gemTypes[i, j];
                }
            }
        }
        
        //横着再来一遍
        for (int j = 1; j < N - 1; j++)
        {
            int temp = _gemTypes[1, j];
            int count = 1;
            for (int i = 2; i < N - 1; i++)
            {
                if (temp == _gemTypes[i, j])
                {
                    count++;
                    if (count == 3)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    temp = _gemTypes[i, j];
                }
            }
        }

        return false;
    }

    
    /// <summary>
    /// 判断当前盘面中某颗宝石有无配对
    /// </summary>
    /// <param name="x0"></param>
    /// <param name="y0"></param>
    /// <returns></returns>
    private bool HasMatch(int x0, int y0)
    {
        Update_gemTypes();

        int x = x0;
        int y = y0;
        int count = 0;
        while (_gemTypes[x, y] == _gemTypes[x0, y0])   //往左探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            x--;
        }

        x = x0 + 1;
        while (_gemTypes[x, y] == _gemTypes[x0, y0])   //往右探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            x++;
        }

        x = x0;
        count = 0;   //左右探测完毕，从0开始上下探测
        while (_gemTypes[x, y] == _gemTypes[x0, y0])   //往下探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            y--;
        }

        y = y0 + 1;
        while (_gemTypes[x, y] == _gemTypes[x0, y0])   //往上探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            y++;
        }

        return false;
    }
    
    
    /// <summary>
    /// 判断传入的盘面有无配对，时间复杂度 O(N²)
    /// </summary>
    /// <param name="gemTypes"></param>
    /// <returns></returns>
    private bool HasMatch(int[,] gemTypes)
    {
        //三连存在性判断，时间复杂度 O(N²)
        for (int i = 1; i < N - 1; i++)
        {
            int temp = gemTypes[i, 1];
            int count = 1;
            for (int j = 2; j < N - 1; j++)
            {
                if (temp == gemTypes[i, j])
                {
                    count++;
                    if (count == 3)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    temp = gemTypes[i, j];
                }
            }
        }
        
        //横着再来一遍
        for (int j = 1; j < N - 1; j++)
        {
            int temp = gemTypes[1, j];
            int count = 1;
            for (int i = 2; i < N - 1; i++)
            {
                if (temp == gemTypes[i, j])
                {
                    count++;
                    if (count == 3)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    temp = gemTypes[i, j];
                }
            }
        }

        return false;
    }

    
    /// <summary>
    /// 判断传入的盘面中某颗宝石有无配对
    /// </summary>
    /// <param name="gemTypes"></param>
    /// <param name="x0"></param>
    /// <param name="y0"></param>
    /// <returns></returns>
    private bool HasMatch(int[,] gemTypes, int x0, int y0)
    {
        int x = x0;
        int y = y0;
        int count = 0;
        while (gemTypes[x, y] == gemTypes[x0, y0])   //往左探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            x--;
        }

        x = x0 + 1;
        while (gemTypes[x, y] == gemTypes[x0, y0])   //往右探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            x++;
        }

        x = x0;
        count = 0;   //左右探测完毕，从0开始上下探测
        while (gemTypes[x, y] == gemTypes[x0, y0])   //往下探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            y--;
        }

        y = y0 + 1;
        while (gemTypes[x, y] == gemTypes[x0, y0])   //往上探测
        {
            count++;
            if (count == 3) {
                return true;
            }
            y++;
        }

        return false;
    }
    
    
    /// <summary>
    /// 计算当前的盘面有几个有效交换，时间复杂度 O(N²)
    /// </summary>
    /// <returns></returns>
    private int HasMove()
    {
        int count = 0;
        
        Update_gemTypes();

        int[,] tempGemTypes = new int[N, N];
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                tempGemTypes[i, j] = _gemTypes[i, j];
            }
        }
        
        for (int i = 1; i < N - 1; i++)
        {
            for (int j = 1; j < N - 2; j++)   //每一个宝石与后一个宝石交换，检查是否有效，再换回
            {
                int temp = tempGemTypes[i, j];
                tempGemTypes[i, j] = tempGemTypes[i, j + 1];
                tempGemTypes[i, j + 1] = temp;
                
                if (HasMatch(tempGemTypes, i, j) || HasMatch(tempGemTypes, i, j + 1)) {
                    count++;
                }
                
                temp = tempGemTypes[i, j];
                tempGemTypes[i, j] = tempGemTypes[i, j + 1];
                tempGemTypes[i, j + 1] = temp;
            }
        }
        
        //横着再来一遍
        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 2; i++)   //每一个宝石与后一个宝石交换，检查是否有效，再换回
            {
                int temp = tempGemTypes[i, j];
                tempGemTypes[i, j] = tempGemTypes[i + 1, j];
                tempGemTypes[i + 1, j] = temp;

                if (HasMatch(tempGemTypes, i, j) || HasMatch(tempGemTypes, i + 1, j)) {
                    count++;
                }

                temp = tempGemTypes[i, j];
                tempGemTypes[i, j] = tempGemTypes[i + 1, j];
                tempGemTypes[i + 1, j] = temp;
            }
        }

        if(count==0)
            Debug.Log("死局");
        
        return count;
    }

    /*
    private Gem[] SearchMatch()
    {
        Update_gemTypes();
    }

    
    /// <summary>
    /// 另外，需要完成计数
    /// </summary>
    /// <param name="gems"></param>
    private void Remove(Gem[] gems)
    {
        
    }

    
    private void Fall()
    {
        
    }*/
}
