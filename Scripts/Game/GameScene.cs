using Godot;

namespace PlantsVsZombies.Game
{
    public partial class GameScene : Node2D
    {
        private GridContainer _gameGrid;
        private Label _sunCountLabel;
        private Label _zombieCountLabel;
        private int _sunCount = 50; // 初始阳光
        private int _zombieKillCount = 0; // 击杀僵尸数
        
        public override void _Ready()
        {
            InitializeGameUI();
            ConnectSignals();
        }
        
        private void InitializeGameUI()
        {
            // 创建游戏网格 (5行9列)
            _gameGrid = new GridContainer();
            _gameGrid.Columns = 9;
            _gameGrid.Position = new Vector2I(300, 150); // 偏移以适应UI布局
            AddChild(_gameGrid);
            
            // 创建网格单元格
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var slot = new Panel();
                    slot.Size = new Vector2I(120, 120);
                    slot.Modulate = Colors.Green;
                    
                    // 添加边框
                    var styleBox = new StyleBoxFlat();
                    styleBox.BorderColor = Colors.DarkGreen;
                    styleBox.BorderWidthLeft = 2;
                    styleBox.BorderWidthRight = 2;
                    styleBox.BorderWidthTop = 2;
                    styleBox.BorderWidthBottom = 2;
                    slot.AddThemeStyleboxOverride("panel", styleBox);
                    
                    _gameGrid.AddChild(slot);
                }
            }
            
            // 创建阳光显示
            _sunCountLabel = new Label();
            _sunCountLabel.Text = $"阳光: {_sunCount}";
            _sunCountLabel.Position = new Vector2I(50, 50);
            _sunCountLabel.Modulate = Colors.Yellow;
            AddChild(_sunCountLabel);
            
            // 创建击杀数显示
            _zombieCountLabel = new Label();
            _zombieCountLabel.Text = $"击杀: {_zombieKillCount}";
            _zombieCountLabel.Position = new Vector2I(50, 80);
            _zombieCountLabel.Modulate = Colors.Red;
            AddChild(_zombieCountLabel);
        }
        
        private void ConnectSignals()
        {
            // 后续连接游戏相关信号
        }
        
        public void AddSun(int amount)
        {
            _sunCount += amount;
            UpdateSunDisplay();
        }
        
        public bool SpendSun(int amount)
        {
            if (_sunCount >= amount)
            {
                _sunCount -= amount;
                UpdateSunDisplay();
                return true;
            }
            return false;
        }
        
        private void UpdateSunDisplay()
        {
            _sunCountLabel.Text = $"阳光: {_sunCount}";
        }
        
        public void OnZombieKilled()
        {
            _zombieKillCount++;
            _zombieCountLabel.Text = $"击杀: {_zombieKillCount}";
        }
    }
}