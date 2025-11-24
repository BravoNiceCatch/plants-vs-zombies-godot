using Godot;
using Plants大战僵尸.Scripts.Plants;
using Plants大战僵尸.Scripts.Game;
using PlantsVsZombies.Core;
using System;

namespace Plants大战僵尸.Scripts.Game
{
    /// <summary>
    /// 植物购买系统测试
    /// 用于验证植物购买系统的功能
    /// </summary>
    public partial class PlantPurchaseSystemTest : Node
    {
        #region Private Fields
        private PlantPurchaseSystem _purchaseSystem;
        private SunlightManager _sunlightManager;
        private GameManager _gameManager;
        #endregion

        #region Godot Lifecycle
        public override void _Ready()
        {
            base._Ready();
            
            GD.Print("=== 开始植物购买系统测试 ===");
            
            // 延迟执行测试，确保所有系统都已初始化
            CallDeferred(nameof(RunTests));
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// 运行所有测试
        /// </summary>
        private void RunTests()
        {
            try
            {
                InitializeTestReferences();
                TestPlantTypeExtensions();
                TestSunlightManager();
                TestPlantPurchaseSystem();
                TestIntegration();
                
                GD.Print("=== 植物购买系统测试完成 ===");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"植物购买系统测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化测试引用
        /// </summary>
        private void InitializeTestReferences()
        {
            _gameManager = GameManager.Instance;
            if (_gameManager == null)
            {
                throw new Exception("无法获取 GameManager 实例");
            }

            _sunlightManager = _gameManager.SunlightManager;
            if (_sunlightManager == null)
            {
                throw new Exception("无法获取 SunlightManager 实例");
            }

            _purchaseSystem = _gameManager.PlantPurchaseSystem;
            if (_purchaseSystem == null)
            {
                throw new Exception("无法获取 PlantPurchaseSystem 实例");
            }

            GD.Print("✓ 测试引用初始化成功");
        }

        /// <summary>
        /// 测试植物类型扩展方法
        /// </summary>
        private void TestPlantTypeExtensions()
        {
            GD.Print("\n--- 测试植物类型扩展方法 ---");

            // 测试阳光成本
            GD.Print($"太阳花阳光成本: {PlantType.Sunflower.GetSunCost()} (期望: 50)");
            GD.Print($"豌豆射手阳光成本: {PlantType.Peashooter.GetSunCost()} (期望: 100)");
            GD.Print($"樱桃炸弹阳光成本: {PlantType.CherryBomb.GetSunCost()} (期望: 150)");

            // 测试显示名称
            GD.Print($"太阳花显示名称: {PlantType.Sunflower.GetDisplayName()} (期望: 太阳花)");
            GD.Print($"豌豆射手显示名称: {PlantType.Peashooter.GetDisplayName()} (期望: 豌豆射手)");
            GD.Print($"樱桃炸弹显示名称: {PlantType.CherryBomb.GetDisplayName()} (期望: 樱桃炸弹)");

            // 测试描述
            GD.Print($"太阳花描述: {PlantType.Sunflower.GetDescription()}");
            GD.Print($"豌豆射手描述: {PlantType.Peashooter.GetDescription()}");
            GD.Print($"樱桃炸弹描述: {PlantType.CherryBomb.GetDescription()}");

            // 测试冷却时间
            GD.Print($"太阳花冷却时间: {PlantType.Sunflower.GetCooldownTime()}s (期望: 5s)");
            GD.Print($"豌豆射手冷却时间: {PlantType.Peashooter.GetCooldownTime()}s (期望: 7.5s)");
            GD.Print($"樱桃炸弹冷却时间: {PlantType.CherryBomb.GetCooldownTime()}s (期望: 20s)");

            // 测试有效性检查
            GD.Print($"太阳花有效性: {PlantType.Sunflower.IsValidPlant()} (期望: true)");
            GD.Print($"None 有效性: {PlantType.None.IsValidPlant()} (期望: false)");

            GD.Print("✓ 植物类型扩展方法测试完成");
        }

        /// <summary>
        /// 测试阳光管理器
        /// </summary>
        private void TestSunlightManager()
        {
            GD.Print("\n--- 测试阳光管理器 ---");

            var initialSunlight = _sunlightManager.CurrentSunlight;
            GD.Print($"初始阳光数量: {initialSunlight}");

            // 测试添加阳光
            _sunlightManager.AddSunlight(25);
            GD.Print($"添加25阳光后: {_sunlightManager.CurrentSunlight} (期望: {initialSunlight + 25})");

            // 测试阳光充足性检查
            GD.Print($"是否能负担50阳光: {_sunlightManager.CanAfford(50)}");
            GD.Print($"是否能负担999阳光: {_sunlightManager.CanAfford(999)}");

            // 测试花费阳光
            var beforeSpend = _sunlightManager.CurrentSunlight;
            var spendSuccess = _sunlightManager.SpendSunlight(50);
            GD.Print($"花费50阳光成功: {spendSuccess}, 当前阳光: {_sunlightManager.CurrentSunlight} (期望: {beforeSpend - 50})");

            // 测试过度花费
            var beforeOverspend = _sunlightManager.CurrentSunlight;
            var overspendSuccess = _sunlightManager.SpendSunlight(9999);
            GD.Print($"过度花费阳光成功: {overspendSuccess} (期望: false), 当前阳光: {_sunlightManager.CurrentSunlight} (期望: {beforeOverspend})");

            // 测试最大值限制
            _sunlightManager.AddSunlight(9999);
            GD.Print($"添加大量阳光后: {_sunlightManager.CurrentSunlight} (期望: ≤ 999)");

            GD.Print("✓ 阳光管理器测试完成");
        }

        /// <summary>
        /// 测试植物购买系统
        /// </summary>
        private void TestPlantPurchaseSystem()
        {
            GD.Print("\n--- 测试植物购买系统 ---");

            // 测试植物选择
            _purchaseSystem.SelectPlant(PlantType.Sunflower);
            GD.Print($"选择太阳花后当前选择: {_purchaseSystem.SelectedPlant} (期望: Sunflower)");
            GD.Print($"是否有植物选择: {_purchaseSystem.HasPlantSelected} (期望: true)");

            // 测试取消选择
            _purchaseSystem.DeselectPlant();
            GD.Print($"取消选择后当前选择: {_purchaseSystem.SelectedPlant} (期望: None)");
            GD.Print($"是否有植物选择: {_purchaseSystem.HasPlantSelected} (期望: false)");

            // 测试购买条件检查
            _sunlightManager.Reset(); // 重置阳光为初始值
            GD.Print($"重置后阳光数量: {_sunlightManager.CurrentSunlight}");

            // 测试可购买性检查
            GD.Print($"太阳花可购买性: {_purchaseSystem.CanPurchasePlant(PlantType.Sunflower)}");
            GD.Print($"樱桃炸弹可购买性: {_purchaseSystem.CanPurchasePlant(PlantType.CherryBomb)}");

            // 测试购买成功
            var testPosition = new Vector2Int(2, 3);
            _purchaseSystem.SelectPlant(PlantType.Sunflower);
            var purchaseSuccess = _purchaseSystem.TryPurchasePlant(PlantType.Sunflower, testPosition);
            GD.Print($"购买太阳花成功: {purchaseSuccess}");

            if (purchaseSuccess)
            {
                GD.Print($"购买后阳光数量: {_sunlightManager.CurrentSunlight}");
                GD.Print($"太阳花购买次数: {_purchaseSystem.GetPurchaseCount(PlantType.Sunflower)}");
            }

            // 测试冷却
            GD.Print($"太阳花剩余冷却: {_purchaseSystem.GetRemainingCooldown(PlantType.Sunflower)}s");

            GD.Print("✓ 植物购买系统测试完成");
        }

        /// <summary>
        /// 测试系统集成
        /// </summary>
        private void TestIntegration()
        {
            GD.Print("\n--- 测试系统集成 ---");

            // 测试事件触发
            bool plantSelectedFired = false;
            bool plantPurchasedFired = false;
            bool sunlightChangedFired = false;

            _purchaseSystem.OnPlantSelected += (plantType) => {
                plantSelectedFired = true;
                GD.Print($"✓ 植物选择事件触发: {plantType.GetDisplayName()}");
            };

            _purchaseSystem.OnPlantPurchased += (plantType, position) => {
                plantPurchasedFired = true;
                GD.Print($"✓ 植物购买事件触发: {plantType.GetDisplayName()} 在 {position}");
            };

            _sunlightManager.OnSunlightChanged += (newAmount) => {
                sunlightChangedFired = true;
                GD.Print($"✓ 阳光变化事件触发: {newAmount}");
            };

            // 触发事件
            _purchaseSystem.SelectPlant(PlantType.Peashooter);
            _sunlightManager.AddSunlight(100);

            // 验证事件触发
            GD.Print($"植物选择事件是否触发: {plantSelectedFired}");
            GD.Print($"植物购买事件是否触发: {plantPurchasedFired}");
            GD.Print($"阳光变化事件是否触发: {sunlightChangedFired}");

            // 测试系统状态
            GD.Print("\n=== 系统状态 ===");
            GD.Print(_gameManager.GetSystemStatus());

            GD.Print("✓ 系统集成测试完成");
        }
        #endregion
    }
}
