using System;
using System.Collections;
using System.Collections.Generic;
using Frame.Audio;
using Game.GameChess;
using RuGameFramework.Event;
using UnityEngine;

namespace Game.Scene
{
	/// <summary>
	/// 回合系统 - 负责调度回合和触发胜利条件
	/// </summary>
	public class TurnSystem
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

		private const string EVENT_PLAYER_ACTION_COMPLETE = "TurnPlayerActionComplete";

		#region 字段
		[Header("当前回合状态")]
		[SerializeField] private TurnState _currentState = TurnState.PlayerAction;
		
		[Header("回合计数")]
		[SerializeField] private int _turnCount = 0;
		
		[Header("游戏是否结束")]
		private bool _isGameOver = false;
		
		// 棋子移动相关
		private List<Chess> _movingChessList;

		// 当前移动的棋子
		private int _movedChessCount = 0;

		// 移动完的棋子数量
		private int _completeChessCount = 0;
		
		// 阶段完成信号量
		private bool _isPlayerActionComplete = false;
		private bool _isChessMoveComplete = false;

		public bool CanPlayerAct => _currentState == TurnState.PlayerAction;
		
		// 协程宿主（用于启动协程）
		private MonoBehaviour _coroutineHost;

		#endregion

		#region 公共方法
		/// <summary>
		/// 初始化回合系统并开始运行
		/// </summary>
		/// <param name="host">协程宿主（通常是GameScene）</param>
		public void Initialize(MonoBehaviour host, List<Chess> allChess)
		{
			_coroutineHost = host;
			_movingChessList = allChess;
			_coroutineHost.StartCoroutine(TurnLoopCoroutine());

			EventManager.AddListener(EVENT_PLAYER_ACTION_COMPLETE, (args) => PlayerActionComplete());
		}
		#endregion

		#region 核心协程
		/// <summary>
		/// 回合循环协程 - 驱动整个回合流转
		/// </summary>
		private IEnumerator TurnLoopCoroutine()
		{
			while (!_isGameOver)
			{
				// 阶段1: 回合开始
				StartTurn();
				
				// 阶段2: 等待玩家操作完成
				_currentState = TurnState.PlayerAction;
				_isPlayerActionComplete = false;
				yield return new WaitUntil(() => _isPlayerActionComplete);
				
				GameScene.guideLine.ClearLine();
				// 阶段3: 棋子移动
				_currentState = TurnState.ChessMoving;
				yield return MoveAllChess();
				
				// 阶段4: 胜利判定
				_currentState = TurnState.VictoryCheck;
				bool isVictory = CheckVictory();
				if (isVictory)
				{
					// 游戏胜利，退出循环
					GameScene.MouseInteractSystem.ClearHighlight();
					EventManager.InvokeEvent(GameScene.EVENT_GAME_END, null);
					yield break;
				}
				
				// 阶段5: 回合结束
				_currentState = TurnState.TurnEnd;
				EndTurn();
				
				yield return null;
			}
		}
		#endregion

		public void PlayerActionComplete()
		{
			if (_currentState == TurnState.PlayerAction)
			{
				_isPlayerActionComplete = true;
			}
		}

		/// <summary>
		/// 开始新回合
		/// </summary>
		private void StartTurn()
		{
			_turnCount++;

			foreach(var chess in _movingChessList)
			{
				chess.OnTurnStart();
			}
			
			// 触发回合开始事件
			OnTurnStart?.Invoke();
		}

		/// <summary>
		/// 结束当前回合
		/// </summary>
		private void EndTurn()
		{	
			// 触发回合结束事件
			OnTurnEnd?.Invoke();
		}

		/// <summary>
		/// 检查胜利条件
		/// </summary>
		/// <returns>是否胜利</returns>
		private bool CheckVictory()
		{
			if(GameScene.IsVectory())
			{
				_isGameOver = true;
				OnGameVictory?.Invoke();
				return true;
			}

			return false;
		}

		/// <summary>
		/// 移动所有棋子
		/// </summary>
		private IEnumerator MoveAllChess()
		{
			// 根据level决定移动顺序
			_movingChessList.Sort((a, b) => b.Level.CompareTo(a.Level));

			while (true)
			{
				bool allCompleted = true;
				
				// 遍历每个棋子，触发移动
				foreach (var chess in _movingChessList)
				{   
					var moveResult = chess.Move();
					
					// 只要有一个棋子未结束，就继续循环
					if (moveResult != Chess.MoveState.MoveEnd)
					{
						allCompleted = false;
					}
				}

				// 等待所有棋子移动动画播放完毕
				yield return new WaitUntil(() => 
				{
					foreach (var chess in _movingChessList)
					{
						if (chess.IsMoving)
						{
							return false;
						}
					}
					return true;
				});
				
				// 如果所有棋子都已结束本回合移动，退出循环
				if (allCompleted)
				{
					break;
				}
			}

		}
	}
}
