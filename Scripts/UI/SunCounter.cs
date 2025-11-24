using Godot;

namespace PlantsVsZombies.UI
{
	/// <summary>
	/// 阳光计数器UI组件
	/// </summary>
	public partial class SunCounter : Control
	{
		[Export] public int CurrentSun { get; private set; } = 50;

		// UI组件
		private Panel _backgroundPanel;
		private TextureRect _sunIcon;
		private Label _sunCountLabel;
		private AnimationPlayer _animationPlayer;
		private AudioStreamPlayer2D _collectSound;

		// 动画相关
		private bool _isAnimating = false;

		// 信号
		[Signal] public delegate void SunChangedEventHandler(int newAmount);
		[Signal] public delegate void SunCollectedEventHandler(int amount);

		public override void _Ready()
		{
			InitializeComponents();
			SetupVisuals();
			CreateAnimations();
		}

		/// <summary>
		/// 初始化组件
		/// </summary>
		private void InitializeComponents()
		{
			// 设置大小和位置
			Size = new Vector2I(200, 80);
			Position = new Vector2I(50, 20);

			// 创建背景面板
			_backgroundPanel = new Panel();
			_backgroundPanel.Size = Size;
			AddChild(_backgroundPanel);

			// 创建阳光图标
			_sunIcon = new TextureRect();
			_sunIcon.Name = "SunIcon";
			_sunIcon.Position = new Vector2I(10, 15);
			_sunIcon.Size = new Vector2I(50, 50);
			_sunIcon.Texture = CreateSunTexture();
			_sunIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
			AddChild(_sunIcon);

			// 创建阳光数量标签
			_sunCountLabel = new Label();
			_sunCountLabel.Name = "SunCountLabel";
			_sunCountLabel.Position = new Vector2I(70, 20);
			_sunCountLabel.Size = new Vector2I(120, 40);
			_sunCountLabel.Text = CurrentSun.ToString();
			_sunCountLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_sunCountLabel);

			// 创建动画播放器
			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);

