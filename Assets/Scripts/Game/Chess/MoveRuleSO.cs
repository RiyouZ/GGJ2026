

using System.Collections.Generic;
using UnityEngine;

namespace Game.GameChess
{
    [CreateAssetMenu(menuName = "Custom/MoveRule")]
    public class MoveRuleSO : ScriptableObject
    {
        [Header("移动方向列表")]
        [SerializeField]
        private List<Vector2Int> moveDirections = new List<Vector2Int>();

        public int GetMoveMaxCount() => moveDirections.Count;

        /// <summary>
        /// 根据移动阶段索引获取对应的移动方向
        /// </summary>
        public Vector2Int GetMoveDirection(int moveStepIndex)
        {
            if (moveStepIndex < 0 || moveStepIndex >= moveDirections.Count)
            {
                Debug.LogWarning($"[MoveRuleSO] 索引 {moveStepIndex} 超出范围，返回 Vector2Int.zero");
                return Vector2Int.zero;
            }

            return moveDirections[moveStepIndex];
        }
    }
}
