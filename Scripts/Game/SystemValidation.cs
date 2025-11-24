using Godot;
using Plants大战僵尸.Scripts.Plants;
using PlantsVsZombies.Core;
using System;

namespace Plants大战僵尸.Scripts.Game
{
    /// <summary>
    /// 系统验证脚本
    /// 用于验证植物购买和阳光系统的基本功能
    /// </summary>
    public partial class SystemValidation : Node
    {
        public override void _Ready()
        {
            GD.Print("=== 植物购买系统验证开始 ===");
            
            ValidatePlantTypes();
            ValidateSystemIntegration();
            
            GD.Print("=== 植物购买系统验证完成 ===");
        }

        /// <summary>
        /// 验证植物类型定义
        /// </summary>
        private void ValidatePlantTypes()
        {
            GD.Print("\n--- 验证植物类型定义 ---");

            // 检查所有植物类型
            foreach (PlantType plantType in Enum.GetValues<PlantType>())
            {
                if (plantType.IsValidPlant())
                {
                    var cost = plantType.GetSunCost();
                    var name = plantType.GetDisplayName();
                    var cooldown = plantType.GetCooldownTime();
                    
                    GD.Print($"✓ {name}: 阳光成本={cost}, 冷却时间={cooldown}s");
                    
                    // 验证数据合理性
                    if (cost <= 0 || cost > 999)
                    {
                        GD.PrintErr($"✗ {name}: 阳光成本异常 {cost}");
                    }
                    
                    if (cooldown <= 0 || cooldown > 60)
                    {
                        GD.PrintErr($"✗ {name}: 冷却时间异常 {cooldown}s");
                    }
                }
            }
        }

        /// <summary>
        /// 验证系统集成
        /// </summary>
        private void ValidateSystemIntegration()
        {
            GD.Print("\n--- 验证系统集成 ---");

            try
            {
                var gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    GD.PrintErr("✗ GameManager 不存在");
                    return;
                }
                GD.Print("✓ GameManager 存在");

                var sunlightManager = gameManager.SunlightManager;
                if (sunlightManager == null)
                {
                    GD.PrintErr("✗ SunlightManager 不存在");
                }
                else
                {
                    GD.Print("✓ SunlightManager 存在");
                    GD.Print($"  当前阳光: {sunlightManager.CurrentSunlight}");
                }

                var purchaseSystem = gameManager.PlantPurchaseSystem;
                if (purchaseSystem == null)
                {
                    GD.PrintErr("✗ PlantPurchaseSystem 不存在");
                }
                else
                {
                    GD.Print("✓ PlantPurchaseSystem 存在");
                    GD.Print($"  当前选择: {purchaseSystem.SelectedPlant}");
                    GD.Print($"  是否有选择: {purchaseSystem.HasPlantSelected}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"✗ 系统验证失败: {ex.Message}");
            }
        }
    }
}
