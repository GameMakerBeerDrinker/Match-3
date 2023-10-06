using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts
{
    public enum GameState{
        Waiting,
        Selecting,
        Swapping,
        Matching,
        ReSwapping,
        Falling,
        End,
    }
    public class GameManager : MonoBehaviour
    {
        private const int BoardLength = 8;   //配置游戏的行列数（不包括边界）
        private const int GemTypeCount = 7;   //配置宝石的种类数
        
        private const int BoardLengthWithSides = BoardLength + 2;


        public static GameManager Manager;

        public Gem gemPrefab;

        public Gem[,] Gems;

        private int[,] _gemTypes;

        private int[,] _matchMatrix;   //0为不消，1-7为对应颜色的宝石消

        public int gemCount;

        public bool canMove = true;

        public GameState gameState;

        public int matchedGemsCount;

        public TMP_Text matchedGemsCountText;


        private void Awake()
        {
            if (Manager == null)
                Manager = this;
            else DestroyImmediate(this);
        }


        private void Start() {
            gameState = GameState.Waiting;
            swapDir = -1;
            GetValidGameBoard();

            _refillBoard = new int[9, 9];
        }

        public Vector3 curMouseWorldPos;
        public Vector2Int curMouseLogicPos;

        public Vector2 inSwapPos;
        public Vector2 swapPointer;

        public Vector2Int swapBeg;
        public Vector2Int swapTar;
        public int swapDir;
        
        public Vector2Int[] dir;
        private void Update() {
            curMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            curMouseWorldPos = new Vector3(curMouseWorldPos.x, curMouseWorldPos.y, -5);
            curMouseLogicPos = WorldPosToLogicPos(curMouseWorldPos);
            
            switch (gameState) {
                case GameState.Waiting:
                    ChangeGemTarScale(curMouseLogicPos, 0.5f);
                    if (Input.GetMouseButtonDown(0)) {
                        gameState = GameState.Selecting;
                        inSwapPos = curMouseWorldPos;
                    }
                    break;
                
                case GameState.Selecting:
                    swapPointer = (Vector2)curMouseWorldPos - inSwapPos;
                    if (Input.GetMouseButtonUp(0)) {
                        if (swapPointer.magnitude > 0.5f) {
                            swapDir = GetSwapDir(swapPointer);
                        }
                        else {
                            gameState = GameState.Waiting;
                        }
                        
                    }
                    if (swapPointer.magnitude > 1.5f) {
                        swapDir = GetSwapDir(swapPointer);
                    }

                    if (swapDir != -1) {
                        swapBeg = WorldPosToLogicPos(inSwapPos);
                        swapTar = swapBeg + dir[swapDir];
                        gameState = GameState.Swapping;
                        swapDir = -1;
                    }
                    break;
                
                case GameState.Swapping:
                    GetGemFromLogicPos(swapBeg).transform.position
                        = GetGemFromLogicPos(swapBeg).transform.position
                            .ApproachValue(LogicPosToWorldPos(swapTar), 64f * Vector3.one);
                    
                    GetGemFromLogicPos(swapTar).transform.position
                        = GetGemFromLogicPos(swapTar).transform.position
                            .ApproachValue(LogicPosToWorldPos(swapBeg), 64f * Vector3.one);

                    if (GetGemFromLogicPos(swapBeg).transform.position.Equal(LogicPosToWorldPos(swapTar),0.01f) &&
                        GetGemFromLogicPos(swapTar).transform.position.Equal(LogicPosToWorldPos(swapBeg),0.01f)) {
                        Swap(swapBeg,swapTar);
                        if (HasMatch()) {
                            SearchMatch();
                            RemoveMatchedGems();
                            gameState = GameState.Matching;
                        }
                        else {
                            Swap(swapBeg,swapTar);
                            gameState = GameState.ReSwapping;
                        }
                    }
                    break;
                
                case GameState.Matching:
                    if (IfAllGemsRemoved()) {
                        LogicFallAndRefill();
                        gameState = GameState.Falling;
                    }
                    break;
                
                case GameState.ReSwapping:
                    GetGemFromLogicPos(swapBeg).transform.position
                        = GetGemFromLogicPos(swapBeg).transform.position
                            .ApproachValue(LogicPosToWorldPos(swapBeg), 64f * Vector3.one);
                    
                    GetGemFromLogicPos(swapTar).transform.position
                        = GetGemFromLogicPos(swapTar).transform.position
                            .ApproachValue(LogicPosToWorldPos(swapTar), 64f * Vector3.one);

                    if (GetGemFromLogicPos(swapBeg).transform.position.Equal(LogicPosToWorldPos(swapBeg),0.01f) &&
                        GetGemFromLogicPos(swapTar).transform.position.Equal(LogicPosToWorldPos(swapTar), 0.01f)) {
                        gameState = GameState.Waiting;
                    }
                    break;
                
                case GameState.Falling:
                    AnimateFall();
                    if (IfAllGemsInPosition()) {
                        if (HasMatch()) {
                            SearchMatch();
                            RemoveMatchedGems();
                            gameState = GameState.Matching;
                        }
                        else if(HasMove() > 0){
                            gameState = GameState.Waiting;
                        }
                        else {
                            gameState = GameState.End;
                            matchedGemsCountText.text = "Gems You Matched:\n" +
                                                        "<size=24>" + matchedGemsCount + "</size>\n" +
                                                        "You are dead.";
                        }
                    }
                    break;
            }
        }
        
        
        public int GetSwapDir(Vector2 p) {
            float d = Vector2.SignedAngle(p,new Vector2(-1,1));
            d = Mathf.Repeat(d, 360);
            d /= 90;
            if (d < 0) {
                d = 3 + d;
            }
            return (int)d;
        }

        public Gem GetGemFromLogicPos(Vector2Int p) {
            return Gems[p.x, p.y];
        }

        /// <summary>
        /// check if the gem in v2i p is valid gem(not out of range / empty or edge)
        /// </summary>
        /// <param name="p">pos in v2i</param>
        /// <returns></returns>
        public bool IsValidGem(Vector2Int p) {
            if (p.x <= 0 || p.x >= 9 || p.y <= 0 || p.y >= 9)
                return false;
            return (Gems[p.x, p.y].gemType != Gem.GemType.Empty
                    && Gems[p.x, p.y].gemType != Gem.GemType.Edge);
        }

        public void Swap(Vector2Int p, int d) {
            if(IsValidGem(p) && IsValidGem(p + dir[d]))
                Swap(p, p + dir[d]);
        }
        
        public void Swap(Vector2Int p1, Vector2Int p2) {
            Swap(p1.x,p1.y,p2.x,p2.y);
        }
        
        /// <summary>
        /// 交换两颗宝石，并判断是否为有效交换
        /// </summary>
        public void Swap(int x1,int y1,int x2,int y2) 
        {

            if (!IsAdjacent(x1, y1, x2, y2))
            {
                Debug.Log("不能交换两个不相邻的宝石");
                return;
            }

            Gems[x1, y1].logicPos.Set(x2, y2);
            Gems[x2, y2].logicPos.Set(x1, y1);
            (Gems[x1, y1], Gems[x2, y2]) = (Gems[x2, y2], Gems[x1, y1]);
            
            UpdateGemTypeOnBoard();
            
            if (!HasMatch(x1, y1) && !HasMatch(x2, y2)) //判断是否为有效交换
            {
                Debug.Log("无效交换，请换回");
                return;
            }

            Debug.Log("有效交换");
        }

        public bool IfAllGemsInPosition() {
            for (int i = 1; i <= BoardLength; i++) {
                for (int j = 1; j <= BoardLength; j++) {
                    if(!Gems[i,j].transform.position.Equal(LogicPosToWorldPos(i,j),0.01f))
                        return false;
                }
            }

            return true;
        }
        

        /// <summary>
        /// 生成一个无配对且有有效交换的盘面，并赋给当前盘面
        /// </summary>
        private void GetValidGameBoard() {
            Gems = new Gem[BoardLengthWithSides, BoardLengthWithSides];
            _gemTypes = new int[BoardLengthWithSides, BoardLengthWithSides];
            _matchMatrix = new int[BoardLengthWithSides, BoardLengthWithSides];
            for (int i = 0; i < BoardLengthWithSides; i++) {
                for (int j = 0; j < BoardLengthWithSides; j++) {
                    Gems[i, j] = Instantiate(gemPrefab);
                    Gems[i, j].gemType = Gem.GemType.Edge;
                }
            }
            
            do GetRandomGameBoard();
            while (HasMatch() || HasMove() == 0);
            
            for (int i = 0; i < BoardLengthWithSides; i++) {
                for (int j = 0; j < BoardLengthWithSides; j++) {
                    Gems[i, j].RefreshGemType();
                    Gems[i, j].transform.position = new Vector3(i - 4.5f, j - 4.5f,0);
                    Gems[i, j].logicPos.Set(i, j);
                }
            }
        }
        
        private void GetRandomGameBoard() {
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    Gems[i, j].gemType = (Gem.GemType)Random.Range(1, GemTypeCount+1);
                }
            }
        }

        #region ToolFuncs
        
        /// <summary>
        /// 判断两个坐标是否相邻
        /// </summary>
        private bool IsAdjacent(int x1, int y1, int x2, int y2) {
            return (Mathf.Abs(x1 - x2) == 1 && y1 == y2)
                   || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
        }
        
        public Vector3 LogicPosToWorldPos(int i, int j) {
            return new Vector3(i - 4.5f, j - 4.5f, 0);
        }
        
        public Vector3 LogicPosToWorldPos(Vector2Int p) {
            return new Vector3(p.x - 4.5f, p.y - 4.5f, 0);
        }

        public Vector2Int WorldPosToLogicPos(Vector3 pos) {
            return new Vector2Int(Mathf.FloorToInt(pos.x + 5f), Mathf.FloorToInt(pos.y + 5f));
        }

        public bool IsLogicPosValid(Vector2Int pos) {
            return pos.x is >= 1 and <= 8 && pos.y is >= 1 and <= 8;
        }
        
        public bool IsLogicPosValid(int x,int y) {
            return x is >= 1 and <= 8 && y is >= 1 and <= 8;
        }

        public void ChangeGemTarScale(Vector2Int pos, float tarScale) {
            if(IsLogicPosValid(pos)) Gems[pos.x, pos.y].tarScale = tarScale;
        }
        
        public void ChangeGemTarScale(int x,int y, float tarScale) {
            if (IsLogicPosValid(x, y)) Gems[x, y].tarScale = tarScale;
        }
        
        /// <summary>
        /// 将当前盘面的宝石类型信息更新到_gemTypes[N,N]中
        /// </summary>
        private void UpdateGemTypeOnBoard() {
            for (int i = 0; i < BoardLengthWithSides; i++) {
                for (int j = 0; j < BoardLengthWithSides; j++) {
                    _gemTypes[i, j] = (int)Gems[i, j].gemType;
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// 判断当前盘面有无配对，时间复杂度 O(N²)
        /// </summary>
        public bool HasMatch()
        {
            UpdateGemTypeOnBoard();

            for (int i = 1; i < BoardLengthWithSides - 1; i++)
            {
                int temp = _gemTypes[i, 1];
                int count = 1;
                for (int j = 2; j < BoardLengthWithSides - 1; j++)
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
            for (int j = 1; j < BoardLengthWithSides - 1; j++)
            {
                int temp = _gemTypes[1, j];
                int count = 1;
                for (int i = 2; i < BoardLengthWithSides - 1; i++)
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
        public bool HasMatch(int x0, int y0)
        {
            UpdateGemTypeOnBoard();

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

        public bool HasMatch(Vector2Int p) {
            return HasMatch(p.x, p.y);
        }


        /// <summary>
        /// 未测试：判断传入的盘面有无配对，时间复杂度 O(N²)
        /// </summary>
        /// <param name="gemTypes"></param>
        /// <returns></returns>
        private bool HasMatch(int[,] gemTypes)
        {
            //三连存在性判断，时间复杂度 O(N²)
            for (int i = 1; i < BoardLengthWithSides - 1; i++)
            {
                int temp = gemTypes[i, 1];
                int count = 1;
                for (int j = 2; j < BoardLengthWithSides - 1; j++)
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
            for (int j = 1; j < BoardLengthWithSides - 1; j++)
            {
                int temp = gemTypes[1, j];
                int count = 1;
                for (int i = 2; i < BoardLengthWithSides - 1; i++)
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

            UpdateGemTypeOnBoard();

            int[,] tempGemTypes = new int[BoardLengthWithSides, BoardLengthWithSides];
            for (int i = 0; i < BoardLengthWithSides; i++)
            {
                for (int j = 0; j < BoardLengthWithSides; j++)
                {
                    tempGemTypes[i, j] = _gemTypes[i, j];
                }
            }

            for (int i = 1; i < BoardLengthWithSides - 1; i++)
            {
                for (int j = 1; j < BoardLengthWithSides - 2; j++) //每一个宝石与后一个宝石交换，检查是否有效，再换回
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
            for (int j = 1; j < BoardLengthWithSides - 1; j++)
            {
                for (int i = 1; i < BoardLengthWithSides - 2; i++) //每一个宝石与后一个宝石交换，检查是否有效，再换回
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

        
        public void SearchMatch() {
            UpdateGemTypeOnBoard();
            MatchMatrixSetZero();
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (HasMatch(i, j)) {
                        _matchMatrix[i, j] = _gemTypes[i, j];
                    }
                }
            }
        }


        private void MatchMatrixSetZero() {
            for (int i = 0; i < BoardLengthWithSides; i++) {
                for (int j = 0; j < BoardLengthWithSides; j++) {
                    _matchMatrix[i, j] = 0;
                }
            }
        }

        public void RemoveMatchedGems() {
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                //string str = " ";
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (_matchMatrix[i, j] > 0) {
                        Gems[i, j].gemType = Gem.GemType.Empty;
                        matchedGemsCount++;
                    }

                    //str += _matchMatrix[i, j] + " ";
                }
                //Debug.Log(str);
            }
            matchedGemsCountText.text = "Gems You Matched:\n" +
                                        "<size=24>" + matchedGemsCount + "</size>";
        }

        public bool IfAllGemsRemoved() {
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (_matchMatrix[i, j] > 0) {
                        if (!Gems[i, j].curScale.Equal(0f, 0.01f)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        
        public void LogicFallAndRefill() {
            int[] emptyCount = new int[BoardLengthWithSides];
            
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (Gems[i, j].gemType == Gem.GemType.Empty) {
                        emptyCount[i]++;   //为每一列的空位计数
                    }
                }
            }

            for (int i = 1; i < BoardLengthWithSides - 1; i++) {  //遍历每一列{
                //Debug.Log("第"+i+"列空位数："+emptyCount[i]);
                if (emptyCount[i] == 0 || emptyCount[i] == BoardLengthWithSides - 2) continue;   
                //如果没有空位或者全是空位，则到下一列
                for (int count = 0; count < emptyCount[i];){   //循环次数为空位的个数
                    //Debug.Log("处理第"+(count+1)+"个空位");
                    for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                        if (Gems[i, j].gemType == Gem.GemType.Empty){   //从下往上找到第一个空位
                            //Debug.Log("找到了第" + (count + 1) + "个空位，在" + i + "," + j);
                            for (int k = j; k < BoardLengthWithSides - 2; k++) {
                                (Gems[i, k], Gems[i, k + 1]) = (Gems[i, k + 1], Gems[i, k]);
                                //Debug.Log(i + "," + (k + 1) + "宝石掉落一格");
                            }
                            count++;
                            break;   //一个空位已移除，跳到下一个空位
                        }
                    }
                }
            }
            
            
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (Gems[i, j].gemType == Gem.GemType.Empty) {
                        Destroy(Gems[i, j].gameObject);
                        Gems[i, j] = Instantiate(gemPrefab, LogicPosToWorldPos(i, BoardLength + j),
                            Quaternion.Euler(0, 0, 0));
                        Gems[i,j].gemType = (Gem.GemType)Random.Range(1, GemTypeCount+1);
                        Gems[i,j].RefreshGemType();
                    }
                }
            }
            
            //Refill game board, new gems are spawned upside of the board
            GetValidRefillBoard();
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (Gems[i, j].gemType == Gem.GemType.Empty) {
                        Destroy(Gems[i, j].gameObject);
                        Gems[i, j] = Instantiate(gemPrefab, LogicPosToWorldPos(i, BoardLength + j),
                            Quaternion.Euler(0, 0, 0));
                        Gems[i, j].gemType = (Gem.GemType)_refillBoard[i, j];
                        Gems[i, j].RefreshGemType();
                    }
                }
            }
        }
        
        private int[,] _refillBoard;

        private void FillRefillBoardWithZero() {
            for (int i = 0; i < 9; i++) {
                for (int j = 0; j < 9; j++) {
                    _refillBoard[i, j] = 0;
                }
            }
        }

        private void GetValidRefillBoard() {
            FillRefillBoardWithZero();
            do {
                for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                    for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                        if (Gems[i, j].gemType == Gem.GemType.Empty) {
                            _refillBoard[i, j] = Random.Range(1, GemTypeCount + 1);
                        }
                    }
                }
            } while (!HasMatch(_refillBoard));
        }

        public void AnimateFall() {
            for (int i = 1; i < BoardLengthWithSides - 1; i++) {
                for (int j = 1; j < BoardLengthWithSides - 1; j++) {
                    if (Gems[i, j] != null) {
                        Gems[i, j].transform.position = Gems[i, j].transform.position
                            .ApproachValue(LogicPosToWorldPos(i, j), 64f * Vector3.one);
                    }
                }
            }
        }


        public void TestDisplay()
        {
            for (int i = 1; i < BoardLengthWithSides-1 ; i++)
            {
                Debug.Log("第"+i+"列");
                
                for (int j = 1; j < BoardLengthWithSides-1 ; j++)
                {
                    Debug.Log(Gems[i, j].gemType);
                }
            }
        }
        
        
        
    }
}