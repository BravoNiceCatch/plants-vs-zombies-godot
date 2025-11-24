using Godot;

namespace PlantsVsZombies.UI
{
	public partial class MainMenu : Control
	{
		// 主菜单组件
		private Button _startButton;
		private Button _settingsButton;
		private Button _exitButton;
		private Label _titleLabel;
		private Panel _backgroundPanel;
		
		public override void _Ready()
		{
			InitializeComponents();
			SetupVisuals();
			ConnectSignals();
		}
		
		/// <summary>
		/// 初始化UI组件
		/// </summary>
		private void InitializeComponents()
		{
			// 创建背景 - 手动设置锚点填满父容器
			_backgroundPanel = new Panel();
			_backgroundPanel.AnchorLeft = 0;
			_backgroundPanel.AnchorTop = 0;
			_backgroundPanel.AnchorRight = 1;
			_backgroundPanel.AnchorBottom = 1;
			AddChild(_backgroundPanel);

			// 创建标题 - 使用锚点居中定位
			_titleLabel = new Label();
			_titleLabel.Text = "植物大战僵尸";
			_titleLabel.AnchorLeft = 0.5f;
			_titleLabel.AnchorTop = 0.5f;
			_titleLabel.AnchorRight = 0.5f;
			_titleLabel.AnchorBottom = 0.5f;
			_titleLabel.OffsetLeft = -200; // 相对于中心偏移
			_titleLabel.OffsetTop = -220;
			_titleLabel.OffsetRight = 200;
			_titleLabel.OffsetBottom = -180;
			AddChild(_titleLabel);

			// 创建按钮容器 - 使用锚点居中定位
			var buttonContainer = new VBoxContainer();
			buttonContainer.AnchorLeft = 0.5f;
			buttonContainer.AnchorTop = 0.5f;
			buttonContainer.AnchorRight = 0.5f;
			buttonContainer.AnchorBottom = 0.5f;
			buttonContainer.OffsetLeft = -150; // 相对于中心偏移
			buttonContainer.OffsetTop = -50;
			buttonContainer.OffsetRight = 150;
			buttonContainer.OffsetBottom = 200;
			buttonContainer.AddThemeConstantOverride("separation", 20);
			AddChild(buttonContainer);
			
			// 创建开始按钮
			_startButton = new Button();
			_startButton.Text = "开始游戏";
			_startButton.CustomMinimumSize = new Vector2I(300, 60);
			buttonContainer.AddChild(_startButton);
			
			// 创建设置按钮
			_settingsButton = new Button();
			_settingsButton.Text = "游戏设置";
			_settingsButton.CustomMinimumSize = new Vector2I(300, 60);
			buttonContainer.AddChild(_settingsButton);
			
			// 创建退出按钮
			_exitButton = new Button();
			_exitButton.Text = "退出游戏";
			_exitButton.CustomMinimumSize = new Vector2I(300, 60);
			buttonContainer.AddChild(_exitButton);
		}
		
		/// <summary>
		/// 设置视觉效果
		/// </summary>
		private void SetupVisuals()
		{
			// 设置背景样式 - 草坪主题
			var backgroundStyle = new StyleBoxFlat();
			backgroundStyle.BgColor = new Color(0.15f, 0.25f, 0.15f, 1.0f);
			backgroundStyle.BorderColor = Colors.DarkGreen;
			backgroundStyle.BorderWidthLeft = 5;
			backgroundStyle.BorderWidthRight = 5;
			backgroundStyle.BorderWidthTop = 5;
			backgroundStyle.BorderWidthBottom = 5;
			_backgroundPanel.AddThemeStyleboxOverride("panel", backgroundStyle);
			
			// 设置标题样式
			_titleLabel.Modulate = Colors.LightGreen;
			_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_titleLabel.VerticalAlignment = VerticalAlignment.Center;
			
			// 设置按钮样式
			var buttons = new Button[] { _startButton, _settingsButton, _exitButton };
			foreach (var button in buttons)
			{
				var buttonStyle = new StyleBoxFlat();
				buttonStyle.BgColor = new Color(0.2f, 0.4f, 0.2f, 0.9f);
				buttonStyle.BorderColor = Colors.LightGreen;
				buttonStyle.BorderWidthLeft = 3;
				buttonStyle.BorderWidthRight = 3;
				buttonStyle.BorderWidthTop = 3;
				buttonStyle.BorderWidthBottom = 3;
				buttonStyle.CornerRadiusTopLeft = 10;
				buttonStyle.CornerRadiusTopRight = 10;
				buttonStyle.CornerRadiusBottomLeft = 10;
				buttonStyle.CornerRadiusBottomRight = 10;
				
				button.AddThemeStyleboxOverride("normal", buttonStyle);
				button.Modulate = Colors.White;
			}
		}
		
		/// <summary>
		/// 连接信号
		/// </summary>
		private void ConnectSignals()
		{
			_startButton.Pressed += OnStartButtonPressed;
			_settingsButton.Pressed += OnSettingsButtonPressed;
			_exitButton.Pressed += OnExitButtonPressed;
		}
		
		/// <summary>
		/// 开始游戏按钮处理
		/// </summary>
		private void OnStartButtonPressed()
		{
			GD.Print("[MainMenu] 开始新游戏");
			GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
		}
		
		/// <summary>
		/// 设置按钮处理
		/// </summary>
		private void OnSettingsButtonPressed()
		{
			GD.Print("[MainMenu] 打开设置");
			// TODO: 实现设置面板
		}
		
		/// <summary>
		/// 退出游戏按钮处理
		/// </summary>
		private void OnExitButtonPressed()
		{
			GD.Print("[MainMenu] 退出游戏");
			GetTree().Quit();
		}
	}
}
