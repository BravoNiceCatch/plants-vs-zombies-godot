using Godot;
using Plants大战僵尸.Scripts.Plants;
using Plants大战僵尸.Scripts.Game;
using PlantsVsZombies.Core;
using System;

namespace PlantsVsZombies.UI
{
	/// <summary>
	/// 植物卡片组件 - 用于显示可选择的植物
	/// 集成了阳光成本检查、购买逻辑和冷却管理
	/// </summary>
	public partial class PlantCard : Control
	{
		[Export] public PlantType PlantType { get; set; } = PlantType.None;
		[Export] public Texture2D PlantIcon { get; set; }
		[Export] public PackedScene PlantScene { get; set; }

		// 植物信息（从 PlantType 获取）
		public string PlantName => PlantType.GetDisplayName();
		public int SunCost => PlantType.GetSunCost();
		public float CooldownTime => PlantType.GetCooldownTime();
		public string Description => PlantType.GetDescription();

		// UI组件
		private Panel _backgroundPanel;
		private TextureRect _iconRect;
		private Label _nameLabel;
		private Label _costLabel;
		private Label _cooldownLabel;
		private TextureRect _sunIcon;
		private AnimationPlayer _animationPlayer;
		private ProgressBar _cooldownBar;

		// 系统引用
		private PlantPurchaseSystem _purchaseSystem;
		private SunlightManager _sunlightManager;

		// 状态
		private bool _isAvailable = true;
		private bool _isOnCooldown = false;
		private float _currentCooldown = 0f;
		private bool _isSelected = false;
		private int _currentSunlight = 0;

		// 信号
		[Signal] public delegate void CardSelectedEventHandler(PlantCard card);
		[Signal] public delegate void CardDeselectedEventHandler(PlantCard card);
		[Signal] public delegate void PlantPurchaseRequestedEventHandler(PlantType plantType, int gridX, int gridY);

		public override void _Ready()
		{
			InitializeComponents();
			SetupVisuals();
			ConnectSignals();
			FindSystemReferences();
		}

		public override void _Process(double delta)
		{
			UpdateCooldown((float)delta);
			UpdateAvailability();
		}

		/// <summary>
		/// 初始化UI组件
		/// </summary>
		private void InitializeComponents()
		{
			// 设置卡片大小
			CustomMinimumSize = new Vector2(100, 120);
			Size = new Vector2(100, 120);

			// 创建背景面板
			_backgroundPanel = new Panel();
			_backgroundPanel.Size = Size;
			AddChild(_backgroundPanel);

			// 创建植物图标
			_iconRect = new TextureRect();
			_iconRect.Position = new Vector2(10, 10);
			_iconRect.Size = new Vector2(80, 60);
			_iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
			_iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			AddChild(_iconRect);

			// 创建植物名称标签
			_nameLabel = new Label();
			_nameLabel.Position = new Vector2(5, 75);
			_nameLabel.Size = new Vector2(90, 15);
			_nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_nameLabel.VerticalAlignment = VerticalAlignment.Center;
			_nameLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_nameLabel);

			// 创建阳光图标
			_sunIcon = new TextureRect();
			_sunIcon.Position = new Vector2(5, 95);
			_sunIcon.Size = new Vector2(16, 16);
			_sunIcon.Texture = CreateSunTexture();
			AddChild(_sunIcon);

			// 创建阳光费用标签
			_costLabel = new Label();
			_costLabel.Position = new Vector2(25, 95);
			_costLabel.Size = new Vector2(30, 16);
			_costLabel.Modulate = Colors.Yellow;
			_costLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_costLabel);

