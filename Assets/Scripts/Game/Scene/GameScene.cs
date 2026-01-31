using System;
using System.Collections;
using System.Collections.Generic;
using Frame.Audio;
using Frame.Core;
using Game.GameChess;
using RuGameFramework.Event;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
	public class GameScene : MonoBehaviour
	{
		public const string EVENT_CHESS_MOVE = "EventChessMove";

		public const string EVENT_SCENE_PLAYER_SKILL = "ScenePlayerSkill";
		
		public Tilemap tilemap;
		public Vector2Int cellSize = new Vector2Int(9, 6);

		/// <summary>
		/// 网格系统（具体接口和职责见GridSystem_design）
		/// </summary>
		public static GridSystem GridSystem { get; private set; }

		/// <summary>
		/// 回合系统（具体接口和职责后续补充，详见回合系统相关文档）
		/// </summary>
		public static Scene.TurnSystem TurnSystem { get; private set; }
		/// </summary>
		public static MouseInteractSystem MouseInteractSystem { get; private set; }

		private List<Chess> _friendChesses = new List<Chess>(4);


		// Start is called before the first frame update
		void Start()
		{
			WwiseAudio.LoadBank("Main");
			
			// 初始化各个系统（具体依赖关系详见project文档架构）
			InitializeSystems();
			InitializeListeners();

		}

		private void InitializeListeners()
		{
			EventManager.AddListener(EVENT_CHESS_MOVE, (args) =>
			{
				ChessMoveArgs moveArgs = args as ChessMoveArgs;
				SetChess(moveArgs.Chess, moveArgs.From.x, moveArgs.From.y, moveArgs.To.x, moveArgs.To.y);
			});
		}

		void Update()
		{
			SystemOrderServer.UpdateSystemServer(Time.deltaTime);
			// 更新鼠标交互系统
			MouseInteractSystem?.Update();
		}
		
		void FixedUpdate()
		{
			SystemOrderServer.FixUpdateSystemServer(Time.fixedDeltaTime);
		}

		void LateUpdate()
		{
			SystemOrderServer.LateUpdateSystemServer(Time.deltaTime);
		}

		void OnDestroy()
		{
			SystemOrderServer.Dispose();
			
			// 销毁流程：调用所有系统的Dispose方法
			DisposeSystems();
		}

		#region 系统初始化与销毁

		/// <summary>
		/// 初始化所有系统
		/// </summary>
		private void InitializeSystems()
		{
			var allChess = FindObjectsByType<Chess>(FindObjectsSortMode.None);

			// 初始化GridSystem
			if (tilemap != null)
			{
				GridSystem = new GridSystem(tilemap, cellSize, new List<Cell>(GetComponentsInChildren<Cell>()));
				GridSystem.Initialize(allChess);
			}

			_friendChesses.Clear();
			foreach(var chess in allChess)
			{
				if(chess.Faction == Faction.Friend)
				{
					_friendChesses.Add(chess);
				}
			}
			
			// 初始化TurnSystem
			TurnSystem = new Scene.TurnSystem();
			TurnSystem.Initialize(this, _friendChesses);
			
			// 初始化MouseInteractSystem（传入TurnSystem用于权限检查）
			MouseInteractSystem = new MouseInteractSystem(tilemap, GridSystem, TurnSystem);
			MouseInteractSystem.Initialize();
		}

		/// <summary>
		/// 销毁所有系统
		/// </summary>
		private void DisposeSystems()
		{
			MouseInteractSystem?.Dispose();
			MouseInteractSystem = null;
			
			TurnSystem = null;
			
			GridSystem?.Dispose();
			GridSystem = null;

		}

		#endregion

		#region 格子与棋子相关方法

		/// <summary>
		/// 获取x, y 逻辑坐标中的棋子（一个格子只能有一个棋子）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>所在格子的棋子类</returns>
		public static Chess GetChess(int x, int y)
		{
			if (GridSystem == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("GridSystem is not initialized");
#endif
				return null;
			}
			
			Cell cell = GridSystem.GetLocalCell(x, y);
			return cell?.Chess;
		}

		/// <summary>
		/// 获取逻辑坐标中的棋子（Vector2Int 重载）
		/// </summary>
		/// <param name="pos">逻辑坐标</param>
		/// <returns>所在格子的棋子类</returns>
		public static Chess GetChess(Vector2Int pos)
		{
			return GetChess(pos.x, pos.y);
		}

		public static void SetChess(Chess chess, int ox, int oy, int x, int y)
		{
			if (GridSystem == null)
			{
				return;
			}

			GridSystem.GetLocalCell(ox, oy)?.RemoveChess();
			GridSystem.GetLocalCell(x, y)?.SetChess(chess);
		}

		/// <summary>
		/// 获取x, y 逻辑坐标中的格子
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>返回逻辑坐标格子</returns>
		public static Cell GetCell(int x, int y)
		{
			if (GridSystem == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("GridSystem is not initialized");
#endif
				return null;
			}
			
			return GridSystem.GetLocalCell(x, y);
		}

		/// <summary>
		/// 获取x, y 逻辑坐标中的格子的类型（类型定义见GridSystem_design）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>返回逻辑坐标格子的类型</returns>
		public static CellType GetCellType(int x, int y)
		{
			if (GridSystem == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("GridSystem is not initialized");
#endif
				return CellType.Normal;
			}
			
			return GridSystem.GetCellType(x, y);
		}

		/// <summary>
		/// 逻辑坐标格子高亮（支持批量操作，具体高亮形式调用Cell的高亮显示接口）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		public static void HighlightCell(int x, int y)
		{
			GridSystem?.HighlightCell(x, y);
		}

		/// <summary>
		/// 批量高亮格子
		/// </summary>
		/// <param name="positions">要高亮的格子坐标列表</param>
		public static void HighlightCells(List<Vector2Int> positions)
		{
			GridSystem?.HighlightCells(positions);
		}

		/// <summary>
		/// 取消逻辑坐标格子高亮（支持批量操作）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		public static void CancelHighlightCell(int x, int y)
		{
			GridSystem?.CancelHighlightCell(x, y);
		}

		/// <summary>
		/// 批量取消格子高亮
		/// </summary>
		/// <param name="positions">要取消高亮的格子坐标列表</param>
		public static void CancelHighlightCells(List<Vector2Int> positions)
		{
			GridSystem?.CancelHighlightCells(positions);
		}

		/// <summary>
		/// 获取逻辑坐标格子的世界坐标（返回格子中心点的世界坐标，具体使用GridSystem调用）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>格子中心点的世界坐标</returns>
		public static Vector2 GetCellWorld(int x, int y)
		{
			if (GridSystem == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("GridSystem is not initialized");
#endif
				return Vector2.zero;
			}
			
			return GridSystem.GetCellWorldPos(x, y);
		}

		public static Vector2Int GetWorldCellPos(float worldX, float worldY)
		{
			if (GridSystem == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning("GridSystem is not initialized");
#endif
				return Vector2Int.zero;
			}

			return GridSystem.GetWorldCellPos(worldX, worldY);
		}

		public static bool IsVectory()
		{
			return GridSystem.IsVictory();
		}

		#endregion
	}

	public class ChessMoveArgs : IGameEventArgs
	{
		public Chess Chess;
		public Vector2Int From;
		public Vector2Int To;

		public ChessMoveArgs(Chess chess, Vector2Int from, Vector2Int to)
		{
			Chess = chess;
			From = from;
			To = to;
		}
		
        public void Dispose()
        {
			Chess = null;
			From = Vector2Int.zero;
			To = Vector2Int.zero;
        }
    }

}
