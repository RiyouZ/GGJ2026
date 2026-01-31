using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using RuGameFramework.Event;
using Spine.Unity;
using RuGameFramework.AnimeStateMachine;
using Frame.Audio;

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
		protected Vector2Int currentPos;

		public const string ANIME_IDLE = "idle";
		public const string ANIME_MOVE = "move";

		public const string ANIME_SELECT = "select";
		public const string ANIME_SWITCH = "switch";

		private static readonly Dictionary<int, string> _maskMap = new Dictionary<int, string>()
		{
			{0, "mouse"},
			{1, "fox"}
		};	


		private SkeletonAnimation _skeletonAnimation;
		private SkeletonStateMachine  _skeletonMachine;
		
		// 属性访问器
		public virtual Faction Faction => _chessMask?.Faction ?? Faction.Neutral;
		public int Level
		{
			get
			{
				if (_chessMask.IsKing)
				{
					return _chessMask.Level + 4;
				}
				else
				{
					return _chessMask.Level;
				}
			}
		}
		public ChessMask ChessMask => _chessMask;
		public int MoveStepIndex => _moveStepIndex;
		public Vector2Int CurrentPos => currentPos;

		public Vector2Int NextPos { private set; get; }

		
		
		/// <summary>
		/// 设置面具
		/// </summary>
		public void SetMask(ChessMask mask) 
		{
			_chessMask = mask;
		}

		public void SkillGold()
		{
			
		}

		// 替换面具
		public void SwapMask(Chess newChess) 
		{
			var tempMask = this._chessMask;
			this._chessMask = newChess._chessMask;
			newChess._chessMask = tempMask;

			OnSwapMask();
			newChess.OnSwapMask();
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
		// 是否正在播放移动动画
		private bool _isMoving = false;
		public bool IsMoving => _isMoving;	

		private bool _isSelected = false;
	
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
			Vector2Int nextPos = currentPos + dir;

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
				if (ChessMask.CanAttack(target.ChessMask)) 
				{
					// 消灭对方，移动到目标格子
					Debug.Log($"[Chess] {name} 吃掉 {target.name}");
					target.Die();

					if (_chessMask.IsKing)
					{
						EventManager.InvokeEvent(MouseInteractSystem.EVENT_SKLL_SUCCESS, null);
					}

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
			_isMoving = true;
			NextPos = newPos;
			_skeletonMachine.InvokeTrigger(ANIME_MOVE);
			EventManager.InvokeEvent(GameScene.EVENT_CHESS_MOVE, new ChessMoveArgs(this, currentPos, newPos));
		}
		
		/// <summary>
		/// 死亡处理（播放动画，但不触发回合）
		/// </summary>
		public void Die() 
		{
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
			
			Vector2Int simulatedPos = currentPos;
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
		public virtual void Initialize() 
		{
			ResetMove();
			// 网格坐标
			currentPos = GameScene.GetWorldCellPos(this.transform.position.x, this.transform.position.y);

			this.transform.position = GameScene.GetCellWorld(currentPos.x, currentPos.y);

			_skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

			_skeletonMachine = new SkeletonStateMachine(_skeletonAnimation);

			_skeletonMachine.RegisterState(0, ANIME_IDLE, true);
			_skeletonMachine.RegisterState(0, ANIME_MOVE, false)
			.AddAnoAnimationEvent("move", (track, e) =>
			{
				this.transform.position = GameScene.GetCellWorld(NextPos.x, NextPos.y);
				
			})
			.OnAnimationComplate((st, track) => 
			{
				_isMoving = false;
			})
			.AddAnoAnimationEvent("move_SFX", (track, e) =>
			{
				Debug.Log("[Chess] 播放移动音效");
				// WwiseAudio.PlayEvent("Doll_Move_Prepare_Quick_SFX", this.gameObject);
			})
			.AddAnoAnimationEvent("stop_SFX", (track, e) =>
			{
				WwiseAudio.PlayEvent("Doll_Move_Land_SFX", this.gameObject);
				var nextCell = GameScene.GetCell(NextPos.x, NextPos.y);
				if (nextCell != null && nextCell.CellType == CellType.Block)
				{
					Debug.Log("[Chess] 播放撞击音效");
					// WwiseAudio.PlayEvent("Table_Chair_Bump_SFX", this.gameObject);
				}
				else if(nextCell != null && nextCell.CellType == CellType.Flag)
				{
					// WwiseAudio.PlayEvent("SFX_CapturePoint_Trigger", this.gameObject);
				}
				else
				{
					// WwiseAudio.PlayEvent("Doll_Position_Arrive_Landing_SFX", this.gameObject);
				}
				currentPos = NextPos;
			});
			
			_skeletonMachine.RegisterState(0, ANIME_SELECT, false)
			.AddAnoAnimationEvent("select_SFX", (track, e) => 
			{	
				Debug.Log("[Chess] 播放选中音效");
				// WwiseAudio.PlayEvent("Doll_Select_Highlight_SFX", this.gameObject);
			});

			_skeletonMachine.RegisterState(0, ANIME_SWITCH, false)
			.AddAnoAnimationEvent("switch", (track, e) => 
			{
				UpdateMaskSkin();
			})
			.AddAnoAnimationEvent("switch_SFX", (track, e) => 
			{
				// var sfxIndex = Random.Range(0, 2);
				// switch (sfxIndex)
				// {
				// 	case 0:
				// 		WwiseAudio.PlayEvent("Mask_Special_SFX", this.gameObject);
				// 		break;
				// }

				// WwiseAudio.PlayEvent("Doll_Mask_Switch_Effect_SFX", this.gameObject);
			});


			_skeletonMachine.AddTransition(ANIME_IDLE, ANIME_MOVE, () => _skeletonMachine.Trigger(ANIME_MOVE));
			_skeletonMachine.AddTransition(ANIME_MOVE, ANIME_IDLE, () => !_skeletonMachine.Trigger(ANIME_MOVE));

			_skeletonMachine.AddTransition(ANIME_IDLE, ANIME_SELECT, () => _isSelected);
			_skeletonMachine.AddTransition(ANIME_SELECT, ANIME_IDLE, () => !_isSelected);

			_skeletonMachine.AddTransition(ANIME_IDLE, ANIME_SWITCH, () => _skeletonMachine.Trigger(ANIME_SWITCH));
			_skeletonMachine.AddTransition(ANIME_SWITCH, ANIME_IDLE, () => !_skeletonMachine.Trigger(ANIME_SWITCH));
			
			_skeletonMachine.SetDefault(ANIME_IDLE);
			_skeletonMachine.StartMachine();
			UpdateMaskSkin();
		}

		public void OnSelectedStart()
		{
			_isSelected = true;
		}

		public void OnSelectedEnd()
		{
			_isSelected = false;
		}

        void Update()
        {
            if(_skeletonMachine != null)
			{
				_skeletonMachine.UpdateMachine();
			}
        }

		public void OnSwapMask()
		{
			_skeletonMachine.InvokeTrigger(ANIME_SWITCH);
		}
        public void UpdateMaskSkin()
		{
			if (_skeletonAnimation == null)
			{
				Debug.LogWarning($"[Chess] {name} SkeletonAnimation 组件未找到，无法更新皮肤");
				return;
			}

			if (_chessMask == null)
			{
				Debug.LogWarning($"[Chess] {name} ChessMask 未设置，无法更新皮肤");
				return;
			}

			if (_maskMap.TryGetValue(this.Level, out string skinName))
			{
				_skeletonAnimation.Skeleton.SetSkin(skinName);
				_skeletonAnimation.Skeleton.SetSlotsToSetupPose();
				_skeletonAnimation.AnimationState.Apply(_skeletonAnimation.Skeleton);
				_skeletonAnimation.Update(0);
			}
		}
	}
	
}
