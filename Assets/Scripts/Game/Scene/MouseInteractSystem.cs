using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
    /// <summary>
    /// 鼠标交互系统，处理鼠标点击Tile等交互
    /// </summary>
    public class MouseInteractSystem
    {
        private Camera _camera;
        private GridSystem _gridSystem;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tilemap">场景的tilemap</param>
        /// <param name="gridSystem">网格系统</param>
        /// <param name="camera">主相机（如为null则使用Camera.main）</param>
        public MouseInteractSystem(Tilemap tilemap, GridSystem gridSystem, Camera camera = null)
        {
            _gridSystem = gridSystem;
            _camera = camera ?? Camera.main;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
#if UNITY_EDITOR
            Debug.Log("MouseInteractSystem initialized");
#endif
        }

        /// <summary>
        /// 每帧更新，检测鼠标点击
        /// </summary>
        public void Update()
        {
            // 检测鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        /// <summary>
        /// 处理鼠标点击
        /// </summary>
        private void HandleMouseClick()
        {
            // 获取鼠标在屏幕上的位置
            Vector3 mouseScreenPos = Input.mousePosition;
            
            // 转换为世界坐标
            Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);

            // 将世界坐标转换为逻辑坐标
            var cell = _gridSystem.GetWorldCell(mouseWorldPos.x, mouseWorldPos.y);

            if (cell != null)
            {
                OnCellClicked(cell, mouseWorldPos);
            }
        }

        /// <summary>
        /// 当Cell被点击时调用
        /// </summary>
        /// <param name="cell">被点击的Cell</param>
        /// <param name="worldPos">点击的世界坐标</param>
        private void OnCellClicked(Cell cell, Vector2 worldPos)
        {
#if UNITY_EDITOR
            Debug.Log($"Cell clicked: Logical({cell.X}, {cell.Y}), Type: {cell.CellType}, World({worldPos.x:F2}, {worldPos.y:F2})");
#endif

            // TODO: 在这里添加点击Cell后的逻辑
            // 例如：高亮、选中棋子、移动棋子等
            
            // 示例：高亮被点击的格子
            _gridSystem?.HighlightCell(cell.X, cell.Y);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            _gridSystem = null;
            _camera = null;
            
#if UNITY_EDITOR
            Debug.Log("MouseInteractSystem disposed");
#endif
        }
    }
}
