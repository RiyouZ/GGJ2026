# Chess 设计文档

## 总体规则
- 枚举判断：使用枚举判断阵营、状态等（例如 `Faction`, `PieceState`）。
- 明确字段：
    - `Faction`（阵营枚举）：Friend、Enemy、Neutral
    - `level`（等级, 从面具获取）
    - `ChessMask`（面具，参考 ChessMask_design.md）
- 自己不动：棋子不会自主移动（没有自动寻路或 AI 驱动的自行移动）。
- `MoveStep` 在调用 `Move` 时逐步推进：移动由外部系统或回合控制器驱动，每次调用仅推进一小步（MoveStep）。
- `MoveStep` 仅为索引，实际移动方向由 `ChessMask.MoveRule` 决定。
- 传入的是每次移动的增量向量（增量以格子为单位，例如 `Vector2Int`），若目标越界则本步不动（并返回失败/被阻挡标识）。
- 越界处理：若下一步越界，则棋子原地不动。
- `CanWalk` 可能会发生变化，但当前实现暂不处理动态变更（仅在设计中注明，后续可扩展）。
- 按敌方和友方阵营区分：移动与判定逻辑应基于 `Faction`（阵营）进行处理。
- 死亡有动画，但不触发回合结束：死亡播放动画为表现行为；回合结束的判定以“所有棋子完成移动”为准（死亡不会额外触发回合）。
- `Previs`（预测路径）是整个路径数据，`Cell` 负责路径的可视化表现（高亮、箭头等）。
- 一帧内所有棋子进行一次移动：每一帧（或每个逻辑 Tick）驱动所有活着的棋子执行一次 `MoveStep`，其余行为暂不处理。

## 属性
- `Faction` 阵营（Friend/Enemy/Neutral）
- `Level` 等级（决定吃子判定，从面具获取）
- `ChessMask` 面具（详细参考 ChessMask_design.md）
- `MoveStepIndex` 当前移动阶段索引（用于获取下一步方向）

## 方法与接口
- `Move()`
    - **参数**: 无
    - **返回值**: bool（是否成功移动）
    - **描述**: 
        - 根据 mask 的规则移动，每次只移动一个单位格，mask 会有该轮移动的方向向量
        - 移动前检测下一步格子的内容，按以下规则：
            - 若下一步格子 CanWalk == false 或越界，则原地不动，返回 false
            - 若下一步格子 CanWalk == true：
                - 下一步格子有棋子，进行比较：
                    - 若都为友方则不可移动，返回 false
                    - 下一步格子中的棋子 level < 当前移动棋子的 level，则消灭下一步格子的棋子，自己移动到下一步格子，返回 true
                    - 下一步格子中的棋子 level >= 当前移动棋子的 level，则不可移动，返回 false
                - 下一个格子无棋子，移动到下一格，返回 true

- `ResetMove()`
    - **描述**: 重置移动标记索引

- `SetMask(mask)`
    - **参数**: ChessMask
    - **描述**: 设置 mask

- `Previs()`
    - **返回值**: List<Vector2Int> 路径点数组
    - **描述**: 鼠标指向棋子或方格时调用，返回棋子完整移动路径，并批量高亮相关 Cell

## MoveStep 结构
```csharp
public struct MoveStep {
    public int Index; // 当前阶段索引
    // 可扩展：public bool IsFinal;
}
```

## 伪代码示例
```csharp
// 移动主流程
bool Move() {
    Vector2Int dir = ChessMask.GetMoveDir(MoveStepIndex);
    Vector2Int nextPos = CurrentPos + dir;
    if (IsOutOfBounds(nextPos) || !GridSystem.CanWalk(nextPos)) return false;
    Chess target = GridSystem.GetChess(nextPos);
    if (target != null) {
        if (target.Faction == this.Faction) return false;
        if (target.mask.canattack(mask)) {
            target.Die();
            GridSystem.MoveChess(this, nextPos);
            return true;
        } else {
            return false;
        }
    } else {
        GridSystem.MoveChess(this, nextPos);
        return true;
    }
}
```

## 其他说明
- Previs 方法建议返回完整路径点数组，供 Cell 高亮。
- 越界和阻挡统一返回 false，棋子原地不动。
- 可扩展点：动态 CanWalk、死亡动画与回合同步、MoveStep 精度。

---
如需进一步补充接口签名或伪代码，请告知。





