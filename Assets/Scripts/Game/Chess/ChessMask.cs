using UnityEngine;

namespace Game.GameChess {
	[CreateAssetMenu(fileName = "ChessMask", menuName = "Custom/ChessMask", order = 1)]
	public class ChessMask : ScriptableObject 
	{
		[Header("移动规则")]
		[SerializeField] private MoveRuleSO _moveRule;
		
		[Header("等级")]
		[SerializeField] private int _level;
		
		[Header("阵营")]
		[SerializeField] private Faction _faction = Faction.Neutral;
		
		public int Level => _level;
		public Faction Faction => _faction;
		public MoveRuleSO MoveRule => _moveRule;
		
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
			if (_moveRule == null) {
				Debug.LogWarning($"[ChessMask] {name} 的 MoveRule 未设置！");
				return Vector2Int.zero;
			}
			
			return _moveRule.GetMoveDirection(moveStepIndex);
		}
	}
}
