using Godot;

namespace PlantsVsZombies.UI
{
    public partial class MainMenu : Control
    {
        private Button _startButton;
        private Button _exitButton;
        
        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
        }
        
        private void InitializeUI()
        {
            // 设置UI基础布局
            Size = new Vector2I(1920, 1080);
            
            // 创建开始按钮
            _startButton = new Button();
            _startButton.Text = "开始游戏";
            _startButton.Size = new Vector2I(200, 50);
            _startButton.Position = new Vector2I(960 - 100, 540 - 50);
            AddChild(_startButton);
            
            // 创建退出按钮
            _exitButton = new Button();
            _exitButton.Text = "退出游戏";
            _exitButton.Size = new Vector2I(200, 50);
            _exitButton.Position = new Vector2I(960 - 100, 540 + 20);
            AddChild(_exitButton);
        }
        
        private void ConnectSignals()
        {
            _startButton.Pressed += OnStartButtonPressed;
            _exitButton.Pressed += OnExitButtonPressed;
        }
        
        private void OnStartButtonPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
        }
        
        private void OnExitButtonPressed()
        {
            GetTree().Quit();
        }
    }
}