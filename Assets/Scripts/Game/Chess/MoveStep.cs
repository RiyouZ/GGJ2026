using UnityEngine;

namespace Game.GameChess {
    [System.Serializable]
    public struct MoveStep {
        public int Index; // 当前阶段索引，用于从 MoveRule 获取移动方向
        
        // 可扩展：是否为路径最后一步
        // public bool IsFinal;
    }
}
