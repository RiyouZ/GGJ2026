
using Game.GameChess;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
	/// <summary>
	/// 存储格子信息的类
	/// </summary>
	public class Cell : MonoBehaviour
	{
		
		[SerializeField]
		private CellType _cellType = CellType.Normal;

		/// <summary>
		/// 当前格子的棋子
		/// </summary>
		[System.NonSerialized]
		public Chess Chess;

		/// <summary>
		/// 能否行走（只读属性，与CellType相关）
		/// </summary>
		public bool CanWalk
		{
			get
			{
				// 根据CellType判断是否可行走
				return _cellType == CellType.Normal;
			}
		}

		/// <summary>
		/// 格子类型
		/// </summary>
		public CellType CellType => _cellType;

		/// <summary>
		/// 逻辑坐标X
		/// </summary>
		[System.NonSerialized]
		public int X;

		/// <summary>
		/// 逻辑坐标Y
		/// </summary>
		[System.NonSerialized]
		public int Y;



		/// <summary>
		/// 设置棋子到格子（不会自动清除原有棋子，具体战斗逻辑不由Grid或Cell管理）
		/// </summary>
		/// <param name="chess">要设置的棋子</param>
		public void SetChess(Chess chess)
		{
			Chess = chess;
		}

		/// <summary>
		/// 格子高亮表现
		/// </summary>

		public void Highlight()
		{
			// TODO: 可通过Tilemap.SetColor等方式高亮
	#if UNITY_EDITOR
			Debug.Log($"Cell ({X}, {Y}) highlighted");
	#endif
		}

		/// <summary>
		/// 取消格子高亮表现
		/// </summary>

		public void CancelHighlight()
		{
			// TODO: 可通过Tilemap.SetColor等方式取消高亮
	#if UNITY_EDITOR
			Debug.Log($"Cell ({X}, {Y}) highlight cancelled");
	#endif
		}

		/// <summary>
		/// 销毁格子资源
		/// </summary>
		public void Dispose()
		{
			Chess = null;
		}
	}
}
