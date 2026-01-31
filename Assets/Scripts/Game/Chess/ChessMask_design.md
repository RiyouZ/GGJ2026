# ChessMask 设计文档

## 描述
- 持有MoveRule(移动向量)的SO
- 持有level值
- 能够被鼠标选中
- 面具可以有多个方向的MoveRule进行选择

## 方法
- CanAttack(mask)
    - **描述**：根据level值判断能否攻击
- GetMoveDir(MoveStep)
    - **描述**：获取下次移动方向
- ChangeMoveDir()
    - **描述**：选择下一个方向MoveRule

# MoveRuleSO 设计文档
## 描述
- 移动的规则配置

- List<Vector2Int>
    - **描述**：移动规则



