
# WwiseAudio 桥接类设计文档

## 1. 类简介

- **类名**：WwiseAudio
- **类型**：静态类
- **作用**：作为业务与 Wwise 音频系统的桥接层，提供统一的音效播放、RTPC 获取与设置接口，便于全局随时调用。

## 2. 主要功能

### 2.1 播放 Event 音效

- **方法**：PlayEvent
- **参数**：
	- eventName：string，Wwise 事件名
	- gameObject：GameObject，可选，指定播放源（可为 null，表示全局/默认）
- **描述**：触发指定 Wwise Event，实现音效播放。支持 3D/2D 音效。

### 2.2 获取 RTPC 值

- **方法**：GetRTPC
- **参数**：
	- rtpcName：string，RTPC 名称
	- gameObject：GameObject，可选，指定对象
- **返回**：float，当前 RTPC 值
- **描述**：获取指定 RTPC 的当前值，支持全局与对象级 RTPC。


### 2.3 设置 RTPC 值

- **方法**：SetRTPC
- **参数**：
	- rtpcName：string，RTPC 名称
	- value：float，目标值
	- gameObject：GameObject，可选，指定对象
- **描述**：设置指定 RTPC 的值，支持全局与对象级 RTPC。

### 2.4 加载 SoundBank

- **方法**：LoadBank
- **参数**：
	- bankName：string，SoundBank 名称
	- callback：Action<bool>，可选，加载完成回调（true=成功，false=失败）
- **描述**：加载指定名称的 SoundBank，支持异步/同步加载。建议所有音频事件前确保所需 SoundBank 已加载。

## 3. 使用示例

```csharp
// 播放音效
WwiseAudio.PlayEvent("Play_Footstep", player);

// 设置 RTPC
WwiseAudio.SetRTPC("MusicIntensity", 0.8f);

// 获取 RTPC
float intensity = WwiseAudio.GetRTPC("MusicIntensity");

// 加载 SoundBank
WwiseAudio.LoadBank("Init", success => {
		if (success) Debug.Log("Init bank loaded!");
});
```

## 4. 设计要点

- 静态类，线程安全，随时可调用。
- 封装 Wwise SDK 相关调用，业务层无需关心底层实现。
- 支持 GameObject 级别与全局操作，适配 3D/2D 场景。
+- 后续可扩展：支持停止事件、回调、Switch、State、SoundBank 管理等。
+- 支持重复加载同一 SoundBank 时自动去重。
+- 支持回调通知加载结果，便于业务层做容错处理。
+- SoundBank 采用游戏启动时预加载模式，所有必要的 SoundBank 在启动阶段一次性加载，业务层无需手动调用 LoadBank。
+- 推荐在游戏启动或场景切换时预加载常用 SoundBank。

## 3. 使用示例

```csharp
// 播放音效
WwiseAudio.PlayEvent("Play_Footstep", player);

// 设置 RTPC
WwiseAudio.SetRTPC("MusicIntensity", 0.8f);

// 获取 RTPC
float intensity = WwiseAudio.GetRTPC("MusicIntensity");
```

## 4. 设计要点

- 静态类，线程安全，随时可调用。
- 封装 Wwise SDK 相关调用，业务层无需关心底层实现。
- 支持 GameObject 级别与全局操作，适配 3D/2D 场景。
- 后续可扩展：支持停止事件、回调、Switch、State 等。

## 5. 约定与注意事项

- eventName、rtpcName 必须与 Wwise 工程中保持一致。
- GameObject 参数为 null 时，操作作用于全局。
- 建议所有音频相关操作均通过本类完成，避免直接调用 Wwise API。

## 6. 初始化与依赖检查

- **描述**：在调用任何 Wwise API 前，需确保 Wwise 已正确初始化并加载所需音频数据。
- **行为建议**：
  - 若 Wwise 未初始化，进行初始化。

## 7. 异常处理与容错

- **描述**：明确对不存在的 `eventName`/`rtpcName`、`GameObject` 已销毁等情况的处理方式。
- **行为建议**：
  - 对不存在的 `eventName`/`rtpcName`：记录警告并返回（播放不执行），避免抛出致命异常影响主循环。
  - 对失效的 `GameObject`：在调用前做 null/Destroyed 检查，若已销毁则降级为全局行为或记录并忽略。

## 8. 线程安全 / 调用时机

- **描述**：说明 Wwise API 通常要求在主线程中调用，或接口是否安全跨线程。
- **行为建议**：
  - 所有 Wwise 相关调用在主线程执行（例如 Update、事件回调或 UI 线程）。
