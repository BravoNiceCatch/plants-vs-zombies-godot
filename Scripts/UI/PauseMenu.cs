using Godot;

namespace PlantsVsZombies.UI
{
	/// <summary>
	/// 游戏暂停菜单
	/// </summary>
	public partial class PauseMenu : Control
	{
		// UI组件
		private Panel _backgroundPanel;
		private Label _titleLabel;
		private Button _resumeButton;
		private Button _settingsButton;
		private Button _restartButton;
		private Button _mainMenuButton;
		private Button _exitButton;
		private VBoxContainer _buttonContainer;
		private AnimationPlayer _animationPlayer;

		// 设置面板
		private SettingsPanel _settingsPanel;
		private bool _isSettingsOpen = false;

		// 信号
		[Signal] public delegate void ResumedEventHandler();
		[Signal] public delegate void RestartRequestedEventHandler();
		[Signal] public delegate void MainMenuRequestedEventHandler();
		[Signal] public delegate void ExitRequestedEventHandler();

		public override void _Ready()
		{
			InitializeComponents();
			SetupVisuals();
			CreateAnimations();
			ConnectSignals();

			// 初始时隐藏
			Visible = false;
			ProcessMode = ProcessModeEnum.WhenPaused;
		}

		public override void _Input(InputEvent @event)
		{
			// ESC键切换暂停状态
			if (@event.IsActionPressed("ui_cancel"))
			{
				if (_isSettingsOpen)
				{
					CloseSettings();
				}
				else if (Visible)
				{
					ResumeGame();
				}
				else
				{
					ShowPause();
				}
			}
		}

		/// <summary>
		/// 初始化组件
		/// </summary>
		private void InitializeComponents()
		{
			// 设置大小
			Size = new Vector2I(800, 600);
			Position = new Vector2I(560, 240); // 居中位置

			// 创建半透明背景
			var colorRect = new ColorRect();
			colorRect.Size = GetViewport().GetVisibleRect().Size * -1; // 全屏
			colorRect.Position = -Position; // 抵消当前节点的位置
			colorRect.Color = new Color(0, 0, 0, 0.7f);
			AddChild(colorRect);

			// 创建主背景面板
			_backgroundPanel = new Panel();
			_backgroundPanel.Size = Size;
			_backgroundPanel.Position = Vector2I.Zero;
			AddChild(_backgroundPanel);

			// 创建标题标签
			_titleLabel = new Label();
			_titleLabel.Text = "游戏暂停";
			_titleLabel.Position = new Vector2I(300, 50);
			_titleLabel.Size = new Vector2I(200, 50);
			AddChild(_titleLabel);

			// 创建按钮容器
			_buttonContainer = new VBoxContainer();
			_buttonContainer.Position = new Vector2I(250, 150);
			_buttonContainer.Size = new Vector2I(300, 400);
			_buttonContainer.AddThemeConstantOverride("separation", 20);
			AddChild(_buttonContainer);

			// 创建按钮
			CreateButtons();

			// 创建设置面板
			_settingsPanel = new SettingsPanel();
			_settingsPanel.Visible = false;
			AddChild(_settingsPanel);

			// 创建动画播放器
			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);
		}

		/// <summary>
		/// 创建按钮
		/// </summary>
		private void CreateButtons()
		{
			// 继续游戏按钮
			_resumeButton = new Button();
			_resumeButton.Text = "继续游戏";
			_resumeButton.CustomMinimumSize = new Vector2I(300, 60);
			_buttonContainer.AddChild(_resumeButton);

			// 设置按钮
			_settingsButton = new Button();
			_settingsButton.Text = "游戏设置";
			_settingsButton.CustomMinimumSize = new Vector2I(300, 60);
			_buttonContainer.AddChild(_settingsButton);

			// 重新开始按钮
			_restartButton = new Button();
			_restartButton.Text = "重新开始";
			_restartButton.CustomMinimumSize = new Vector2I(300, 60);
			_buttonContainer.AddChild(_restartButton);

			// 返回主菜单按钮
			_mainMenuButton = new Button();
			_mainMenuButton.Text = "返回主菜单";
			_mainMenuButton.CustomMinimumSize = new Vector2I(300, 60);
			_buttonContainer.AddChild(_mainMenuButton);

			// 退出游戏按钮
			_exitButton = new Button();
			_exitButton.Text = "退出游戏";
			_exitButton.CustomMinimumSize = new Vector2I(300, 60);
			_buttonContainer.AddChild(_exitButton);
		}

