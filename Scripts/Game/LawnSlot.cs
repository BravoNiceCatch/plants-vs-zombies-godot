using Godot;

namespace PlantsVsZombies.Core
{
	/// <summary>
	/// 草坪格子组件 - 单个草坪地块
	/// </summary>
	public partial class LawnSlot : Panel
	{
		[Export] public Vector2I GridPosition { get; set; } // 网格坐标
		[Export] public bool IsOccupied { get; private set; } // 是否被占用
		[Export] public Texture2D GrassTexture { get; set; } // 草坪纹理
		
		private Sprite2D _grassSprite;
		private Label _rowLabel;
		private AnimationPlayer _animationPlayer;
		private static readonly Color[] RowColors = 
		{
			new Color(0.2f, 0.4f, 0.1f),  // 深绿
			new Color(0.3f, 0.5f, 0.15f), // 中绿
			new Color(0.25f, 0.45f, 0.12f) // 浅绿
		};
		
		public override void _Ready()
		{
			InitializeGrassAppearance();
			CreateRowIndicator();
			InitializeAnimation();
		}
		
		/// <summary>
		/// 初始化草坪外观
		/// </summary>
		private void InitializeGrassAppearance()
		{
			Size = new Vector2I(170, 170); // 最终优化为 170x170 格子大小以占据更大屏幕空间
			CustomMinimumSize = new Vector2I(170, 170); // 强制最小大小
			
			// 设置基础草坪颜色
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = RowColors[GridPosition.Y % RowColors.Length];
			styleBox.BorderColor = Colors.DarkGreen;
			styleBox.BorderWidthLeft = 1;
			styleBox.BorderWidthRight = 1;
			styleBox.BorderWidthTop = 1;
			styleBox.BorderWidthBottom = 1;
			AddThemeStyleboxOverride("panel", styleBox);
			
			// 添加草坪纹理精灵
			_grassSprite = new Sprite2D();
			_grassSprite.Name = "GrassSprite";
			_grassSprite.Position = Size / 2;
			_grassSprite.Modulate = new Color(1, 1, 1, 0.3f); // 半透明
			AddChild(_grassSprite);
			
			// 如果有纹理，则使用纹理
			if (GrassTexture != null)
			{
				_grassSprite.Texture = GrassTexture;
				_grassSprite.Scale = new Vector2(80f / GrassTexture.GetSize().X, 80f / GrassTexture.GetSize().Y);
			}
			else
			{
				// 使用程序化生成的草坪纹理
				GenerateGrassTexture();
			}
		}
		
		/// <summary>
		/// 程序化生成草坪纹理
		/// </summary>
		private void GenerateGrassTexture()
		{
			var image = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
			
			// 生成随机草坪纹理
			for (int x = 0; x < 64; x++)
			{
				for (int y = 0; y < 64; y++)
				{
					var noise = GD.Randf() * 0.3f; // 随机噪点
					var color = RowColors[GridPosition.Y % RowColors.Length];
					
					// 调整颜色亮度
					var r = Mathf.Clamp(color.R + noise - 0.15f, 0, 1);
					var g = Mathf.Clamp(color.G + noise - 0.1f, 0, 1);
					var b = Mathf.Clamp(color.B + noise - 0.2f, 0, 1);
					
					image.SetPixel(x, y, new Color(r, g, b, 1));
				}
			}
			
			var texture = ImageTexture.CreateFromImage(image);
			_grassSprite.Texture = texture;
			_grassSprite.Scale = new Vector2(1.2f, 1.2f);
		}
		
		/// <summary>
		/// 创建行列指示器
		/// </summary>
		private void CreateRowIndicator()
		{
			// 创建行数标签
			_rowLabel = new Label();
			_rowLabel.Text = $"行{GridPosition.Y + 1}";
			_rowLabel.Position = new Vector2I(5, 5);
			_rowLabel.Modulate = new Color(1, 1, 1, 0.6f);
			_rowLabel.AddThemeStyleboxOverride("normal", CreateTransparentStyleBox());
			AddChild(_rowLabel);
		}
		
		/// <summary>
		/// 创建透明背景样式
		/// </summary>
		private StyleBoxFlat CreateTransparentStyleBox()
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0, 0, 0, 0.3f);
			styleBox.CornerRadiusTopLeft = 3;
			styleBox.CornerRadiusTopRight = 3;
			styleBox.CornerRadiusBottomLeft = 3;
			styleBox.CornerRadiusBottomRight = 3;
			return styleBox;
		}
		
		/// <summary>
		/// 初始化动画组件
		/// </summary>
		private void InitializeAnimation()
		{
			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);
			
			// 创建简单的摇摆动画
			CreateSwayAnimation();
		}
		
		/// <summary>
		/// 创建摇摆动画
		/// </summary>
		private void CreateSwayAnimation()
		{
			var animation = new Animation();
			animation.Length = 4.0f;
			animation.LoopMode = Animation.LoopModeEnum.None;
			
			// 为草坪精灵添加摇摆轨迹
			var track = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(track, "GrassSprite:rotation");
			animation.TrackInsertKey(track, 0.0f, 0f);
			animation.TrackInsertKey(track, 1.0f, Mathf.DegToRad(2f));
			animation.TrackInsertKey(track, 2.0f, 0f);
			animation.TrackInsertKey(track, 3.0f, Mathf.DegToRad(-2f));
			animation.TrackInsertKey(track, 4.0f, 0f);
			
			var animLib = new AnimationLibrary();
			animLib.AddAnimation("sway", animation);
			_animationPlayer.AddAnimationLibrary("lawn_anim", animLib);
			_animationPlayer.Play("lawn_anim/sway");
			
			// 随机开始时间，让每个格子的动画不同步
			_animationPlayer.Seek(GD.Randf() * animation.Length);
		}
		
		/// <summary>
		/// 标记格子为已占用
		/// </summary>
		public void SetOccupied(bool occupied)
		{
			IsOccupied = occupied;
			Modulate = occupied ? new Color(0.8f, 0.8f, 0.8f, 1.0f) : Colors.White;
		}
		
		/// <summary>
		/// 高亮显示格子（用于种植提示）
		/// </summary>
		public void Highlight(bool highlight)
		{
			if (highlight)
			{
				Modulate = new Color(1.2f, 1.2f, 1.0f, 1.0f);
				Scale = new Vector2(1.05f, 1.05f);
			}
			else
			{
				Modulate = IsOccupied ? new Color(0.8f, 0.8f, 0.8f, 1.0f) : Colors.White;
				Scale = Vector2.One;
			}
		}
		
		/// <summary>
		/// 播放种植动画
		/// </summary>
		public void PlayPlantAnimation()
		{
			var animation = new Animation();
			animation.Length = 0.3f;
			
			// 缩放动画
			var scaleTrack = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(scaleTrack, "scale");
			animation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One * 1.2f);
			animation.TrackInsertKey(scaleTrack, 0.3f, Vector2.One);
			
			var animLib = new AnimationLibrary();
			animLib.AddAnimation("plant", animation);
			_animationPlayer.AddAnimationLibrary("lawn_anim", animLib);
			_animationPlayer.Play("plant");
		}
		
		/// <summary>
		/// 获取格子中心的世界坐标
		/// </summary>
		public Vector2 GetCenterPosition()
		{
			return Position + Size / 2;
		}
	}
}