			// 创建冷却时间标签
			_cooldownLabel = new Label();
			_cooldownLabel.Position = new Vector2(60, 95);
			_cooldownLabel.Size = new Vector2(35, 16);
			_cooldownLabel.Modulate = Colors.Cyan;
			_cooldownLabel.Visible = false;
			_cooldownLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_cooldownLabel);

			// 创建冷却进度条
			_cooldownBar = new ProgressBar();
			_cooldownBar.Position = new Vector2(5, 105);
			_cooldownBar.Size = new Vector2(90, 8);
			_cooldownBar.MaxValue = CooldownTime;
			_cooldownBar.Value = 0;
			_cooldownBar.Visible = false;
			_cooldownBar.Modulate = Colors.Cyan;
			AddChild(_cooldownBar);

			// 创建动画播放器
			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);
		}

		/// <summary>
		/// 查找系统引用
		/// </summary>
		private void FindSystemReferences()
		{
			// 查找植物购买系统
			var gameManager = GameManager.Instance;
			if (gameManager != null)
			{
				_purchaseSystem = gameManager.GetNode<PlantPurchaseSystem>("PlantPurchaseSystem");
				if (_purchaseSystem == null)
				{
					GD.PrintErr("无法找到 PlantPurchaseSystem");
				}

				// 查找阳光管理器
				_sunlightManager = gameManager.GetNode<SunlightManager>("SunlightManager");
				if (_sunlightManager != null)
				{
					// 订阅阳光变化事件
					_sunlightManager.OnSunlightChanged += OnSunlightChanged;
					_currentSunlight = _sunlightManager.CurrentSunlight;
				}
				else
				{
					GD.PrintErr("无法找到 SunlightManager");
				}
			}

			// 如果找到购买系统，连接事件
			if (_purchaseSystem != null)
			{
				_purchaseSystem.OnPlantSelected += OnPlantPurchaseSystemSelected;
				_purchaseSystem.OnPlantDeselected += OnPlantPurchaseSystemDeselected;
			}
		}

		/// <summary>
		/// 设置视觉效果
		/// </summary>
		private void SetupVisuals()
		{
			// 设置背景样式
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = PlantType.GetPrimaryColor();
			styleBox.BgColor = styleBox.BgColor.Darkened(0.3f);
			styleBox.BorderColor = PlantType.GetSecondaryColor();
			styleBox.BorderWidthLeft = 2;
			styleBox.BorderWidthRight = 2;
			styleBox.BorderWidthTop = 2;
			styleBox.BorderWidthBottom = 2;
			styleBox.CornerRadiusTopLeft = 8;
			styleBox.CornerRadiusTopRight = 8;
			styleBox.CornerRadiusBottomLeft = 8;
			styleBox.CornerRadiusBottomRight = 8;
			_backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);

			// 设置植物信息
			_nameLabel.Text = PlantName;
			_costLabel.Text = SunCost.ToString();

			// 如果有植物图标，使用它；否则创建默认图标
			if (PlantIcon != null)
			{
				_iconRect.Texture = PlantIcon;
			}
			else
			{
				_iconRect.Texture = CreateDefaultPlantIcon();
			}

			// 设置工具提示
			TooltipText = $"{PlantName}\n{Description}\n阳光成本: {SunCost}\n冷却时间: {CooldownTime}秒";
		}

		/// <summary>
		/// 创建标签样式框
		/// </summary>
		private StyleBoxFlat CreateLabelStyleBox()
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0, 0, 0, 0.5f);
			styleBox.CornerRadiusTopLeft = 3;
			styleBox.CornerRadiusTopRight = 3;
			styleBox.CornerRadiusBottomLeft = 3;
			styleBox.CornerRadiusBottomRight = 3;
			return styleBox;
		}

		/// <summary>
		/// 创建阳光纹理
		/// </summary>
		private Texture2D CreateSunTexture()
		{
			var image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
			var center = new Vector2I(8, 8);

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					var pos = new Vector2I(x, y);
					var dist = pos.DistanceTo(center);

					if (dist <= 6f)
					{
						var color = new Color(1.0f, 0.8f, 0.0f, 1.0f);
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
		/// 创建默认植物图标
		/// </summary>
		private Texture2D CreateDefaultPlantIcon()
		{
			var image = Image.CreateEmpty(80, 60, false, Image.Format.Rgba8);
			var primaryColor = PlantType.GetPrimaryColor();
			var secondaryColor = PlantType.GetSecondaryColor();

			// 根据植物类型创建不同的图标
			switch (PlantType)
			{
				case PlantType.Sunflower:
					DrawSunflowerIcon(image, primaryColor, secondaryColor);
					break;
				case PlantType.Peashooter:
					DrawPeashooterIcon(image, primaryColor, secondaryColor);
					break;
				case PlantType.CherryBomb:
					DrawCherryBombIcon(image, primaryColor, secondaryColor);
					break;
				default:
					DrawDefaultLeafIcon(image, primaryColor);
					break;
			}

			return ImageTexture.CreateFromImage(image);
		}

		/// <summary>
		/// 绘制太阳花图标
		/// </summary>
		private void DrawSunflowerIcon(Image image, Color primaryColor, Color secondaryColor)
		{
			var center = new Vector2I(40, 30);

			// 绘制花瓣
			for (int i = 0; i < 8; i++)
			{
				var angle = (float)i / 8 * Mathf.Tau;
				var petalCenter = center + new Vector2I(
					(int)(Mathf.Cos(angle) * 15),
					(int)(Mathf.Sin(angle) * 15)
				);

				for (int x = 0; x < 80; x++)
				{
					for (int y = 0; y < 60; y++)
					{
						var pos = new Vector2I(x, y);
						var dist = pos.DistanceTo(petalCenter);
						if (dist <= 8f)
						{
							image.SetPixel(x, y, secondaryColor);
						}
					}
				}
			}

			// 绘制中心
			for (int x = 0; x < 80; x++)
			{
				for (int y = 0; y < 60; y++)
				{
					var pos = new Vector2I(x, y);
					var dist = pos.DistanceTo(center);
					if (dist <= 10f)
					{
						image.SetPixel(x, y, primaryColor);
					}
				}
			}
		}

		/// <summary>
		/// 绘制豌豆射手图标
		/// </summary>
		private void DrawPeashooterIcon(Image image, Color primaryColor, Color secondaryColor)
		{
			var center = new Vector2I(40, 30);

			// 绘制主体（椭圆形）
			for (int x = 0; x < 80; x++)
			{
				for (int y = 0; y < 60; y++)
				{
					var pos = new Vector2I(x, y);
					var distX = Mathf.Abs(pos.X - center.X) / 20f;
					var distY = Mathf.Abs(pos.Y - center.Y) / 15f;
					var dist = Mathf.Sqrt(distX * distX + distY * distY);

					if (dist <= 1f)
					{
						image.SetPixel(x, y, primaryColor);
					}
				}
			}

			// 绘制嘴巴（深绿色）
			for (int x = center.X + 15; x < center.X + 25; x++)
			{
				for (int y = center.Y - 5; y < center.Y + 5; y++)
				{
					if (x >= 0 && x < 80 && y >= 0 && y < 60)
					{
						image.SetPixel(x, y, secondaryColor.Darkened(0.3f));
					}
				}
			}
		}

		/// <summary>
		/// 绘制樱桃炸弹图标
		/// </summary>
		private void DrawCherryBombIcon(Image image, Color primaryColor, Color secondaryColor)
		{
			// 绘制两个圆形樱桃
			var leftCenter = new Vector2I(30, 35);
			var rightCenter = new Vector2I(50, 35);

			// 左樱桃
			for (int x = 0; x < 80; x++)
			{
				for (int y = 0; y < 60; y++)
				{
					var pos = new Vector2I(x, y);
					if (pos.DistanceTo(leftCenter) <= 12f)
					{
						image.SetPixel(x, y, primaryColor);
					}
					else if (pos.DistanceTo(rightCenter) <= 12f)
					{
						image.SetPixel(x, y, primaryColor);
					}
				}
			}

			// 绘制导火索
			for (int y = 15; y < 25; y++)
			{
				if (y >= 0 && y < 60)
				{
					image.SetPixel(39, y, secondaryColor);
					image.SetPixel(40, y, secondaryColor);
					image.SetPixel(41, y, secondaryColor);
				}
			}

			// 绘制火花
			for (int x = 35; x < 45; x++)
			{
				for (int y = 10; y < 18; y++)
				{
					if (x >= 0 && x < 80 && y >= 0 && y < 60)
					{
						image.SetPixel(x, y, Colors.Yellow);
					}
				}
			}
		}

		/// <summary>
		/// 绘制默认叶子图标
		/// </summary>
		private void DrawDefaultLeafIcon(Image image, Color primaryColor)
		{
			var center = new Vector2I(40, 30);

			for (int x = 0; x < 80; x++)
			{
				for (int y = 0; y < 60; y++)
				{
					var pos = new Vector2I(x, y);
					var dist = pos.DistanceTo(center);

					if (dist <= 20f)
					{
						var intensity = 1.0f - (dist / 20f);
						var color = primaryColor.Darkened(1.0f - intensity);
						image.SetPixel(x, y, color);
					}
					else
					{
						image.SetPixel(x, y, Colors.Transparent);
					}
				}
			}
		}

		/// <summary>
		/// 连接信号
		/// </summary>
		private void ConnectSignals()
		{
			GuiInput += OnGuiInput;
			MouseEntered += OnMouseEntered;
			MouseExited += OnMouseExited;
		}

		/// <summary>
		/// 处理GUI输入
		/// </summary>
		private void OnGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left && IsCardClickable())
				{
					SelectCard();
				}
			}
		}

		/// <summary>
		/// 鼠标进入处理
		/// </summary>
		private void OnMouseEntered()
		{
			if (IsCardClickable())
			{
				_backgroundPanel.Modulate = new Color(1.2f, 1.2f, 1.0f, 1.0f);
				Scale = new Vector2(1.05f, 1.05f);
			}
		}

		/// <summary>
		/// 鼠标退出处理
		/// </summary>
		private void OnMouseExited()
		{
			if (!_isSelected)
			{
				UpdateCardAppearance();
				Scale = Vector2.One;
			}
		}

		/// <summary>
		/// 检查卡片是否可点击
		/// </summary>
		private bool IsCardClickable()
		{
			return _isAvailable && !_isOnCooldown && PlantType.IsValidPlant();
		}

		/// <summary>
		/// 选择卡片
		/// </summary>
		public void SelectCard()
		{
			if (!IsCardClickable())
				return;

			// 通知购买系统
			if (_purchaseSystem != null)
			{
				_purchaseSystem.SelectPlant(PlantType);
			}

			_isSelected = true;
			UpdateCardAppearance();

			EmitSignal(SignalName.CardSelected, this);
			PlaySelectAnimation();
		}

		/// <summary>
		/// 取消选择卡片
		/// </summary>
		public void DeselectCard()
		{
			_isSelected = false;
			UpdateCardAppearance();

			EmitSignal(SignalName.CardDeselected, this);
		}

		/// <summary>
		/// 开始冷却
		/// </summary>
		public void StartCooldown()
		{
			if (_purchaseSystem != null)
			{
				_currentCooldown = _purchaseSystem.GetRemainingCooldown(PlantType);
			}
			else
			{
				_currentCooldown = CooldownTime;
			}

			if (_currentCooldown > 0)
			{
				_isOnCooldown = true;
				_cooldownLabel.Visible = true;
				_cooldownBar.Visible = true;
				_cooldownBar.MaxValue = CooldownTime;
				_isAvailable = false;

				UpdateCooldownDisplay();
			}
		}

		/// <summary>
		/// 更新冷却状态
		/// </summary>
		private void UpdateCooldown(float delta)
		{
			if (_isOnCooldown)
			{
				_currentCooldown -= delta;

				if (_currentCooldown <= 0f)
				{
					_isOnCooldown = false;
					_currentCooldown = 0f;
					_cooldownLabel.Visible = false;
					_cooldownBar.Visible = false;
					_isAvailable = true;

					// 如果未被选中，恢复正常颜色
					if (!_isSelected)
					{
						UpdateCardAppearance();
					}
				}

				UpdateCooldownDisplay();
			}
		}

		/// <summary>
		/// 更新可用性
		/// </summary>
		private void UpdateAvailability()
		{
			// 检查购买系统中的可用性
			if (_purchaseSystem != null)
			{
				_isAvailable = _purchaseSystem.CanPurchasePlant(PlantType);
				_currentCooldown = _purchaseSystem.GetRemainingCooldown(PlantType);
				_isOnCooldown = _currentCooldown > 0;
			}
			else
			{
				// 降级处理：基于阳光数量判断
				_isAvailable = _currentSunlight >= SunCost && !_isOnCooldown;
			}

			UpdateCardAppearance();
		}

		/// <summary>
		/// 更新卡片外观
		/// </summary>
		private void UpdateCardAppearance()
		{
			if (_isSelected)
			{
				// 选中状态 - 金色高亮
				_backgroundPanel.Modulate = new Color(1.0f, 1.0f, 0.5f, 1.0f);
				return;
			}

			if (_isOnCooldown)
			{
				// 冷却状态 - 灰色
				_backgroundPanel.Modulate = new Color(0.6f, 0.6f, 0.6f, 0.7f);
				_costLabel.Modulate = Colors.Gray;
				_nameLabel.Modulate = Colors.Gray;
				_iconRect.Modulate = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			}
			else if (!_isAvailable)
			{
				// 阳光不足状态 - 暗红色
				_backgroundPanel.Modulate = new Color(0.4f, 0.2f, 0.2f, 0.8f);
				_costLabel.Modulate = Colors.Red;
				_nameLabel.Modulate = Colors.Red;
				_iconRect.Modulate = new Color(0.8f, 0.6f, 0.6f, 1.0f);
			}
			else
			{
				// 可用状态 - 正常颜色
				_backgroundPanel.Modulate = Colors.White;
				_costLabel.Modulate = Colors.Yellow;
				_nameLabel.Modulate = Colors.White;
				_iconRect.Modulate = Colors.White;
			}
		}

		/// <summary>
		/// 更新冷却显示
		/// </summary>
		private void UpdateCooldownDisplay()
		{
			_cooldownLabel.Text = Mathf.Ceil(_currentCooldown).ToString();
			_cooldownBar.Value = CooldownTime - _currentCooldown;

			// 根据冷却状态调整透明度
			if (_isOnCooldown)
			{
				var alpha = 0.3f + (_currentCooldown / CooldownTime) * 0.4f;
				_backgroundPanel.Modulate = new Color(0.6f, 0.6f, 0.6f, alpha);
			}
		}

		/// <summary>
		/// 播放选择动画
		/// </summary>
		private void PlaySelectAnimation()
		{
			var animation = new Animation();
			animation.Length = 0.2f;

			var scaleTrack = animation.AddTrack(Animation.TrackType.Value, 0);
			animation.TrackSetPath(scaleTrack, ".");
			animation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One * 1.1f);
			animation.TrackInsertKey(scaleTrack, 0.1f, Vector2.One * 1.2f);
			animation.TrackInsertKey(scaleTrack, 0.2f, Vector2.One * 1.1f);

			var animLib = new AnimationLibrary();
			animLib.AddAnimation("select", animation);
			_animationPlayer.AddAnimationLibrary("ui_anim", animLib);
			_animationPlayer.Play("select");
		}

		/// <summary>
		/// 播放使用动画
		/// </summary>
		public void PlayUseAnimation()
		{
			var animation = new Animation();
			animation.Length = 0.3f;

			var scaleTrack = animation.AddTrack(Animation.TrackType.Value, 0);
			animation.TrackSetPath(scaleTrack, ".");
			animation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One * 1.1f);
			animation.TrackInsertKey(scaleTrack, 0.15f, Vector2.One * 0.8f);
			animation.TrackInsertKey(scaleTrack, 0.3f, Vector2.One);

			var modulateTrack = animation.AddTrack(Animation.TrackType.Value, 0);
			animation.TrackSetPath(modulateTrack, ".");
			animation.TrackInsertKey(modulateTrack, 0.0f, new Color(1.0f, 1.0f, 0.5f, 1.0f));
			animation.TrackInsertKey(modulateTrack, 0.15f, new Color(1.0f, 0.8f, 0.2f, 0.8f));
			animation.TrackInsertKey(modulateTrack, 0.3f, Colors.White);

			var animLib = new AnimationLibrary();
			animLib.AddAnimation("use", animation);
			_animationPlayer.AddAnimationLibrary("ui_anim", animLib);
			_animationPlayer.Play("use");
		}

		// 事件处理方法
		private void OnSunlightChanged(int newSunlight)
		{
			_currentSunlight = newSunlight;
			UpdateAvailability();
		}

		private void OnPlantPurchaseSystemSelected(PlantType selectedPlantType)
		{
			if (selectedPlantType != PlantType)
			{
				DeselectCard();
			}
		}

		private void OnPlantPurchaseSystemDeselected()
		{
			DeselectCard();
		}

		// 公共属性
		public bool IsAvailable => _isAvailable;
		public bool IsOnCooldown => _isOnCooldown;
		public bool IsSelected => _isSelected;

		/// <summary>
		/// 检查卡片是否可购买（包括冷却和阳光检查）
		/// </summary>
		public bool CanAfford(int currentSun)
		{
			return _isAvailable && !_isOnCooldown && currentSun >= SunCost && PlantType.IsValidPlant();
		}

		/// <summary>
		/// 重置卡片状态
		/// </summary>
		public void ResetCard()
		{
			_isSelected = false;
			_isOnCooldown = false;
			_currentCooldown = 0f;
			_isAvailable = true;
			_cooldownLabel.Visible = false;
			_cooldownBar.Visible = false;
			_cooldownBar.Value = 0;
			UpdateCardAppearance();
		}

		public PackedScene PlantPrefab => PlantScene;
	}
}