			// 创建音效播放器（可选）
			_collectSound = new AudioStreamPlayer2D();
			AddChild(_collectSound);
		}

		/// <summary>
		/// 设置视觉效果
		/// </summary>
		private void SetupVisuals()
		{
			// 设置背景样式
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
			styleBox.BorderColor = Colors.Yellow;
			styleBox.BorderWidthLeft = 3;
			styleBox.BorderWidthRight = 3;
			styleBox.BorderWidthTop = 3;
			styleBox.BorderWidthBottom = 3;
			styleBox.CornerRadiusTopLeft = 15;
			styleBox.CornerRadiusTopRight = 15;
			styleBox.CornerRadiusBottomLeft = 15;
			styleBox.CornerRadiusBottomRight = 15;

			// 添加发光效果
			styleBox.ShadowColor = new Color(1.0f, 0.8f, 0.0f, 0.5f);
			styleBox.ShadowSize = 4;
			styleBox.ShadowOffset = new Vector2I(2, 2);

			_backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);

			// 设置标签样式
			var font = ThemeDB.FallbackFont;
				_sunCountLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			_sunCountLabel.AddThemeStyleboxOverride("hover", CreateLabelStyleBox());
			_sunCountLabel.Modulate = Colors.Yellow;
		}

		/// <summary>
		/// 创建标签样式框
		/// </summary>
		private StyleBoxFlat CreateLabelStyleBox()
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0, 0, 0, 0.3f);
			styleBox.CornerRadiusTopLeft = 8;
			styleBox.CornerRadiusTopRight = 8;
			styleBox.CornerRadiusBottomLeft = 8;
			styleBox.CornerRadiusBottomRight = 8;
			return styleBox;
		}

		/// <summary>
		/// 创建阳光纹理
		/// </summary>
		private Texture2D CreateSunTexture()
		{
			var imageSize = 64;
			var image = Image.CreateEmpty(imageSize, imageSize, false, Image.Format.Rgba8);
			var center = new Vector2I(imageSize / 2, imageSize / 2);

			for (int x = 0; x < imageSize; x++)
			{
				for (int y = 0; y < imageSize; y++)
				{
					var pos = new Vector2I(x, y);
					var distance = pos.DistanceTo(center);

					if (distance <= imageSize / 3)
					{
						// 创建太阳光线效果
						var angle = Mathf.Atan2(y - center.Y, x - center.X);
						var rayAngle = Mathf.Round(angle / (Mathf.Pi / 8)) * (Mathf.Pi / 8); // 16条光线
						var rayIntensity = 1.0f - Mathf.Abs(angle - rayAngle) / (Mathf.Pi / 16);

						// 径向渐变
						var radialGradient = 1.0f - (distance / (imageSize / 3));

						// 组合效果
						var finalIntensity = radialGradient * (0.7f + rayIntensity * 0.3f);
						var brightness = 0.8f + finalIntensity * 0.2f;

						var color = new Color(brightness, brightness * 0.9f, brightness * 0.2f, 1.0f);
						image.SetPixel(x, y, color);
					}
					else
					{
						image.SetPixel(x, y, Colors.Transparent);
					}
				}
			}

			return ImageTexture.CreateFromImage(image);
		}

		/// <summary>
		/// 创建动画
		/// </summary>
		private void CreateAnimations()
		{
			// 创建统一的动画库
			var animLib = new AnimationLibrary();

			// 创建收集动画
			var collectAnimation = new Animation();
			collectAnimation.Length = 0.5f;

			// 缩放动画
			var scaleTrack = collectAnimation.AddTrack(Animation.TrackType.Value, 0);
			collectAnimation.TrackSetPath(scaleTrack, "SunIcon:scale");
			collectAnimation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One);
			collectAnimation.TrackInsertKey(scaleTrack, 0.2f, Vector2.One * 1.3f);
			collectAnimation.TrackInsertKey(scaleTrack, 0.5f, Vector2.One);

			// 旋转动画
			var rotationTrack = collectAnimation.AddTrack(Animation.TrackType.Value, 0);
			collectAnimation.TrackSetPath(rotationTrack, "SunIcon:rotation");
			collectAnimation.TrackInsertKey(rotationTrack, 0.0f, 0f);
			collectAnimation.TrackInsertKey(rotationTrack, 0.5f, Mathf.Tau); // 完整旋转一圈

			// 颜色动画
			var modulateTrack = collectAnimation.AddTrack(Animation.TrackType.Value, 0);
			collectAnimation.TrackSetPath(modulateTrack, "SunIcon:modulate");
			collectAnimation.TrackInsertKey(modulateTrack, 0.0f, new Color(1.0f, 1.0f, 0.2f, 1.0f));
			collectAnimation.TrackInsertKey(modulateTrack, 0.3f, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			collectAnimation.TrackInsertKey(modulateTrack, 0.5f, Colors.White);

			animLib.AddAnimation("collect", collectAnimation);

			// 创建数字变化动画
			var numberAnimation = new Animation();
			numberAnimation.Length = 0.3f;

			var numberScaleTrack = numberAnimation.AddTrack(Animation.TrackType.Value, 0);
			numberAnimation.TrackSetPath(numberScaleTrack, "SunCountLabel:scale");
			numberAnimation.TrackInsertKey(numberScaleTrack, 0.0f, Vector2.One * 1.2f);
			numberAnimation.TrackInsertKey(numberScaleTrack, 0.3f, Vector2.One);

			animLib.AddAnimation("numberChange", numberAnimation); // 修复：使用正确的动画名称

			// 创建消耗动画
			var spendAnimation = new Animation();
			spendAnimation.Length = 0.2f;

			var spendScaleTrack = spendAnimation.AddTrack(Animation.TrackType.Value, 0);
			spendAnimation.TrackSetPath(spendScaleTrack, "SunIcon:scale");
			spendAnimation.TrackInsertKey(spendScaleTrack, 0.0f, Vector2.One * 0.8f);
			spendAnimation.TrackInsertKey(spendScaleTrack, 0.2f, Vector2.One);

			var spendModulateTrack = spendAnimation.AddTrack(Animation.TrackType.Value, 0);
			spendAnimation.TrackSetPath(spendModulateTrack, "SunIcon:modulate");
			spendAnimation.TrackInsertKey(spendModulateTrack, 0.0f, new Color(1.0f, 0.5f, 0.5f, 1.0f)); // 红色闪一下
			spendAnimation.TrackInsertKey(spendModulateTrack, 0.2f, new Color(1.0f, 1.0f, 1.0f, 1.0f));

			animLib.AddAnimation("spend", spendAnimation);

			// 创建警告动画
			var warningAnimation = new Animation();
			warningAnimation.Length = 0.3f;

			var warningModulateTrack = warningAnimation.AddTrack(Animation.TrackType.Value, 0);
			warningAnimation.TrackSetPath(warningModulateTrack, "SunIcon:modulate");
			warningAnimation.TrackInsertKey(warningModulateTrack, 0.0f, Colors.Red);
			warningAnimation.TrackInsertKey(warningModulateTrack, 0.15f, Colors.White);
			warningAnimation.TrackInsertKey(warningModulateTrack, 0.3f, Colors.Red);
			warningAnimation.TrackInsertKey(warningModulateTrack, 0.45f, new Color(1.0f, 1.0f, 1.0f, 1.0f));

			var warningScaleTrack = warningAnimation.AddTrack(Animation.TrackType.Value, 0);
			warningAnimation.TrackSetPath(warningScaleTrack, "SunIcon:scale");
			warningAnimation.TrackInsertKey(warningScaleTrack, 0.0f, Vector2.One * 1.1f);
			warningAnimation.TrackInsertKey(warningScaleTrack, 0.3f, Vector2.One);

			animLib.AddAnimation("warning", warningAnimation);

			// 一次性添加动画库
			_animationPlayer.AddAnimationLibrary("ui_anim", animLib);
		}

		/// <summary>
		/// 添加阳光
		/// </summary>
		public void AddSun(int amount)
		{
			if (amount <= 0) return;

			var oldAmount = CurrentSun;
			CurrentSun += amount;
			UpdateDisplay();

			// 播放收集动画
			PlayCollectAnimation();

			// 播放数字变化动画
			PlayNumberChangeAnimation();

			// 发出信号
			EmitSignal(SignalName.SunChanged, CurrentSun);
			EmitSignal(SignalName.SunCollected, amount);

			GD.Print($"[SunCounter] 阳光增加: {oldAmount} -> {CurrentSun} (+{amount})");
		}

		/// <summary>
		/// 消耗阳光
		/// </summary>
		public bool SpendSun(int amount)
		{
			if (amount <= 0) return true;
			if (CurrentSun < amount) return false;

			var oldAmount = CurrentSun;
			CurrentSun -= amount;
			UpdateDisplay();

			// 播放消耗动画（缩放缩小）
			PlaySpendAnimation();

			// 发出信号
			EmitSignal(SignalName.SunChanged, CurrentSun);

			GD.Print($"[SunCounter] 阳光消耗: {oldAmount} -> {CurrentSun} (-{amount})");
			return true;
		}

		/// <summary>
		/// 直接设置阳光数量
		/// </summary>
		public void SetSun(int amount)
		{
			if (amount < 0) amount = 0;

			CurrentSun = amount;
			UpdateDisplay();

			// 如果是增加，播放动画
			if (amount > 0)
			{
				PlayNumberChangeAnimation();
			}

			EmitSignal(SignalName.SunChanged, CurrentSun);
		}

		/// <summary>
		/// 检查是否有足够的阳光
		/// </summary>
		public bool HasEnoughSun(int amount)
		{
			return CurrentSun >= amount;
		}

		/// <summary>
		/// 更新显示
		/// </summary>
		private void UpdateDisplay()
		{
			// 防御性检查：如果标签还未初始化，直接返回
			if (_sunCountLabel == null)
				return;

			_sunCountLabel.Text = CurrentSun.ToString();

			// 根据阳光数量改变颜色
			if (CurrentSun >= 1000)
			{
				_sunCountLabel.Modulate = Colors.Gold; // 大量阳光
			}
			else if (CurrentSun >= 500)
			{
				_sunCountLabel.Modulate = Colors.Yellow; // 中等阳光
			}
			else if (CurrentSun >= 100)
			{
				_sunCountLabel.Modulate = Colors.LightYellow; // 少量阳光
			}
			else
			{
				_sunCountLabel.Modulate = Colors.White; // 不足阳光
			}
		}

		/// <summary>
		/// 播放收集动画
		/// </summary>
		private void PlayCollectAnimation()
		{
			if (!_isAnimating)
			{
				_isAnimating = true;
				_animationPlayer.Play("ui_anim/collect");
				_animationPlayer.AnimationFinished += (animName) =>
				{
					if (animName == "ui_anim/collect")
					{
						_isAnimating = false;
					}
				};
			}
		}

		/// <summary>
		/// 播放数字变化动画
		/// </summary>
		private void PlayNumberChangeAnimation()
		{
			_animationPlayer.Play("ui_anim/numberChange");
		}

		/// <summary>
		/// 播放消耗动画
		/// </summary>
		private void PlaySpendAnimation()
		{
			_animationPlayer.Play("ui_anim/spend");
		}

		/// <summary>
		/// 播放警告动画（阳光不足时）
		/// </summary>
		public void PlayWarningAnimation()
		{
			_animationPlayer.Play("ui_anim/warning");
		}

		public override void _Process(double delta)
		{
			// 可以在这里添加持续的视觉效果
			// 比如根据当前阳光数量调整亮度等
		}
	}
}
