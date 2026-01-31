# GameScene 设计文档

## 描述
- 初始化游戏场景
- 管理游戏场景内所有格子和棋子（每个格子只能有一个棋子，不允许叠加）
- 管理游戏场景内格子
- 所有格子管理相关采用GridSystem调用（具体类型见GridSystem_design）

## 属性
- public static GridSystem 
- public static TurnSystem（具体接口和职责后续补充，详见回合系统相关文档）
- public static MouseInteractSystem（具体接口和职责后续补充）

## 方法
// Chess 相关文档将与回合系统一同补充
- GetChess(x, y)
	- **描述**：获取x, y 逻辑坐标中的棋子（一个格子只能有一个棋子）
	- **返回值**: Chess 所在格子的棋子类

- GetCell(x, y)
	- **描述**: 获取x, y 逻辑坐标中的格子
	- **返回值**：Cell 返回逻辑坐标格子

- GetCellType(x, y)
	- **描述**: 获取x, y 逻辑坐标中的格子的类型（类型定义见GridSystem_design）
	- **返回值**：CellType 返回逻辑坐标格子的类型

- HighlightCell(x, y)
	- **描述**: 逻辑坐标格子高亮（支持批量操作，具体高亮形式调用Cell的高亮显示接口）

- CancelHighlightCell(x, y)
	- **描述**: 取消逻辑坐标格子高亮（支持批量操作）

- GetCellWorld(x, y)
	- **描述**：获取逻辑坐标格子的世界坐标（返回格子中心点的世界坐标，具体使用GridSystem调用）

## 初始化与销毁
- 该场景类继承MonoBehaviour
- 初始化流程：在Start方法中调用场景中所有系统的Initialize方法（具体依赖关系详见project文档架构）
- 销毁流程：在销毁时调用所有系统的Dispose方法

## 事件机制
- 预留事件名称：
	- OnChessMove
	- OnCellStateChange
	- OnTurnStart
	- OnTurnEnd
	- 其他事件后续补充

## 其他说明
- 并发与多玩家暂不考虑
- GridSystem接口和职责后续补充

