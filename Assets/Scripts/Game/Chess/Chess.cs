using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using RuGameFramework.Event;

namespace Game.GameChess {
	public enum MoveResult 
	{
		Success,
		Blocked,
		OutOfBounds,
		CannotAttack,
		Complete,
		End
	}

	public class Chess : MonoBehaviour 
	{
		[Header("面具配置")]
		[SerializeField] private ChessMask _chessMask;
		
		[Header("当前移动阶段索引")]
		private int _moveStepIndex = 0;
		
		[Header("当前格子位置（逻辑坐标）")]
		private Vector2Int _currentPos;
		
		// 属性访问器
		public virtual Faction Faction => _chessMask?.Faction ?? Faction.Neutral;
		public int Level => _chessMask?.Level ?? 0;
		public ChessMask ChessMask => _chessMask;
		public int MoveStepIndex => _moveStepIndex;
		public Vector2Int CurrentPos => _currentPos;
		
		/// <summary>
		/// 设置面具
		/// </summary>
		public void SetMask(ChessMask mask) 
		{
			_chessMask = mask;
		}

		// 替换面具
		public void SwapMask(Chess newChess) 
		{
			var tempMask = this._chessMask;
			this._chessMask = newChess._chessMask;
			newChess._chessMask = tempMask;

			// TODO 刷新皮肤
		}

		public void OnTurnStart()
		{
			ResetMove();
		}
		
		/// <summary>
		/// 重置移动标记索引
		/// </summary>
		private void ResetMove() 
		{
			_moveStepIndex = 0;
			_isMoveSuccessful = false;
		}
		
		// 为True时 后续不能移动
		private bool _isMoveSuccessful = false;

		/// <summary>
		/// 移动主流程
		/// 根据 mask 的规则移动，每次只移动一个单位格
		/// </summary>
		/// <returns>是否成功移动</returns>
		public MoveResult Move() 
		{
			if (_chessMask == null) 
			{
				Debug.LogWarning($"[Chess] {name} 没有设置 ChessMask！");
				return MoveResult.Complete;
			}

			if (IsMoveComplete())
			{
				// 防止回合计数
				return MoveResult.End;
			}

			// 获取下一步移动方向
			Vector2Int dir = _chessMask.GetMoveDir(_moveStepIndex);
			Vector2Int nextPos = _currentPos + dir;

			// 检查越界
			if (IsOutOfBounds(nextPos)) 
			{
				return MoveResult.Complete;
			}
			
			// 检查 CanWalk
			if (!GameScene.GridSystem.CanWalk(nextPos)) 
			{
				return MoveResult.Complete;
			}
			
			// 检查目标格子是否有棋子
			Chess target = GameScene.GetChess(nextPos);
			if (target != null) 
			{
				// 友方不可移动
				if (target.Faction == this.Faction) 
				{
					_isMoveSuccessful = true;
					return MoveResult.Complete;
				}
				
				// 比较 level
				Debug.Log($"[Chess] {name} 尝试攻击 {target.name} 等级 {target.Level}");
				if (ChessMask.CanAttack(target.ChessMask)) 
				{
					// 消灭对方，移动到目标格子
					Debug.Log($"[Chess] {name} 吃掉 {target.name}");
					target.Die();
					// 消灭停止移动
					MoveToPosition(nextPos);
					// 吃掉后停止
					return MoveResult.Complete;
				} 
				else 
				{
					// level >= 当前棋子，不可移动
					Debug.Log($"[Chess] {name} 目标棋子 level 更高或相等，不可移动");
					return MoveResult.Complete;
				}
			} 
			else 
			{
				// 没有任何障碍时，正常移动
				MoveToPosition(nextPos);
				_moveStepIndex ++;
				if (IsMoveComplete()) 
				{
					return MoveResult.Complete;
				}

				return MoveResult.Complete;
			}
		}
		
		private bool IsMoveComplete() 
		{
			if (_moveStepIndex >= _chessMask.GetMoveMaxCount() || _isMoveSuccessful)
			{
				return true;
			}

			return false;
		}
		
		/// <summary>s
		/// 判断位置是否越界
		/// </summary>
		private bool IsOutOfBounds(Vector2Int pos) 
		{
			// 委托给 GridSystem 判断
			return !GameScene.GridSystem.IsValidPosition(pos);
		}
		
		/// <summary>
		/// 移动到指定位置
		/// </summary>
		private void MoveToPosition(Vector2Int newPos) 
		{
			EventManager.InvokeEvent(GameScene.EVENT_CHESS_MOVE, new ChessMoveArgs(this, _currentPos, newPos));
			_currentPos = newPos;

			// 更新世界坐标
			Vector2 worldPos = GameScene.GetCellWorld(newPos.x, newPos.y);
			transform.position = worldPos;
		}
		
		/// <summary>
		/// 死亡处理（播放动画，但不触发回合）
		/// </summary>
		public void Die() 
		{
			Debug.Log($"[Chess] {name} 死亡");
			
			// TODO: 播放死亡动画
			// TODO: 从 GridSystem 移除
			
			// 暂时直接销毁
			Destroy(gameObject);
		}
		
		/// <summary>
		/// 获取棋子的完整移动路径（用于预览）
		/// </summary>
		/// <returns>路径点数组</returns>
		public List<Vector2Int> Previs() 
		{
			List<Vector2Int> path = new List<Vector2Int>();
			
			if (_chessMask == null || _chessMask.moveRules == null || _chessMask.moveRules.Count == 0) {
				Debug.LogWarning($"[Chess] {name} 无法生成预览路径，ChessMask 或 MoveRule 为空");
				return path;
			}
			
			Vector2Int simulatedPos = _currentPos;
			int maxSteps = _chessMask.GetMoveMaxCount();
			
			// 从当前 moveStepIndex 开始模拟移动
			for (int i = 0; i < maxSteps; i++) {
				int stepIndex = _moveStepIndex + i;
				Vector2Int dir = _chessMask.GetMoveDir(stepIndex);
				Vector2Int nextPos = simulatedPos + dir;
				
				// 越界或不可行走则停止
				if (IsOutOfBounds(nextPos) || !GameScene.GridSystem.CanWalk(nextPos)) {
					break;
				}
				
				// 检查是否有棋子
				Chess target = GameScene.GetChess(nextPos);
				if (target != null) {
					// 友方则停止
					if (target.Faction == this.Faction) 
					{
						break;
					}

					// 可以吃掉则加入路径，但不继续模拟（吃子后停止）
					if (target.Level < this.Level) 
					{
						path.Add(nextPos);
					}
					break;
				}
				
				path.Add(nextPos);
				simulatedPos = nextPos;
			}
			
			return path;
		}
		
		/// <summary>
		/// 初始化棋子位置
		/// </summary>
		public void Initialize() 
		{
			ResetMove();
			// 网格坐标
			_currentPos = GameScene.GetWorldCellPos(this.transform.position.x, this.transform.position.y);

			this.transform.position = GameScene.GetCellWorld(_currentPos.x, _currentPos.y);

			Debug.Log("[Chess] " + name + " initialized at " + _currentPos);
		}
	}
}
