using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.GameChess {
	[System.Serializable]
	public class ChessMask
	{
		[Header("等级")]
		[SerializeField] private int _level;
		
		[Header("阵营")]
		[SerializeField] private Faction _faction = Faction.Neutral;
		
		public int Level => _level;
		public Faction Faction => _faction;

		public List<MoveRuleSO> moveRules = new List<MoveRuleSO>();
		public int _dirIndex= 0;

		public bool IsKing {set; get; } = false;
		
		/// <summary>
		/// 判断是否可以攻击目标面具
		/// 根据 level 值大小决定是否能吃子
		/// </summary>
		public bool CanAttack(ChessMask targetMask) 
		{
			if (targetMask == null) return false;
			
			// 友方不能攻击友方
			if (targetMask._faction == this._faction) 
			{
				return false;
			}
			
			// 只有 level 更高才能吃掉对方
			return this._level > targetMask._level;
		}
		
		/// <summary>
		/// 获取指定 MoveStep 的移动方向
		/// </summary>
		public Vector2Int GetMoveDir(int moveStepIndex) 
		{
			if (moveRules == null || moveRules.Count == 0) {
				return Vector2Int.zero;
			}

			if(_dirIndex < 0 || _dirIndex >= moveRules.Count)
			{
				return Vector2Int.zero;
			}

			return moveRules[_dirIndex].GetMoveDirection(moveStepIndex);
		}

		public void ChangeMoveDir()
		{
			_dirIndex = (_dirIndex + 1) % moveRules.Count;
		}

		public int GetMoveMaxCount()
		{
			if (moveRules == null || moveRules.Count == 0) {
				return 0;
			}

			if(_dirIndex < 0 || _dirIndex >= moveRules.Count)
			{
				return 0;
			}

			return moveRules[_dirIndex].GetMoveMaxCount();
		}
	}
}
