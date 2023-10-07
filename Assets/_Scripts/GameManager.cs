using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        private const int ColumnAndRaw = 8;   //配置游戏的行列数（不包括边界）
        private const int TypeCount = 7;   //配置宝石的种类数
        
        private const int N = ColumnAndRaw + 2;


        public static GameManager Instance;

        public GameObject gemPrefab;

        public Gem[,] Gems;

        private int[,] _gemTypes;

        private int[,] _matchMatrix;   //0为不消，1-7为对应颜色的宝石消

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
            //TestNewGame();
            TestDisplay();
        }


        private void TestNewGame()
        {
                Gems[1, 1].gemType = Gem.GemType.Red;
                Gems[1, 2].gemType = Gem.GemType.Orange;
                Gems[1, 3].gemType = Gem.GemType.Red;
                Gems[2, 1].gemType = Gem.GemType.Red;
                Gems[2, 2].gemType = Gem.GemType.Orange;
                Gems[2, 3].gemType = Gem.GemType.Red;
                Gems[3, 1].gemType = Gem.GemType.Orange;
                Gems[3, 2].gemType = Gem.GemType.Orange;
                Gems[3, 3].gemType = Gem.GemType.Orange;
        }
        
        
        private void Update()
        {
            /*if (Input.GetMouseButtonDown(1))
            {
                //Swap(1,1,1,2);
                SearchMatch();
                Remove();
                Fall();
            }*/
        }


        /// <summary>
        /// 交换两颗宝石，并判断是否为有效交换
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void Swap(int x1,int y1,int x2,int y2) 
        {
            /*if (!canMove)
            {
                Debug.Log("动画阶段，无法移动");
                return;
            }*/

            if (!IsAdjacent(x1, y1, x2, y2))
            {
                Debug.Log("不能交换两个不相邻的宝石");
                return;
            }

            Gem temp1 = Gems[x1, y1];
            Gems[x1, y1] = Gems[x2, y2];
            Gems[x2, y2] = temp1;
            
            /*Gem temp2 = Gems[x1, y1].GetComponent<Gem>();
            Gems[x1, y1] = Gems[x2, y2].GetComponent<Gem>();   //另一种换法，区别是？
            Gems[x2, y2] = temp2.GetComponent<Gem>();*/
            
            Debug.Log("交换后的盘面为");
            TestDisplay();
            
            if (!(HasMatch(x1, y1) && HasMatch(x2, y2))) //判断是否为有效交换
            {
                Debug.Log("无效交换，请换回");
                return;
            }

            Debug.Log("有效交换");
            

            
            /*do
            {
                Remove(SearchMatch());   //检测配对的宝石并消除,这里需要等待上一次消除掉落完毕
                Fall();   //宝石掉落填补空的位置
                Debug.Log("消除一次");
            } while (HasMatch());*/
        }


        /// <summary>
        /// 判断两个坐标是否相邻
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        private bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            return (Mathf.Abs(x1 - x2) == 1 && y1 == y2)
                   || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
        }


        /// <summary>
        /// 生成一个无配对且有有效交换的盘面，并赋给当前盘面
        /// </summary>
        private void NewGame()
        {
            Gems = new Gem[N, N];
            _gemTypes = new int[N, N];
            _matchMatrix = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Gems[i, j] = Instantiate(gemPrefab).GetComponent<Gem>();
                    Gems[i, j].gemType = Gem.GemType.Edge;
                    //Gems[i, j].logicPos.Set(i, j);
                }
            }
            
            do
            {
                Refresh();
                Debug.Log("生成一次盘面");
                //TestDisplay();
            } while (HasMatch() || HasMove() == 0);
            
            //TestDisplay();
        }


        private void Refresh()
        {
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    Gems[i, j].gemType = (Gem.GemType)Random.Range(1, TypeCount+1);
                }
            }
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
        public bool HasMatch()
        {
            Update_gemTypes();

            for (int i = 1; i < N - 1; i++)
            {
                int temp = _gemTypes[i, 1];
                int count = 1;
                for (int j = 2; j < N - 1; j++)
                {
                    if (temp == _gemTypes[i, j] && temp != 0)
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
                    if (temp == _gemTypes[i, j] && temp != 0)
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
        public bool HasMatch(int x0, int y0)
        {
<<<<<<< Updated upstream
            Update_gemTypes();
=======
            if (Gems[x0, y0].gemType == GemType.Empty)
            {
                Debug.Log("传入了空位");
                return false;
            }
            
            UpdateGemTypeOnBoard();
>>>>>>> Stashed changes

            int x = x0;
            int y = y0;
            int count = 0;
            while (_gemTypes[x, y] == _gemTypes[x0, y0]) //往左探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                x--;
            }

            x = x0 + 1;
            while (_gemTypes[x, y] == _gemTypes[x0, y0]) //往右探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                x++;
            }

            x = x0;
            count = 0; //左右探测完毕，从0开始上下探测
            while (_gemTypes[x, y] == _gemTypes[x0, y0]) //往下探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                y--;
            }

            y = y0 + 1;
            while (_gemTypes[x, y] == _gemTypes[x0, y0]) //往上探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                y++;
            }

            return false;
        }


        /// <summary>
        /// 未测试：判断传入的盘面有无配对，时间复杂度 O(N²)
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
            if (gemTypes[x0, y0] == 0)
            {
                Debug.Log("传入的矩阵中此位为空位");
                return false;
            }

            int x = x0;
            int y = y0;
            int count = 0;
            while (gemTypes[x, y] == gemTypes[x0, y0]) //往左探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                x--;
            }

            x = x0 + 1;
            while (gemTypes[x, y] == gemTypes[x0, y0]) //往右探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                x++;
            }

            x = x0;
            count = 0; //左右探测完毕，从0开始上下探测
            while (gemTypes[x, y] == gemTypes[x0, y0]) //往下探测
            {
                count++;
                if (count == 3)
                {
                    return true;
                }

                y--;
            }

            y = y0 + 1;
            while (gemTypes[x, y] == gemTypes[x0, y0]) //往上探测
            {
                count++;
                if (count == 3)
                {
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
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    tempGemTypes[i, j] = _gemTypes[i, j];
                }
            }

            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 2; j++) //每一个宝石与后一个宝石交换，检查是否有效，再换回
                {
                    int temp = tempGemTypes[i, j];
                    tempGemTypes[i, j] = tempGemTypes[i, j + 1];
                    tempGemTypes[i, j + 1] = temp;

                    if (HasMatch(tempGemTypes, i, j) || HasMatch(tempGemTypes, i, j + 1))
                    {
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
                for (int i = 1; i < N - 2; i++) //每一个宝石与后一个宝石交换，检查是否有效，再换回
                {
                    int temp = tempGemTypes[i, j];
                    tempGemTypes[i, j] = tempGemTypes[i + 1, j];
                    tempGemTypes[i + 1, j] = temp;

                    if (HasMatch(tempGemTypes, i, j) || HasMatch(tempGemTypes, i + 1, j))
                    {
                        count++;
                    }

                    temp = tempGemTypes[i, j];
                    tempGemTypes[i, j] = tempGemTypes[i + 1, j];
                    tempGemTypes[i + 1, j] = temp;
                }
            }
            
            Debug.Log("有效交换数"+count);

            return count; 
        }

        
        public void SearchMatch()
        {
            Update_gemTypes();

            _matchMatrixSetZero();
            
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    if (HasMatch(i, j))
                    {
                        _matchMatrix[i, j] = _gemTypes[i, j];
                        
                    }
                    //Debug.Log(_matchMatrix[i, j]);
                }
            }
            
            
        }


        private void _matchMatrixSetZero()
        {
            for (int i = 0; i < N; i++) {
                for (int j = 0; j < N; j++) {
                    _matchMatrix[i, j] = 0;
                }
            }
        }


        /// <summary>
        /// to-do：完成消除计数
        /// </summary>
        /// <param name="gems"></param>
        public void Remove()
        {
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    if (_matchMatrix[i, j] > 0)
                    {
                        Gems[i, j].gemType = Gem.GemType.Empty;
                    }

                    //Debug.Log(Gems[i, j].gemType);
                }
            }
            
            _matchMatrixSetZero();
        }


        public void Fall()
        {
            int[] emptyCount = new int[N];
            
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    if (Gems[i, j].gemType == Gem.GemType.Empty)
                    {
                        emptyCount[i]++;   //为每一列的空位计数
                    }
                }
            }

            for (int i = 1; i < N - 1; i++)   //遍历每一列
            {
                Debug.Log("第"+i+"列空位数："+emptyCount[i]);
                if (emptyCount[i] == 0 || emptyCount[i] == N - 2)
                {
                    continue;   //如果没有空位或者全是空位，则到下一列
                }
                
                for (int count = 0; count < emptyCount[i];)   //循环次数为空位的个数
                {
                    Debug.Log("处理第"+(count+1)+"个空位");
                    for (int j = 1; j < N - 1; j++)
                    {
                        if (Gems[i, j].gemType == Gem.GemType.Empty)   //从下往上找到第一个空位
                        {
                            Debug.Log("找到了第" + (count + 1) + "个空位，在" + i + "," + j);
                            for (int k = j; k < N - 2; k++)
                            {
                                Gem temp = Gems[i, k];
                                Gems[i, k] = Gems[i, k + 1];
                                Gems[i, k + 1] = temp;   //此空位上移至顶端，宝石和其它空位下移1格
                                Debug.Log(i + "," + (k + 1) + "宝石掉落一格");
                            }
                            count++;
                            break;   //一个空位已移除，跳到下一个空位
                        }
                    }
                }
            }
            
            TestDisplay();
        }


        public void TestDisplay()
        {
            for (int i = 1; i < N-1 ; i++)
            {
                Debug.Log("第"+i+"列");
                
                for (int j = 1; j < N-1 ; j++)
                {
                    Debug.Log(Gems[i, j].gemType);
                }
            }
        }
    }
}