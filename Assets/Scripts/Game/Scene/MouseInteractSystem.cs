using System;
using System.Collections.Generic;
using System.Drawing;
using Frame.Audio;
using Game.GameChess;
using Game.Scene;
using JetBrains.Annotations;
using RuGameFramework.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
	/// <summary>
	/// 鼠标交互系统 - 处理选择、交换面具、旋转、悬浮高亮
	/// </summary>
	public class MouseInteractSystem
	{
		public const string EVENT_SKLL_SUCCESS = "SkillSuccess";
		private Camera _camera;
		private GridSystem _gridSystem;
		private TurnSystem _turnSystem;

		// 当前选中的棋子
		private Chess _selectedChess;
		
		// 当前悬浮的棋子（用于高亮预演）
		private Chess _hoveredChess;
		
		// 当前高亮的格子列表
		private List<Vector2Int> _currentHighlightedCells = new List<Vector2Int>();
		
		// 技能状态
		private bool _isSkillActive = false;
		public bool IsSkillActive => _isSkillActive;

		private int _skillCount = 1;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="tilemap">场景的tilemap</param>
		/// <param name="gridSystem">网格系统</param>
		/// <param name="turnSystem">回合系统（用于检查玩家操作权限）</param>
		/// <param name="camera">主相机（如为null则使用Camera.main）</param>
		public MouseInteractSystem(Tilemap tilemap, GridSystem gridSystem, TurnSystem turnSystem = null, Camera camera = null)
		{
			_gridSystem = gridSystem;
			_turnSystem = turnSystem;
			_camera = camera ?? Camera.main;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		public void Initialize()
		{
			EventManager.AddListener(GameScene.EVENT_SCENE_PLAYER_SKILL, (args) => ToggleSkill());
			EventManager.AddListener(EVENT_SKLL_SUCCESS, (args) => OnSkillSuccesss());
		}

		/// <summary>
		/// 每帧更新，检测鼠标交互
		/// </summary>
		public void Update()
		{
			// 检查当前回合是否允许玩家操作
			if (_turnSystem != null && !_turnSystem.CanPlayerAct)
			{
				return;
			}

			// TODO : 热键测试

			if(Input.GetKeyDown(KeyCode.Space))
			{
				GameScene.TurnSystem.PlayerActionComplete();
			}
			
			// Q键：给选中的棋子赋予/取消技能
			if(Input.GetKeyDown(KeyCode.Q))
			{
				ToggleSkill();
			}

			HandleMouseHover();
			HandleMouseLeftClick();
			HandleMouseRightClick();
		}

		/// <summary>
		/// 处理鼠标左键点击 - 选择棋子或交换面具
		/// </summary>
		private void HandleMouseLeftClick()
		{
			if (!Input.GetMouseButtonDown(0)) return;

			Vector2 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
			Chess clickedChess = GetChessAtPosition(worldPos);

			// 点击空白区域或非棋子
			if (clickedChess == null)
			{
				DeselectChess();
				return;
			}

			// 点击已选中的棋子 -> 取消选择
			if (_selectedChess == clickedChess)
			{
				DeselectChess();
				return;
			}

			// 如果不是友方 无法选中
			if(clickedChess.Faction != Faction.Friend)
			{
				return;
			}

			// 如果没有选中棋子，直接选中
			if (_selectedChess == null)
			{
				SelectChess(clickedChess);
				return;
			}

			// 有选中棋子，尝试交换面具（仅友方）
			if (IsFriendly(_selectedChess, clickedChess))
			{
				SwapMasks(_selectedChess, clickedChess);

				// 如果当前悬浮的棋子是交换的任意一方，刷新高亮
				if (_hoveredChess == _selectedChess || _hoveredChess == clickedChess)
				{
					ClearHighlight();
					ShowPreviewHighlight(_hoveredChess);
				}

				DeselectChess();
			}
			else
			{
				// 非友方，切换选择
				SelectChess(clickedChess);
			}
		}

		/// <summary>
		/// 处理鼠标右键点击 - 旋转棋子方向
		/// </summary>
		private void HandleMouseRightClick()
		{
			if (!Input.GetMouseButtonDown(1)) return;

			Vector2 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
			Chess clickedChess = GetChessAtPosition(worldPos);

			if (clickedChess != null)
			{
				if (clickedChess.Faction != Faction.Friend)
				{
					return;
				}
				
				var chessMask = clickedChess.ChessMask;
				if (chessMask != null)
				{
					chessMask.ChangeMoveDir();
					
					// 如果是当前悬浮的棋子，刷新高亮
					if (clickedChess == _hoveredChess)
					{
						ClearHighlight();
						ShowPreviewHighlight(clickedChess);
					}
				}
			}
		}

		/// <summary>
		/// 处理鼠标悬浮 - 显示移动预览高亮
		/// </summary>
		private void HandleMouseHover()
		{
			Vector2 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
			Chess hoveredChess = GetChessAtPosition(worldPos);
			if (hoveredChess == null)
			{
				// 清除高亮
				ClearHighlight();
				_hoveredChess = null;
				return;
			}

			if (hoveredChess.Faction != Faction.Friend)
			{
				return;
			}
			
			// 如果悬浮目标改变
			if (hoveredChess != _hoveredChess)
			{
				
				_hoveredChess = hoveredChess;
				
				// 清除之前的高亮
				ClearHighlight();

				// 如果悬浮在棋子上，显示预演高亮
				if (_hoveredChess != null)
				{
					ShowPreviewHighlight(_hoveredChess);
				}
			}
		}

		/// <summary>
		/// 使用 Physics2D 获取指定位置的棋子
		/// </summary>
		private Chess GetChessAtPosition(Vector2 worldPos)
		{
			Collider2D hit = Physics2D.OverlapPoint(worldPos);
			
			if (hit == null) return null;

			var chess = hit.GetComponentInParent<Chess>();
			if (chess != null)
			{
				return chess;
			}

			return null;
		}

		/// <summary>
		/// 选择棋子
		/// </summary>
		private void SelectChess(Chess chess)
		{
			_selectedChess = chess;
			_selectedChess.OnSelectedStart();
		}

		/// <summary>
		/// 取消选择
		/// </summary>
		private void DeselectChess()
		{
			if(_selectedChess != null)
			{
				_selectedChess.OnSelectedEnd();
			}

			_selectedChess = null;
		}

		/// <summary>
		/// 交换两个棋子的面具
		/// </summary>
		private void SwapMasks(Chess chessA, Chess chessB)
		{
			if (chessA != null && chessB != null)
			{
				chessA.SwapMask(chessB);
			}
		}

		/// <summary>
		/// 判断两个棋子是否为友方
		/// </summary>
		private bool IsFriendly(Chess chessA, Chess chessB)
		{
			if (chessA == null || chessB == null) return false;

			// 优先使用 IsFriendlyWith 方法

				return chessB.Faction == Faction.Friend && chessA.Faction == chessB.Faction;

		}

		/// <summary>
		/// 显示棋子移动预演高亮
		/// </summary>
		private void ShowPreviewHighlight(Chess chess)
		{
			if (chess == null) return;

			// 获取预演的格子列表
			var previewCells = chess.Previs();

			if (previewCells != null && previewCells.Count > 0)
			{
				_currentHighlightedCells = previewCells;
				var endCell = _gridSystem.GetLocalCell(previewCells[previewCells.Count - 1].x, previewCells[previewCells.Count - 1].y);
				GameScene.guideLine.DrawLine(chess.guidePoint, endCell.transform);
				GameScene.HighlightCells(previewCells);
			}
		}

		/// <summary>
		/// 清除当前高亮
		/// </summary>
		public void ClearHighlight()
		{
			if (_currentHighlightedCells.Count > 0)
			{
				GameScene.guideLine.ClearLine();
				GameScene.CancelHighlightCells(_currentHighlightedCells);
				_currentHighlightedCells.Clear();
			}
		}
		
		/// <summary>
		/// 切换技能状态
		/// </summary>
		private void ToggleSkill()
		{
			if (_selectedChess == null) return;

			_isSkillActive = !_isSkillActive;
			
			if (_isSkillActive)
			{
				ApplySkillToChess(_selectedChess);
			}
			else
			{
				CancelSkillToChess(_selectedChess);
			}
		}
		
		/// <summary>
		/// 对棋子应用技能（增加 level）
		/// </summary>
		private void ApplySkillToChess(Chess chess)
		{
			if (chess == null || chess.ChessMask == null) return;
			
			chess.ChessMask.IsKing = true;
			chess.EquipHat();
			chess.UpdateMaskSkin(); // 更新皮肤显示
			
			WwiseAudio.PlayEvent("Play_Doll_Skill_Cast_SFX", chess.gameObject);
		}

		private void CancelSkillToChess(Chess chess)
		{
			if (chess == null || chess.ChessMask == null) return;
			
			chess.ChessMask.IsKing = false;
			chess.UnequipHat();
			chess.UpdateMaskSkin(); // 更新皮肤显示
		}

		private void OnSkillSuccesss()
		{
			_skillCount--;
			_isSkillActive = false;
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public void Dispose()
		{
			ClearHighlight();
			_selectedChess = null;
			_hoveredChess = null;
			_gridSystem = null;
			_turnSystem = null;
			_camera = null;
		}
	}
}
