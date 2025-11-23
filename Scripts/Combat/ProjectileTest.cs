using Godot;

namespace PlantsVsZombies.Scripts.Combat
{
    /// <summary>
    /// 豌豆子弹测试脚本 - 用于测试豌豆发射和碰撞检测
    /// </summary>
    public partial class ProjectileTest : Node2D
    {
        private Timer _peaSpawnTimer;
        private int _peaCount = 0;
        private const int MAX_PEAS = 5;

        public override void _Ready()
        {
            GD.Print("[ProjectileTest] 开始豌豆子弹测试");

            // 创建豌豆生成计时器
            _peaSpawnTimer = new Timer();
            _peaSpawnTimer.WaitTime = 2.0f; // 每2秒发射一个豌豆
            _peaSpawnTimer.Timeout += OnSpawnPea;
            _peaSpawnTimer.Autostart = true;
            AddChild(_peaSpawnTimer);

            // 添加测试说明到屏幕
            CreateTestInstructions();
        }

        private void CreateTestInstructions()
        {
            var instructions = new Label();
            instructions.Text = @"
豌豆子弹测试:
- 自动每2秒发射一个豌豆
- 豌豆沿直线飞行
- 按ESC键暂停游戏
- 按Space键手动发射豌豆
";
            instructions.Position = new Vector2(50, 200);
            instructions.Modulate = Colors.White;
            AddChild(instructions);
        }

        private void OnSpawnPea()
        {
            if (_peaCount < MAX_PEAS)
            {
                CreateTestPea();
                _peaCount++;
            }
            else
            {
                _peaSpawnTimer.Stop();
                GD.Print("[ProjectileTest] 测试完成，已发射5个豌豆");
            }
        }

        private void CreateTestPea()
        {
            // 加载豌豆场景
            var peaScene = GD.Load<PackedScene>("res://Scenes/Combat/Pea.tscn");
            if (peaScene != null)
            {
                var pea = peaScene.Instantiate<Pea>();
                GetTree().CurrentScene.AddChild(pea);

                // 随机Y位置
                float randomY = 200 + GD.Randf() * 300; // 200-500范围内
                var startPos = new Vector2(100, randomY);
                var direction = Vector2.Right;

                // 激活豌豆
                pea.Activate(startPos, direction);

                GD.Print($"[ProjectileTest] 发射豌豆 #{_peaCount + 1}: 位置{startPos}");
            }
            else
            {
                GD.Print("[ProjectileTest] 无法加载豌豆场景");
            }
        }

        public override void _Input(InputEvent @event)
        {
            // Space键手动发射豌豆
            if (@event.IsActionPressed("ui_accept")) // Space或Enter
            {
                CreateTestPea();
            }
        }

        public override void _Process(double delta)
        {
            // 显示当前状态
            if (CollisionManager.Instance != null)
            {
                var stats = CollisionManager.Instance.GetPerformanceStats();
                // 可以将统计信息显示在屏幕上或输出到控制台
            }
        }
    }
}