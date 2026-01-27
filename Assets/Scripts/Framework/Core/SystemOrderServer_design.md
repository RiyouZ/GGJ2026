# SystemOrderServer 设计文档

## 1. 类简介

- **类名**：SystemOrderServer
- **类型**：静态管理器（不应被清理）
- **作用**：负责注册系统、提供系统获取服务，并通过引用计数管理系统生存期与销毁清理。

## 2. 设计概览

- 总管理使用字典进行系统索引。
- 使用红黑树管理更新排序，以 `order` 字段排序。
- 总共三种更新条件，使用三种红黑树存储，默认为 0 的 `order` 不需要存储到更新的红黑树。
- 若 `order` 重复，按红黑树默认排序方式排序。
- 总共三种更新条件，使用三种红黑树存储。order=0 的系统不参与任何更新调度（不加入红黑树，仅可手动调用）。
- 若 `order` 重复，按红黑树默认排序方式排序。
- 多线程：暂不考虑多线程并发，所有操作假定在主线程执行。
- 错误处理：所有错误（如重复注册、未注册获取、非法操作等）均应使用 `LogError` 打印日志，并用 `UNITY_EDITOR` 宏包裹，仅在编辑器下输出。

## 3. 方法

### RegisterSystem<T>(system)

- **描述**：注册系统到服务。注册时将该实例的 `RefCount` 设为 1（表示系统自身持有一次引用），同时将 `SysID` 设置为该系统的 `Type`。
  后续通过 `GetSystem` / `GetSystemAsync` / `WaitUtilGetSystem` 获取系统会使 `RefCount` 依次累加。
  外部不允许对同一实例重复调用 `RegisterSystem`（重复注册应被拒绝并记录错误）。
- **参数**：
  - `T`：注册的接口或类型
  - `system`：注册的系统实体类

### UnRegisterSystem(system)

- **描述**：从 `SystemOrderServer` 中注销系统管理记录，将该实例从内部字典中移除。
  移除时 **不会** 直接调用 `Dispose()`。若被移除时该系统的 `RefCount` 不为 0，应记录一条 Warning 日志并输出当前 `RefCount` 值，以便排查引用未释放的问题。
- **参数**：
  - `system`：注销的类实体

### T GetSystem<T>()

- **描述**：获取对应的系统。若目标系统未注册，则返回 `null`。
- **返回值**：获取的对应实体类或 `null`

### Release(system)

- **描述**：释放对系统的引用，会将目标系统的 `RefCount` 减 1。若 `RefCount` 递减后变为 0，`SystemOrderServer` 将调用该系统的 `Dispose()` 方法并进行必要的清理。
  请注意：`Release` 只处理引用计数与可能触发 `Dispose`，不会从管理字典中主动移除系统（除非 `RefCount` 为 0 并且后续逻辑要求移除）。
- **参数**：
  - `system`：需要释放的系统

### GetSystemAsync<T>(out system)

- **描述**：异步获取系统，使用 `IEnumerator` 协程回调方式实现。
  暂不支持超时处理。
- **参数**：
  - `system`：获取到的引用

### WaitUtilGetSystem<T>(out system)

- **描述**：等待获取系统，是一个 `IEnumerator` 协程。
- **参数**：
  - `system`：获取到的引用

### UpdateSystemServer()

- **描述**：根据 `SysOrder` 调用系统的 `Update(delta)`。

### FixUpdateSystemServer()

- **描述**：根据 `SysFixOrder` 调用系统的 `FixUpdate(delta)`。

### LateUpdateSystemServer()

- **描述**：根据 `SysLateOrder` 调用系统的 `LateUpdate(delta)`。

---

# ISystemServer 接口

## 1. 类简介

- **接口名**：ISystemServer
- **类型**：接口（用于注册服务）
- **约束**：若自身引用计数不为 0，不可调用 `Dispose` 进行销毁。

## 2. 属性

- **SysID**
  - 描述：系统唯一标识。由 `SystemOrderServer` 设为该系统的 `Type`，仅用于内部查找与缓存判断。
- **SysOrder**
- **SysFixOrder**
- **SysLateOrder**
- **RefCount**
  - 描述：自身的引用计数。`RefCount` 仅允许由 `SystemOrderServer` 管理，系统内部不可自行增加或减少。

## 3. 方法

- **Update(delta)**
- **FixUpdate(delta)**
- **LateUpdate(delta)**
- **Dispose(bool waitUpdate=false)**
  - 描述：用于注册服务的接口。若自身引用计数不为 0 不可调用 `Dispose` 进行销毁。
