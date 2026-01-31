using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scene
{
    /// <summary>
    /// 回合系统 - 负责调度回合和触发胜利条件
    /// </summary>
    public class TurnSystem : MonoBehaviour
    {
        #region 状态枚举
        public enum TurnState
        {
            PlayerAction,    // 等待玩家操作
            ChessMoving,     // 棋子移动中
            VictoryCheck,    // 胜利判定
            TurnEnd          // 回合结束
        }
        #endregion

        #region 事件定义
        /// <summary>回合开始事件</summary>
        public event Action OnTurnStart;
        
        /// <summary>回合结束事件</summary>
        public event Action OnTurnEnd;
        
        /// <summary>玩家操作结束事件</summary>
        public event Action OnPlayerActionEnd;
        
        /// <summary>所有棋子移动完毕事件</summary>
        public event Action OnAllChessMoved;
        
        /// <summary>游戏胜利事件</summary>
        public event Action OnGameVictory;
        #endregion

        #region 字段
        [Header("当前回合状态")]
        [SerializeField] private TurnState currentState = TurnState.PlayerAction;
        
        [Header("回合计数")]
        [SerializeField] private int turnCount = 0;
        
        [Header("游戏是否结束")]
        private bool isGameOver = false;
        
        // 棋子移动相关
        private List<GameObject> movingChessList = new List<GameObject>();
        private int movedChessCount = 0;
        #endregion

        #region Unity生命周期
        private void Start()
        {
            StartTurn();
        }

        private void Update()
        {
            if (isGameOver) return;
            
            // 状态机流转
            switch (currentState)
            {
                case TurnState.PlayerAction:
                    // 等待玩家点击结束回合按钮
                    break;
                    
                case TurnState.ChessMoving:
                    // 棋子移动由协程处理，这里只等待
                    break;
                    
                case TurnState.VictoryCheck:
                    CheckVictory();
                    break;
                    
                case TurnState.TurnEnd:
                    EndTurn();
                    break;
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始新回合
        /// </summary>
        public void StartTurn()
        {
            turnCount++;
            Debug.Log($"回合 {turnCount} 开始");
            
            // 触发回合开始事件
            OnTurnStart?.Invoke();
            
            // 进入玩家操作阶段
            currentState = TurnState.PlayerAction;
        }

        /// <summary>
        /// 玩家点击"结束回合"按钮时调用
        /// </summary>
        public void OnPlayerClickEndTurn()
        {
            if (currentState != TurnState.PlayerAction) return;
            
            Debug.Log("玩家结束操作，开始棋子移动");
            
            // 触发玩家操作结束事件
            OnPlayerActionEnd?.Invoke();
            
            // 进入棋子移动阶段
            currentState = TurnState.ChessMoving;
            StartCoroutine(MoveAllChess());
        }

        /// <summary>
        /// 结束当前回合
        /// </summary>
        private void EndTurn()
        {
            Debug.Log($"回合 {turnCount} 结束");
            
            // 触发回合结束事件
            OnTurnEnd?.Invoke();
            
            // 开始下一回合
            StartTurn();
        }

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        private void CheckVictory()
        {
            // TODO: 需要从外部获取我方棋子列表和胜利格子列表
            // 这里使用标签来查找，实际项目中可以通过依赖注入或单例获取
            GameObject[] myChessList = GameObject.FindGameObjectsWithTag("MyChess");
            GameObject[] victoryTiles = GameObject.FindGameObjectsWithTag("VictoryTile");
            
            if (myChessList.Length == 0 || victoryTiles.Length == 0)
            {
                Debug.LogWarning("未找到棋子或胜利格子，跳过胜利判定");
                currentState = TurnState.TurnEnd;
                return;
            }
            
            // 判断所有我方棋子是否都站在胜利格子上
            bool allOnVictoryTile = true;
            foreach (var chess in myChessList)
            {
                bool isOnVictoryTile = false;
                foreach (var tile in victoryTiles)
                {
                    // 简单的位置判断，实际项目中可能需要更精确的判定
                    if (Vector3.Distance(chess.transform.position, tile.transform.position) < 0.5f)
                    {
                        isOnVictoryTile = true;
                        break;
                    }
                }
                
                if (!isOnVictoryTile)
                {
                    allOnVictoryTile = false;
                    break;
                }
            }
            
            if (allOnVictoryTile)
            {
                // 游戏胜利
                Debug.Log("游戏胜利！所有棋子都在胜利格子上！");
                isGameOver = true;
                OnGameVictory?.Invoke();
            }
            else
            {
                // 未胜利，进入回合结束阶段
                currentState = TurnState.TurnEnd;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 移动所有棋子
        /// </summary>
        private IEnumerator MoveAllChess()
        {
            // TODO: 获取所有需要移动的棋子
            // 这里使用标签查找，实际项目中建议通过管理器获取
            GameObject[] allChess = GameObject.FindGameObjectsWithTag("Chess");
            movingChessList.Clear();
            movedChessCount = 0;
            
            Debug.Log($"开始移动 {allChess.Length} 个棋子");
            
            // 遍历每个棋子，触发移动
            foreach (var chess in allChess)
            {
                movingChessList.Add(chess);
                
                // TODO: 调用棋子的移动方法
                // 假设棋子有一个 IChessMovement 接口
                var movement = chess.GetComponent<IChessMovement>();
                if (movement != null)
                {
                    movement.StartMove(OnChessMoveComplete);
                }
                else
                {
                    // 如果没有移动组件，直接标记为完成
                    OnChessMoveComplete();
                }
            }
            
            // 等待所有棋子移动完毕
            while (movedChessCount < movingChessList.Count)
            {
                yield return null;
            }
            
            Debug.Log("所有棋子移动完毕");
            
            // 触发所有棋子移动完毕事件
            OnAllChessMoved?.Invoke();
            
            // 进入胜利判定阶段
            currentState = TurnState.VictoryCheck;
        }

        /// <summary>
        /// 单个棋子移动完成回调
        /// </summary>
        private void OnChessMoveComplete()
        {
            movedChessCount++;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取当前回合数
        /// </summary>
        public int GetTurnCount() => turnCount;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public TurnState GetCurrentState() => currentState;

        /// <summary>
        /// 游戏是否结束
        /// </summary>
        public bool IsGameOver() => isGameOver;
        #endregion
    }

    /// <summary>
    /// 棋子移动接口（示例）
    /// </summary>
    public interface IChessMovement
    {
        void StartMove(Action onComplete);
    }
}
