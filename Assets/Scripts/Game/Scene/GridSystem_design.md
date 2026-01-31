## 事件机制
- 预留事件名称：
    - OnCellHighlight
    - OnCellCancelHighlight
    - OnCellTypeChange
    - 其他事件后续补充
# GridSystem 设计文档

## 描述
- 负责管理Cell的系统
- 通过TileMap 获取逻辑索引和世界坐标
- TileMap的逻辑坐标可能为负数，系统将其全部映射到正整数，映射规则为左下角为(0,0)，右上角为(cellSize.x, cellSize.y)
- 负责世界坐标与逻辑坐标的转换，转换时需考虑浮点误差，边界以最小值为主
- 使用二维数组存储格子信息

## 方法
- Ctor(tilemap, cellSize, cellList)
    - **描述**：构造函数，建立与修正格子的逻辑索引(左下到右上)，并将tilemap坐标映射到正整数区间
    - **参数**:
        - tilemap：场景的tilemap
        - cellSize：场景格子的长宽（如{ x: 宽, y: 高 }）
        - cellList：场景内的所有格子

- GetLocalCell(x, y)
    - **描述**：获取x, y 逻辑坐标中的格子
    - **返回值**: Cell 逻辑坐标的格子

- GetWorldCell(x, y)
    - **描述**: 获取世界坐标中所在的格子
    - **返回值**：Cell 格子

- GetCellWorldPos(x, y)
    - **描述**: 获取x, y 逻辑坐标中的格子世界坐标
    - **返回值**：pos 返回逻辑坐标格子的世界坐标

- GetWorldCellPos(x, y)
    - **描述**: 获取x, y 世界坐标中的格子的逻辑坐标
    - **返回值**：pos 返回世界坐标格子的逻辑坐标

- GetCellType(x, y)
    - **描述**: 获取x, y 逻辑坐标中的格子的类型（目前仅有normal类型，预留接口，后续可扩展）
    - **返回值**：CellType 返回逻辑坐标格子的类型

- HighlightCell(x, y)
    - **描述**: 格子高亮表现（Grid支持批量操作，Cell只负责自身表现）

- CancelHighlightCell(x, y)
    - **描述**: 取消格子高亮表现（Grid支持批量操作，Cell只负责自身表现）


## 初始化与销毁
- 初始化流程：初始化格子数组，建立和修正逻辑坐标，关联对应的tile物体
- 销毁流程：调用所有格子的dispose方法


# Cell 设计文档

## 描述
- 存储格子信息的类

## 枚举
- CellType（目前仅有normal类型，后续可扩展）

## 属性
- Chess
    - **描述**: 当前格子的棋子
- CanWalk
    - **描述**: 能否行走（只读属性，与CellType相关）

## 方法
- SetChess(chess)
    - **描述**: 设置棋子到格子（不会自动清除原有棋子，具体战斗逻辑不由Grid或Cell管理）
## 事件机制
- 事件与回调机制预留，暂不实现

- Highlight()
    - **描述**: 格子高亮表现

- CancelHighlight()
    - **描述**: 取消格子高亮表现