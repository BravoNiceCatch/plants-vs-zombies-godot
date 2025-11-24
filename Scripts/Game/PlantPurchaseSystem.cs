using Godot;
using Plants大战僵尸.Scripts.Plants;
using System;
using System.Collections.Generic;

namespace Plants大战僵尸.Scripts.Game
{
    /// <summary>
    /// 植物购买系统
    /// 负责处理植物的购买逻辑、阳光成本检查和与UI系统的集成
    /// </summary>
    public partial class PlantPurchaseSystem : Node
    {
        #region Events

        /// <summary>
        /// 植物选择事件
        /// </summary>
        public event Action<PlantType> OnPlantSelected;

        /// <summary>
        /// 植物购买成功事件
        /// </summary>
        public event Action<PlantType, Vector2Int> OnPlantPurchased;

        /// <summary>
        /// 植物购买失败事件
        /// </summary>
        public event Action<PlantType, string> OnPurchaseFailed;

        /// <summary>
        /// 植物取消选择事件
        /// </summary>
        public event Action OnPlantDeselected;

        #endregion

        #region Private Fields

        /// <summary>
        /// 当前选择的植物类型
        /// </summary>
        private PlantType _selectedPlant = PlantType.None;

        /// <summary>
        /// 阳光管理器引用
        /// </summary>
        private SunlightManager _sunlightManager;

        /// <summary>
        /// 游戏管理器引用
        /// </summary>
        private GameManager _gameManager;

        /// <summary>
        /// 植物冷却时间管理
        /// </summary>
        private Dictionary<PlantType, float> _plantCooldowns = new();

        /// <summary>
        /// 植物购买历史记录（用于统计分析）
        /// </summary>
        private Dictionary<PlantType, int> _purchaseHistory = new();

        #endregion

        #region Public Properties

        /// <summary>
        /// 当前选择的植物类型
        /// </summary>
        public PlantType SelectedPlant => _selectedPlant;

        /// <summary>
        /// 是否已选择植物
        /// </summary>
        public bool HasPlantSelected => _selectedPlant != PlantType.None;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();
            InitializeSystem();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            UpdateCooldowns((float)delta);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化系统
        /// </summary>
        private void InitializeSystem()
        {
            GD.Print("初始化植物购买系统");

            // 获取管理器引用
            _gameManager = GameManager.Instance;
            if (_gameManager == null)
            {
                GD.PrintErr("无法获取 GameManager 实例");
                return;
            }

            // 查找或创建阳光管理器
            _sunlightManager = _gameManager.GetNode<SunlightManager>("SunlightManager");
            if (_sunlightManager == null)
            {
                GD.Print("SunlightManager 不存在，尝试查找现有阳光系统");
                // 尝试从 GameScene 中获取阳光管理器
                var gameScene = GetTree().CurrentScene as GameScene;
                if (gameScene != null)
                {
                    // 这里需要根据实际的阳光管理实现进行调整
                    GD.Print("等待 GameScene 中的阳光管理器初始化");
                }
            }

            // 初始化冷却时间字典
            InitializeCooldowns();

            // 初始化购买历史
            InitializePurchaseHistory();

            GD.Print("植物购买系统初始化完成");
        }

        /// <summary>
        /// 初始化植物冷却时间
        /// </summary>
        private void InitializeCooldowns()
        {
            _plantCooldowns.Clear();
            
            foreach (PlantType plantType in Enum.GetValues<PlantType>())
            {
                if (plantType.IsValidPlant())
                {
                    _plantCooldowns[plantType] = 0f;
                }
            }

            GD.Print($"植物冷却时间初始化完成，包含 {_plantCooldowns.Count} 种植物");
        }