		/// <summary>
		/// 设置视觉效果
		/// </summary>
		private void SetupVisuals()
		{
			// 设置背景面板样式
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0.15f, 0.2f, 0.25f, 0.95f);
			styleBox.BorderColor = Colors.Cyan;
			styleBox.BorderWidthLeft = 4;
			styleBox.BorderWidthRight = 4;
			styleBox.BorderWidthTop = 4;
			styleBox.BorderWidthBottom = 4;
			styleBox.CornerRadiusTopLeft = 20;
			styleBox.CornerRadiusTopRight = 20;
			styleBox.CornerRadiusBottomLeft = 20;
			styleBox.CornerRadiusBottomRight = 20;

			// 添加发光效果
			styleBox.ShadowColor = new Color(0, 1, 1, 0.4f);
			styleBox.ShadowSize = 8;
			styleBox.ShadowOffset = new Vector2I(3, 3);

			_backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);

			// 设置标题样式
			_titleLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			_titleLabel.Modulate = Colors.Cyan;
			_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_titleLabel.VerticalAlignment = VerticalAlignment.Center;

			// 设置按钮样式
			SetupButtonStyles();
		}

		/// <summary>
		/// 设置按钮样式
		/// </summary>
		private void SetupButtonStyles()
		{
			var buttons = new Button[] { _resumeButton, _settingsButton, _restartButton, _mainMenuButton, _exitButton };

			foreach (var button in buttons)
			{
				var normalStyle = CreateButtonStyleBox(new Color(0.2f, 0.3f, 0.4f, 0.9f), Colors.Cyan);
				var hoverStyle = CreateButtonStyleBox(new Color(0.3f, 0.4f, 0.5f, 0.9f), Colors.White);
				var pressedStyle = CreateButtonStyleBox(new Color(0.1f, 0.2f, 0.3f, 0.9f), Colors.LightCyan);

				button.AddThemeStyleboxOverride("normal", normalStyle);
				button.AddThemeStyleboxOverride("hover", hoverStyle);
				button.AddThemeStyleboxOverride("pressed", pressedStyle);

				button.Modulate = Colors.White;
			}
		}

		/// <summary>
		/// 创建按钮样式框
		/// </summary>
		private StyleBoxFlat CreateButtonStyleBox(Color bgColor, Color borderColor)
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = bgColor;
			styleBox.BorderColor = borderColor;
			styleBox.BorderWidthLeft = 2;
			styleBox.BorderWidthRight = 2;
			styleBox.BorderWidthTop = 2;
			styleBox.BorderWidthBottom = 2;
			styleBox.CornerRadiusTopLeft = 10;
			styleBox.CornerRadiusTopRight = 10;
			styleBox.CornerRadiusBottomLeft = 10;
			styleBox.CornerRadiusBottomRight = 10;
			return styleBox;
		}

		/// <summary>
		/// 创建标签样式框
		/// </summary>
		private StyleBoxFlat CreateLabelStyleBox()
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0, 0, 0, 0.3f);
			styleBox.CornerRadiusTopLeft = 10;
			styleBox.CornerRadiusTopRight = 10;
			styleBox.CornerRadiusBottomLeft = 10;
			styleBox.CornerRadiusBottomRight = 10;
			return styleBox;
		}

		/// <summary>
		/// 创建动画
		/// </summary>
		private void CreateAnimations()
		{
			// 创建统一的动画库
			var animLib = new AnimationLibrary();

			// 显示动画
			var showAnimation = new Animation();
			showAnimation.Length = 0.3f;

			var scaleTrack = showAnimation.AddTrack(Animation.TrackType.Value, 0);
			showAnimation.TrackSetPath(scaleTrack, ".");
			showAnimation.TrackInsertKey(scaleTrack, 0.0f, Vector2.Zero);
			showAnimation.TrackInsertKey(scaleTrack, 0.3f, Vector2.One);

			var modulateTrack = showAnimation.AddTrack(Animation.TrackType.Value, 0);
			showAnimation.TrackSetPath(modulateTrack, ".");
			showAnimation.TrackInsertKey(modulateTrack, 0.0f, new Color(1, 1, 1, 0));
			showAnimation.TrackInsertKey(modulateTrack, 0.3f, new Color(1, 1, 1, 1));

			animLib.AddAnimation("show", showAnimation);

			// 隐藏动画
			var hideAnimation = new Animation();
			hideAnimation.Length = 0.2f;

			var hideScaleTrack = hideAnimation.AddTrack(Animation.TrackType.Value, 0);
			hideAnimation.TrackSetPath(hideScaleTrack, ".");
			hideAnimation.TrackInsertKey(hideScaleTrack, 0.0f, Vector2.One);
			hideAnimation.TrackInsertKey(hideScaleTrack, 0.2f, Vector2.Zero);

			var hideModulateTrack = hideAnimation.AddTrack(Animation.TrackType.Value, 0);
			hideAnimation.TrackSetPath(hideModulateTrack, ".");
			hideAnimation.TrackInsertKey(hideModulateTrack, 0.0f, new Color(1, 1, 1, 1));
			hideAnimation.TrackInsertKey(hideModulateTrack, 0.2f, new Color(1, 1, 1, 0));

			animLib.AddAnimation("hide", hideAnimation);

			// 一次性添加动画库
			_animationPlayer.AddAnimationLibrary("pause_anim", animLib);
		}

		/// <summary>
		/// 连接信号
		/// </summary>
		private void ConnectSignals()
		{
			_resumeButton.Pressed += ResumeGame;
			_settingsButton.Pressed += OpenSettings;
			_restartButton.Pressed += RequestRestart;
			_mainMenuButton.Pressed += RequestMainMenu;
			_exitButton.Pressed += RequestExit;

			_settingsPanel.Closed += CloseSettings;
		}

		/// <summary>
		/// 显示暂停菜单
		/// </summary>
		public void ShowPause()
		{
			if (Visible) return;

			Visible = true;
			GetTree().Paused = true;

			Scale = Vector2.Zero;
			Modulate = new Color(1, 1, 1, 0);
			_animationPlayer.Play("show");

			GD.Print("[PauseMenu] 游戏暂停");
		}

		/// <summary>
		/// 隐藏暂停菜单
		/// </summary>
		private void HidePause()
		{
			if (!Visible) return;

			_animationPlayer.Play("hide");
			_animationPlayer.AnimationFinished += (animName) =>
			{
				if (animName == "hide")
				{
					Visible = false;
					GetTree().Paused = false;
				}
			};
		}

		/// <summary>
		/// 继续游戏
		/// </summary>
		public void ResumeGame()
		{
			HidePause();
			EmitSignal(SignalName.Resumed);
			GD.Print("[PauseMenu] 继续游戏");
		}

		/// <summary>
		/// 打开设置
		/// </summary>
		private void OpenSettings()
		{
			_isSettingsOpen = true;
			_settingsPanel.Visible = true;
			_settingsPanel.ShowSettings();

			// 隐藏主菜单按钮
			_buttonContainer.Visible = false;

			GD.Print("[PauseMenu] 打开设置");
		}

		/// <summary>
		/// 关闭设置
		/// </summary>
		private void CloseSettings()
		{
			_isSettingsOpen = false;
			_settingsPanel.Visible = false;
			_buttonContainer.Visible = true;

			GD.Print("[PauseMenu] 关闭设置");
		}

		/// <summary>
		/// 请求重新开始
		/// </summary>
		private void RequestRestart()
		{
			// 这里可以添加确认对话框
			EmitSignal(SignalName.RestartRequested);
			HidePause();
			GD.Print("[PauseMenu] 请求重新开始");
		}

		/// <summary>
		/// 请求返回主菜单
		/// </summary>
		private void RequestMainMenu()
		{
			// 这里可以添加确认对话框
			EmitSignal(SignalName.MainMenuRequested);
			HidePause();
			GD.Print("[PauseMenu] 请求返回主菜单");
		}

		/// <summary>
		/// 请求退出游戏
		/// </summary>
		private void RequestExit()
		{
			// 这里可以添加确认对话框
			EmitSignal(SignalName.ExitRequested);
			GD.Print("[PauseMenu] 请求退出游戏");
		}

		/// <summary>
		/// 设置面板类
		/// </summary>
		private partial class SettingsPanel : Control
		{
			private Panel _backgroundPanel;
			private Label _titleLabel;
			private HSlider _masterVolumeSlider;
			private Label _masterVolumeLabel;
			private HSlider _musicVolumeSlider;
			private Label _musicVolumeLabel;
			private HSlider _sfxVolumeSlider;
			private Label _sfxVolumeLabel;
			private Button _backButton;
			private VBoxContainer _settingsContainer;

			[Signal] public delegate void ClosedEventHandler();

			public override void _Ready()
			{
				Size = new Vector2I(600, 400);
				Position = new Vector2I(100, 100);

				CreateComponents();
				SetupVisuals();
			}

			private void CreateComponents()
			{
				// 背景
				_backgroundPanel = new Panel();
				_backgroundPanel.Size = Size;
				AddChild(_backgroundPanel);

				// 标题
				_titleLabel = new Label();
				_titleLabel.Text = "游戏设置";
				_titleLabel.Position = new Vector2I(200, 20);
				_titleLabel.Size = new Vector2I(200, 40);
				AddChild(_titleLabel);

				// 设置容器
				_settingsContainer = new VBoxContainer();
				_settingsContainer.Position = new Vector2I(50, 80);
				_settingsContainer.Size = new Vector2I(500, 280);
				_settingsContainer.AddThemeConstantOverride("separation", 25);
				AddChild(_settingsContainer);

				// 主音量
				var masterVolumeContainer = new HBoxContainer();
				masterVolumeContainer.AddChild(new Label { Text = "主音量:", CustomMinimumSize = new Vector2I(100, 30) });
				_masterVolumeSlider = new HSlider { MinValue = 0, MaxValue = 100, Value = 80, CustomMinimumSize = new Vector2I(200, 30) };
				_masterVolumeLabel = new Label { Text = "80%", CustomMinimumSize = new Vector2I(50, 30) };
				masterVolumeContainer.AddChild(_masterVolumeSlider);
				masterVolumeContainer.AddChild(_masterVolumeLabel);
				_settingsContainer.AddChild(masterVolumeContainer);

				// 音乐音量
				var musicVolumeContainer = new HBoxContainer();
				musicVolumeContainer.AddChild(new Label { Text = "音乐音量:", CustomMinimumSize = new Vector2I(100, 30) });
				_musicVolumeSlider = new HSlider { MinValue = 0, MaxValue = 100, Value = 60, CustomMinimumSize = new Vector2I(200, 30) };
				_musicVolumeLabel = new Label { Text = "60%", CustomMinimumSize = new Vector2I(50, 30) };
				musicVolumeContainer.AddChild(_musicVolumeSlider);
				musicVolumeContainer.AddChild(_musicVolumeLabel);
				_settingsContainer.AddChild(musicVolumeContainer);

				// 音效音量
				var sfxVolumeContainer = new HBoxContainer();
				sfxVolumeContainer.AddChild(new Label { Text = "音效音量:", CustomMinimumSize = new Vector2I(100, 30) });
				_sfxVolumeSlider = new HSlider { MinValue = 0, MaxValue = 100, Value = 70, CustomMinimumSize = new Vector2I(200, 30) };
				_sfxVolumeLabel = new Label { Text = "70%", CustomMinimumSize = new Vector2I(50, 30) };
				sfxVolumeContainer.AddChild(_sfxVolumeSlider);
				sfxVolumeContainer.AddChild(_sfxVolumeLabel);
				_settingsContainer.AddChild(sfxVolumeContainer);

				// 返回按钮
				_backButton = new Button { Text = "返回", CustomMinimumSize = new Vector2I(200, 50) };
				_settingsContainer.AddChild(_backButton);

				// 连接信号
				_masterVolumeSlider.ValueChanged += (value) => _masterVolumeLabel.Text = $"{(int)value}%";
				_musicVolumeSlider.ValueChanged += (value) => _musicVolumeLabel.Text = $"{(int)value}%";
				_sfxVolumeSlider.ValueChanged += (value) => _sfxVolumeLabel.Text = $"{(int)value}%";
				_backButton.Pressed += () => EmitSignal(SignalName.Closed);
			}

			private void SetupVisuals()
			{
				var styleBox = new StyleBoxFlat();
				styleBox.BgColor = new Color(0.2f, 0.25f, 0.3f, 0.95f);
				styleBox.BorderColor = Colors.LightBlue;
				styleBox.BorderWidthLeft = 3;
				styleBox.CornerRadiusTopLeft = 15;
				styleBox.CornerRadiusTopRight = 15;
				styleBox.CornerRadiusBottomLeft = 15;
				styleBox.CornerRadiusBottomRight = 15;
				_backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);

				_titleLabel.Modulate = Colors.LightBlue;
				_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
			}

			public void ShowSettings()
			{
				Visible = true;
			}
		}
	}
}
