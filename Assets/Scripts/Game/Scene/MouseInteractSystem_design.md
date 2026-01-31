
# MouseInteractSystem 设计文档

## 描述
- 鼠标点击互动系统
- 鼠标左键点击角色选择 再次点击其他角色交换面具
- 鼠标右键点击角色旋转移动方向（调用chessmask.ChangeMoveDir()）
- 悬浮到角色身上进行cell预演高亮

## 方法

- HandleMouseLeftClick()
	- 使用 Physics2D 检测角色的碰撞（2D碰撞体）。
	- 若点击的是已选择的棋子，则取消选择。
	- 点击非棋子部分也取消选择。
	- 只能与友方棋子交换面具（友方判定以 Chess 的字段或方法为准，如 tag、faction 或 IsFriendlyWith）。

- HandleMouseRightClick()
	- 鼠标右键点击处理。
	- 点击棋子时，调用其 ChessMask.ChangeMoveDir() 方法。
	- 旋转和面向的所有规则以 ChessMask.cs 为准，表现面向只有左右，通过修改scale.x。

- HandleMouseHover()
	- 鼠标悬浮检测。
	- 鼠标悬浮到角色身上时，调用高亮显示。
	- 高亮直接调用 GameScene 的 HighlightCells。

## 交互与权限控制
- 当前回合不是玩家行动时不可交互，需参考 TurnSystem 的 CanPlayerAct 或类似接口。
- 仅面向鼠标输入，不考虑触屏等其他输入方式。
- 所有交互权限、友方判定、输入检测等均在方法内部实现，无需外部判断。

## 其他说明
- 旋转、面具交换等具体规则和动画反馈以 ChessMask.cs 及 Chess 相关实现为准。
- 暂不支持多选、批量操作、撤销等扩展功能。
