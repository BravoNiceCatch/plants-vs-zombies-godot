using Godot;

namespace Plants大战僵尸.Scripts.Plants
{
    /// <summary>
    /// 植物类型枚举
    /// 定义游戏中所有可用的植物类型
    /// </summary>
    public enum PlantType
    {
        /// <summary>
        /// 无植物/空值
        /// </summary>
        None = 0,

        /// <summary>
        /// 太阳花 - 生产阳光
        /// </summary>
        Sunflower = 1,

        /// <summary>
        /// 豌豆射手 - 发射豌豆攻击僵尸
        /// </summary>
        Peashooter = 2,

        /// <summary>
        /// 樱桃炸弹 - 爆炸型植物，造成范围伤害
        /// </summary>
        CherryBomb = 3
    }

    /// <summary>
    /// 植物类型扩展方法
    /// 提供植物类型相关的实用功能
    /// </summary>
    public static class PlantTypeExtensions
    {
        /// <summary>
        /// 获取植物的阳光成本
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>阳光成本</returns>
        public static int GetSunCost(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Sunflower => 50,
                PlantType.Peashooter => 100,
                PlantType.CherryBomb => 150,
                _ => 0
            };
        }

        /// <summary>
        /// 获取植物的显示名称
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.None => "无",
                PlantType.Sunflower => "太阳花",
                PlantType.Peashooter => "豌豆射手",
                PlantType.CherryBomb => "樱桃炸弹",
                _ => "未知植物"
            };
        }

        /// <summary>
        /// 获取植物的描述信息
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>植物描述</returns>
        public static string GetDescription(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.None => "空植物",
                PlantType.Sunflower => "生产阳光的植物，每5秒产生25单位阳光",
                PlantType.Peashooter => "基础攻击植物，发射豌豆攻击僵尸",
                PlantType.CherryBomb => "一次性爆炸植物，造成大范围伤害",
                _ => "未知植物类型"
            };
        }

        /// <summary>
        /// 获取植物的冷却时间（秒）
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>冷却时间</returns>
        public static float GetCooldownTime(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Sunflower => 5.0f,
                PlantType.Peashooter => 7.5f,
                PlantType.CherryBomb => 20.0f,
                _ => 0f
            };
        }

        /// <summary>
        /// 检查是否为有效植物类型
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>是否有效</returns>
        public static bool IsValidPlant(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Sunflower => true,
                PlantType.Peashooter => true,
                PlantType.CherryBomb => true,
                _ => false
            };
        }

        /// <summary>
        /// 获取植物的主要颜色（用于UI显示）
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>主要颜色</returns>
        public static Color GetPrimaryColor(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Sunflower => new Color(1.0f, 0.8f, 0.0f), // 金黄色
                PlantType.Peashooter => new Color(0.2f, 0.8f, 0.2f), // 绿色
                PlantType.CherryBomb => new Color(1.0f, 0.2f, 0.2f), // 红色
                _ => Colors.Gray
            };
        }

        /// <summary>
        /// 获取植物的次要颜色（用于UI显示）
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <returns>次要颜色</returns>
        public static Color GetSecondaryColor(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Sunflower => new Color(1.0f, 1.0f, 0.0f), // 亮黄色
                PlantType.Peashooter => new Color(0.5f, 1.0f, 0.5f), // 浅绿色
                PlantType.CherryBomb => new Color(1.0f, 0.5f, 0.5f), // 浅红色
                _ => Colors.LightGray
            };
        }
    }
}
