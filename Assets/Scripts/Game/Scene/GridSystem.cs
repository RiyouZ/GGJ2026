using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
	/// <summary>
	/// 负责管理Cell的系统
	/// 处理世界坐标与逻辑坐标的转换，以及TileMap坐标映射
	/// </summary>
	public class GridSystem
	{
		private Tilemap _tilemap;
		private Cell[,] _cells;
		private Vector2Int _cellSize;
		private Vector3Int _offset; // TileMap坐标偏移量，用于映射到正整数

		/// <summary>
		/// 构造函数，建立与修正格子的逻辑索引(左下到右上)，并将tilemap坐标映射到正整数区间
		/// </summary>
		/// <param name="tilemap">场景的tilemap</param>
		/// <param name="cellSize">场景格子的长宽</param>
		/// <param name="cellList">Cell 列表（通过 transform position 计算逻辑坐标）</param>
		public GridSystem(Tilemap tilemap, Vector2Int cellSize, List<Cell> cellList)
		{
			_tilemap = tilemap;
			_cellSize = cellSize;
			
			// 初始化格子数组
			_cells = new Cell[cellSize.x, cellSize.y];
			
			// 第一遍遍历：找到所有 Cell 的最小 TileMap 坐标作为偏移量
			Vector3Int minTilemapPos = new Vector3Int(int.MaxValue, int.MaxValue, 0);
			
			if (cellList != null)
			{
				foreach (var cell in cellList)
				{
					if (cell == null || cell.transform == null)
						continue;
					
					Vector3 worldPos = cell.transform.position;
					Vector3Int tilemapPos = _tilemap.WorldToCell(worldPos);
					
					minTilemapPos.x = Mathf.Min(minTilemapPos.x, tilemapPos.x);
					minTilemapPos.y = Mathf.Min(minTilemapPos.y, tilemapPos.y);
				}
			}
			
			// 设置偏移量为实际的最小坐标
			_offset = minTilemapPos;
			
#if UNITY_EDITOR
			Debug.Log($"GridSystem offset set to TileMap({_offset.x}, {_offset.y})");
#endif
			
			// 第二遍遍历：计算逻辑坐标并存储
			if (cellList != null)
			{
				foreach (var cell in cellList)
				{
					if (cell == null || cell.transform == null)
						continue;
					
					// 获取 Cell 的世界坐标
					Vector3 worldPos = cell.transform.position;
					
					// 转换为 TileMap 坐标
					Vector3Int tilemapPos = _tilemap.WorldToCell(worldPos);
					
					// 计算逻辑坐标（TileMap坐标 - 偏移量）
					int logicalX = tilemapPos.x - _offset.x;
					int logicalY = tilemapPos.y - _offset.y;

					if (logicalX >= 0 && logicalX < cellSize.x && logicalY >= 0 && logicalY < cellSize.y)
					{
						// 设置 cell 的逻辑坐标
						cell.X = logicalX;
						cell.Y = logicalY;
						
						// 存储到对应的逻辑坐标数组位置
						_cells[logicalX, logicalY] = cell;
						
#if UNITY_EDITOR
						Debug.Log($"Cell at World({worldPos.x:F2}, {worldPos.y:F2}) -> TileMap({tilemapPos.x}, {tilemapPos.y}) -> Logical({logicalX}, {logicalY})");
#endif
					}
#if UNITY_EDITOR
					else
					{
						Debug.LogWarning($"Cell at World({worldPos.x:F2}, {worldPos.y:F2}) -> TileMap({tilemapPos.x}, {tilemapPos.y}) maps to out-of-bounds logical position ({logicalX}, {logicalY})");
					}
#endif
				}
			}
			
#if UNITY_EDITOR
			Debug.Log($"GridSystem constructed with {cellList?.Count ?? 0} cells from provided list");
#endif
		}

		/// <summary>
		/// 初始化方法
		/// </summary>
		public void Initialize()
		{
#if UNITY_EDITOR
			Debug.Log($"GridSystem initialized with size ({_cellSize.x}, {_cellSize.y})");
#endif
		}

		/// <summary>
		/// 获取x, y 逻辑坐标中的格子
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>逻辑坐标的格子</returns>
		public Cell GetLocalCell(int x, int y)
		{
			if (x < 0 || x >= _cellSize.x || y < 0 || y >= _cellSize.y)
			{
#if UNITY_EDITOR
				Debug.LogWarning($"GetLocalCell: ({x}, {y}) is out of bounds");
#endif
				return null;
			}

			Debug.Log($"GetLocalCell: Returning cell at ({x}, {y})");
			
			return _cells[x, y];
		}

		/// <summary>
		/// 获取世界坐标中所在的格子
		/// </summary>
		/// <param name="worldX">世界坐标X</param>
		/// <param name="worldY">世界坐标Y</param>
		/// <returns>格子</returns>
		public Cell GetWorldCell(float worldX, float worldY)
		{
			Vector2 worldPos = new Vector2(worldX, worldY);
			Vector2Int cellPos = GetWorldCellPos(worldPos.x, worldPos.y);

			Debug.Log($"GetWorldCell: World({worldX:F2}, {worldY:F2}) -> Logical({cellPos.x}, {cellPos.y})");
			
			return GetLocalCell(cellPos.x, cellPos.y);
		}

		/// <summary>
		/// 获取x, y 逻辑坐标中的格子世界坐标（返回格子中心点）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		/// <returns>返回逻辑坐标格子的世界坐标</returns>
		public Vector2 GetCellWorldPos(int x, int y)
		{
			// 将逻辑坐标映射回TileMap坐标
			Vector3Int cellPos = new Vector3Int(x + _offset.x, y + _offset.y, 0);
			
			// 获取格子中心点的世界坐标
			Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);
			
			return worldPos;
		}

		/// <summary>
		/// 获取世界坐标中的格子的逻辑坐标
		/// </summary>
		/// <param name="worldX">世界坐标X</param>
		/// <param name="worldY">世界坐标Y</param>
		/// <returns>返回世界坐标格子的逻辑坐标</returns>
		public Vector2Int GetWorldCellPos(float worldX, float worldY)
		{
			Vector3 worldPos = new Vector3(worldX, worldY, 0);
			Vector3Int cellPos = _tilemap.WorldToCell(worldPos);
			
			// 考虑浮点误差，边界以最小值为主
			// 将TileMap坐标映射到逻辑坐标
			int logicalX = cellPos.x - _offset.x;
			int logicalY = cellPos.y - _offset.y;

			Debug.Log($"GetWorldCellPos: World({worldX:F2}, {worldY:F2}) -> TileMap({cellPos.x}, {cellPos.y}) -> Logical({logicalX}, {logicalY})");
			
			return new Vector2Int(logicalX, logicalY);
		}

		public CellType GetCellType(int x, int y)
		{
			Cell cell = GetLocalCell(x, y);
			if (cell != null)
			{
				return cell.CellType;
			}

			return CellType.Normal; // 默认返回Normal类型
		}

		/// <summary>
		/// 格子高亮表现（支持批量操作）
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		public void HighlightCell(int x, int y)
		{
			Cell cell = GetLocalCell(x, y);
			cell?.Highlight();
		}

		/// <summary>
		/// 格子高亮表现（批量操作）
		/// </summary>
		/// <param name="positions">要高亮的格子坐标列表</param>
		public void HighlightCells(List<Vector2Int> positions)
		{
			foreach (var pos in positions)
			{
				HighlightCell(pos.x, pos.y);
			}
		}

		/// <summary>
		/// 取消格子高亮表现
		/// </summary>
		/// <param name="x">逻辑坐标X</param>
		/// <param name="y">逻辑坐标Y</param>
		public void CancelHighlightCell(int x, int y)
		{
			Cell cell = GetLocalCell(x, y);
			cell?.CancelHighlight();
		}

		/// <summary>
		/// 取消格子高亮表现（批量操作）
		/// </summary>
		/// <param name="positions">要取消高亮的格子坐标列表</param>
		public void CancelHighlightCells(List<Vector2Int> positions)
		{
			foreach (var pos in positions)
			{
				CancelHighlightCell(pos.x, pos.y);
			}
		}

		/// <summary>
		/// 判断逻辑坐标位置是否有效（在边界内）
		/// </summary>
		/// <param name="pos">逻辑坐标</param>
		/// <returns>是否有效</returns>
		public bool IsValidPosition(Vector2Int pos)
		{
			return pos.x >= 0 && pos.x < _cellSize.x && pos.y >= 0 && pos.y < _cellSize.y;
		}

		/// <summary>
		/// 判断逻辑坐标位置是否可行走
		/// </summary>
		/// <param name="pos">逻辑坐标</param>
		/// <returns>是否可行走</returns>
		public bool CanWalk(Vector2Int pos)
		{
			if (!IsValidPosition(pos))
			{
				return false;
			}

			Cell cell = GetLocalCell(pos.x, pos.y);
			return cell?.CanWalk ?? false;
		}

		/// <summary>
		/// 销毁流程：调用所有格子的dispose方法
		/// </summary>
		public void Dispose()
		{
			for (int x = 0; x < _cellSize.x; x++)
			{
				for (int y = 0; y < _cellSize.y; y++)
				{
					_cells[x, y]?.Dispose();
				}
			}
			
			_cells = null;
			_tilemap = null;
			
#if UNITY_EDITOR
			Debug.Log("GridSystem disposed");
#endif
		}
	}
}