        /// <summary>
        /// 初始化购买历史记录
        /// </summary>
        private void InitializePurchaseHistory()
        {
            _purchaseHistory.Clear();
            
            foreach (PlantType plantType in Enum.GetValues<PlantType>())
            {
                if (plantType.IsValidPlant())
                {
                    _purchaseHistory[plantType] = 0;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 选择植物类型
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否选择成功</returns>
        public bool SelectPlant(PlantType plantType)
        {
            if (!IsValidPlantSelection(plantType))
            {
                return false;
            }

            var previousPlant = _selectedPlant;
            _selectedPlant = plantType;

            GD.Print($"植物已选择: {plantType.GetDisplayName()}");

            // 触发选择事件
            OnPlantSelected?.Invoke(plantType);

            return true;
        }

        /// <summary>
        /// 取消植物选择
        /// </summary>
        public void DeselectPlant()
        {
            if (_selectedPlant != PlantType.None)
            {
                GD.Print($"取消植物选择: {_selectedPlant.GetDisplayName()}");
                _selectedPlant = PlantType.None;
                OnPlantDeselected?.Invoke();
            }
        }

        /// <summary>
        /// 尝试购买植物
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>是否购买成功</returns>
        public bool TryPurchasePlant(PlantType plantType, Vector2Int gridPosition)
        {
            // 验证购买条件
            var validationResult = ValidatePurchaseConditions(plantType, gridPosition);
            if (!validationResult.IsValid)
            {
                OnPurchaseFailed?.Invoke(plantType, validationResult.ErrorMessage);
                return false;
            }

            // 扣除阳光
            if (!DeductSunlight(plantType))
            {
                OnPurchaseFailed?.Invoke(plantType, "阳光不足");
                return false;
            }

            // 记录购买
            RecordPurchase(plantType);

            // 开始冷却
            StartCooldown(plantType);

            GD.Print($"成功购买植物: {plantType.GetDisplayName()} 位置: {gridPosition}");

            // 触发购买成功事件
            OnPlantPurchased?.Invoke(plantType, gridPosition);

            return true;
        }

        /// <summary>
        /// 尝试在当前选择的位置购买植物
        /// </summary>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>是否购买成功</returns>
        public bool TryPurchaseSelectedPlant(Vector2Int gridPosition)
        {
            if (!HasPlantSelected)
            {
                OnPurchaseFailed?.Invoke(PlantType.None, "未选择任何植物");
                return false;
            }

            return TryPurchasePlant(_selectedPlant, gridPosition);
        }

        /// <summary>
        /// 检查植物是否可以购买
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否可以购买</returns>
        public bool CanPurchasePlant(PlantType plantType)
        {
            // 检查是否为有效植物
            if (!plantType.IsValidPlant())
            {
                return false;
            }

            // 检查阳光是否足够
            if (!HasEnoughSunlight(plantType))
            {
                return false;
            }

            // 检查冷却时间
            if (IsOnCooldown(plantType))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取植物的剩余冷却时间
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>剩余冷却时间（秒）</returns>
        public float GetRemainingCooldown(PlantType plantType)
        {
            return _plantCooldowns.TryGetValue(plantType, out var cooldown) ? cooldown : 0f;
        }

        /// <summary>
        /// 获取植物的购买次数
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>购买次数</returns>
        public int GetPurchaseCount(PlantType plantType)
        {
            return _purchaseHistory.TryGetValue(plantType, out var count) ? count : 0;
        }

        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        /// <returns>状态信息字符串</returns>
        public string GetSystemStatus()
        {
            var status = $"植物购买系统状态:\n";
            status += $"当前选择: {_selectedPlant.GetDisplayName()}\n";
            status += $"当前阳光: {_sunlightManager?.CurrentSunlight ?? 0}\n";
            
            foreach (var plantType in _plantCooldowns.Keys)
            {
                var cooldown = _plantCooldowns[plantType];
                var canPurchase = CanPurchasePlant(plantType);
                status += $"{plantType.GetDisplayName()}: 冷却{cooldown:F1}s, 可购买: {canPurchase}\n";
            }

            return status;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 验证植物选择是否有效
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否有效</returns>
        private bool IsValidPlantSelection(PlantType plantType)
        {
            if (!plantType.IsValidPlant())
            {
                GD.PrintErr($"无效的植物类型: {plantType}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证购买条件
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>验证结果</returns>
        private PurchaseValidationResult ValidatePurchaseConditions(PlantType plantType, Vector2Int gridPosition)
        {
            // 检查植物类型有效性
            if (!plantType.IsValidPlant())
            {
                return new PurchaseValidationResult(false, "无效的植物类型");
            }

            // 检查阳光是否足够
            if (!HasEnoughSunlight(plantType))
            {
                return new PurchaseValidationResult(false, $"阳光不足，需要 {plantType.GetSunCost()} 单位");
            }

            // 检查冷却时间
            if (IsOnCooldown(plantType))
            {
                var remainingCooldown = GetRemainingCooldown(plantType);
                return new PurchaseValidationResult(false, $"植物冷却中，还需等待 {remainingCooldown:F1} 秒");
            }

            // 检查网格位置有效性（这里需要与网格系统集成）
            if (!IsValidGridPosition(gridPosition))
            {
                return new PurchaseValidationResult(false, "无效的网格位置");
            }

            return new PurchaseValidationResult(true, string.Empty);
        }

        /// <summary>
        /// 检查是否有足够的阳光
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否有足够阳光</returns>
        private bool HasEnoughSunlight(PlantType plantType)
        {
            if (_sunlightManager == null)
            {
                GD.PrintErr("SunlightManager 未初始化");
                return false;
            }

            return _sunlightManager.CanAfford(plantType.GetSunCost());
        }

        /// <summary>
        /// 检查植物是否在冷却中
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否在冷却中</returns>
        private bool IsOnCooldown(PlantType plantType)
        {
            return _plantCooldowns.TryGetValue(plantType, out var cooldown) && cooldown > 0f;
        }

        /// <summary>
        /// 检查网格位置是否有效
        /// </summary>
        /// <param name="gridPosition">网格位置</param>
        /// <returns>是否有效</returns>
        private bool IsValidGridPosition(Vector2Int gridPosition)
        {
            // 这里需要与网格系统集成
            // 暂时返回 true，实际实现需要检查网格系统
            return gridPosition.Row >= 0 && gridPosition.Row < 5 && 
                   gridPosition.Col >= 0 && gridPosition.Col < 9;
        }

        /// <summary>
        /// 扣除阳光
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否扣除成功</returns>
        private bool DeductSunlight(PlantType plantType)
        {
            if (_sunlightManager == null)
            {
                GD.PrintErr("SunlightManager 未初始化，无法扣除阳光");
                return false;
            }

            return _sunlightManager.SpendSunlight(plantType.GetSunCost());
        }

        /// <summary>
        /// 记录购买历史
        /// </summary>
        /// <param name="plantType">植物类型</param>
        private void RecordPurchase(PlantType plantType)
        {
            if (_purchaseHistory.ContainsKey(plantType))
            {
                _purchaseHistory[plantType]++;
            }
            else
            {
                _purchaseHistory[plantType] = 1;
            }

            GD.Print($"植物 {plantType.GetDisplayName()} 已购买 {_purchaseHistory[plantType]} 次");
        }

        /// <summary>
        /// 开始植物冷却
        /// </summary>
        /// <param name="plantType">植物类型</param>
        private void StartCooldown(PlantType plantType)
        {
            var cooldownTime = plantType.GetCooldownTime();
            _plantCooldowns[plantType] = cooldownTime;
            
            GD.Print($"植物 {plantType.GetDisplayName()} 开始冷却，时间: {cooldownTime} 秒");
        }

        /// <summary>
        /// 更新冷却时间
        /// </summary>
        /// <param name="delta">时间增量</param>
        private void UpdateCooldowns(float delta)
        {
            var plantsToRefresh = new List<PlantType>();

            foreach (var kvp in _plantCooldowns)
            {
                if (kvp.Value > 0f)
                {
                    var newCooldown = Mathf.Max(0f, kvp.Value - delta);
                    _plantCooldowns[kvp.Key] = newCooldown;

                    // 如果冷却完成，记录需要刷新的植物
                    if (newCooldown == 0f && kvp.Value > 0f)
                    {
                        plantsToRefresh.Add(kvp.Key);
                    }
                }
            }

            // 通知冷却完成的植物
            foreach (var plantType in plantsToRefresh)
            {
                GD.Print($"植物 {plantType.GetDisplayName()} 冷却完成");
                // 这里可以添加冷却完成的事件通知
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 购买验证结果
        /// </summary>
        private struct PurchaseValidationResult
        {
            public bool IsValid { get; }
            public string ErrorMessage { get; }

            public PurchaseValidationResult(bool isValid, string errorMessage)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }

        #endregion
    }

    /// <summary>
    /// 网格位置结构
    /// </summary>
    public struct Vector2Int
    {
        public int Row;
        public int Col;

        public Vector2Int(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }
    }
}
