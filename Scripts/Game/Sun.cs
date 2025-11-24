using Godot;

namespace PlantsVsZombies.Core
{
	/// <summary>
	/// 阳光组件 - 从天空掉落的阳光
	/// </summary>
	public partial class Sun : Area2D
	{
		[Export] public int SunValue { get; set; } = 25; // 阳光价值
		[Export] public float FallDuration { get; set; } = 3.0f; // 下落时间
		[Export] public float Lifetime { get; set; } = 8.0f; // 生命周期
		[Export] public float BobAmplitude { get; set; } = 20f; // 漂浮幅度
		[Export] public float BobFrequency { get; set; } = 2f; // 漂浮频率
		
		private Sprite2D _sunSprite;
		private AnimationPlayer _animationPlayer;
		private CollisionShape2D _collisionShape;
		private Timer _lifetimeTimer;
		private bool _isCollected = false;
		private bool _isGrounded = false;
		private Vector2 _targetPosition;
		private float _fallTimer = 0f;
		
		[Signal] public delegate void SunCollectedEventHandler(int value);
		[Signal] public delegate void SunExpiredEventHandler();
		
		public override void _Ready()
		{
			InitializeComponents();
			SetupVisuals();
			SetupCollision();
			SetupAnimation();
			SetupTimers();
		}
		
		/// <summary>
		/// 初始化组件
		/// </summary>
		private void InitializeComponents()
		{
			_sunSprite = new Sprite2D();
			_animationPlayer = new AnimationPlayer();
			_collisionShape = new CollisionShape2D();
			_lifetimeTimer = new Timer();
			
			// 设置节点名称，便于动画系统引用
			_sunSprite.Name = "SunSprite";
			
			AddChild(_sunSprite);
			AddChild(_animationPlayer);
			AddChild(_collisionShape);
			AddChild(_lifetimeTimer);
			
			// 设置 Area2D 的基础属性
			Monitoring = true;
			Monitorable = false;
		}
		
		/// <summary>
		/// 设置视觉效果
		/// </summary>
		private void SetupVisuals()
		{
			// 创建阳光纹理
			GenerateSunTexture();
			
			// 设置阴影效果
			_sunSprite.Modulate = new Color(1.2f, 1.0f, 0.2f, 1.0f); // 金黄色
		}
		
		/// <summary>
		/// 生成阳光纹理
		/// </summary>
		private void GenerateSunTexture()
		{
			var image = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
			var center = new Vector2I(16, 16);
			
			// 生成太阳形状
			for (int x = 0; x < 32; x++)
			{
				for (int y = 0; y < 32; y++)
				{
					var pos = new Vector2I(x, y);
					var dist = pos.DistanceTo(center);
					
					if (dist <= 12f) // 外圆半径
					{
						// 计算太阳光线角度
						var angle = Mathf.Atan2(y - center.Y, x - center.X);
						var rayAngle = Mathf.Round(angle / (Mathf.Pi / 4f)) * (Mathf.Pi / 4f); // 8条光线
						var rayDist = Mathf.Abs(Mathf.Cos(angle - rayAngle));
						
						// 根据距离调整透明度
						var alpha = dist <= 8f ? 1.0f : Mathf.Clamp((12f - dist) / 4f, 0f, 1f);
						
						// 创建金色渐变
						var brightness = 0.7f + rayDist * 0.3f;
						var color = new Color(brightness, brightness * 0.8f, brightness * 0.2f, alpha);
						
						// 添加内部纹理
						if (dist <= 4f)
						{
							color = new Color(1.0f, 0.95f, 0.3f, alpha);
						}
						
						image.SetPixel(x, y, color);
					}
				}
			}
			
			var texture = ImageTexture.CreateFromImage(image);
			_sunSprite.Texture = texture;
			
			// 设置碰撞形状
			var circleShape = new CircleShape2D();
			circleShape.Radius = 12f;
			_collisionShape.Shape = circleShape;
		}
		
		/// <summary>
		/// 设置碰撞检测
		/// </summary>
		private void SetupCollision()
		{
			// 设置碰撞层和掩码
			CollisionLayer = 4; // 阳光层
			CollisionMask = 1;   // 玩家鼠标层
		}
		
		/// <summary>
		/// 设置动画
		/// </summary>
		private void SetupAnimation()
		{
			// 创建统一的动画库
			var animLib = new AnimationLibrary();

			// 创建漂浮动画
			CreateFloatingAnimation(animLib);

			// 创建脉动动画
			CreatePulseAnimation(animLib);

			// 创建旋转动画
			CreateRotationAnimation(animLib);

			// 创建下落动画
			CreateFallAnimation(animLib);

			// 创建收集动画
			CreateCollectAnimation(animLib);

			// 创建过期动画
			CreateExpireAnimation(animLib);

			// 一次性添加动画库
			_animationPlayer.AddAnimationLibrary("sun_anim", animLib);
		}
		
		/// <summary>
		/// 创建漂浮动画
		/// </summary>
		private void CreateFloatingAnimation(AnimationLibrary animLib)
		{
			var animation = new Animation();
			animation.Length = Lifetime;
			animation.LoopMode = Animation.LoopModeEnum.None;

			var track = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(track, "position");

			// 漂浮运动轨迹
			for (float t = 0f; t <= 1f; t += 0.1f)
			{
				var time = t * Lifetime;
				var offset = new Vector2(
					Mathf.Sin(time * BobFrequency) * BobAmplitude,
					Mathf.Cos(time * BobFrequency * 0.7f) * BobAmplitude * 0.5f
				);
				animation.TrackInsertKey(track, time, offset);
			}

			animLib.AddAnimation("float", animation);
		}
		
		/// <summary>
		/// 创建脉动动画
		/// </summary>
		private void CreatePulseAnimation(AnimationLibrary animLib)
		{
			var animation = new Animation();
			animation.Length = 1.0f;
			animation.LoopMode = Animation.LoopModeEnum.None;

			var track = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(track, "scale");

			animation.TrackInsertKey(track, 0.0f, Vector2.One);
			animation.TrackInsertKey(track, 0.5f, Vector2.One * 1.1f);
			animation.TrackInsertKey(track, 1.0f, Vector2.One);

			animLib.AddAnimation("pulse", animation);
		}
		
		/// <summary>
		/// 创建旋转动画
		/// </summary>
		private void CreateRotationAnimation(AnimationLibrary animLib)
		{
			var animation = new Animation();
			animation.Length = 4.0f;
			animation.LoopMode = Animation.LoopModeEnum.Linear;

			var track = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(track, "rotation");

			animation.TrackInsertKey(track, 0.0f, 0f);
			animation.TrackInsertKey(track, 4.0f, Mathf.Tau); // 完整旋转一圈

			animLib.AddAnimation("rotate", animation);
		}
		
		/// <summary>
		/// 设置计时器
		/// </summary>
		private void SetupTimers()
		{
			_lifetimeTimer.WaitTime = Lifetime;
			_lifetimeTimer.Timeout += OnLifetimeExpired;
		}
		
		/// <summary>
		/// 开始阳光掉落
		/// </summary>
		/// <param name="startPos">起始位置</param>
		/// <param name="targetY">目标Y坐标（地面高度）</param>
		public void StartFalling(Vector2 startPos, float targetY)
		{
			Position = startPos;
			_targetPosition = new Vector2(startPos.X + (float)GD.RandRange(-50f, 50f), targetY);
			_fallTimer = 0f;
			_isGrounded = false;

			// 播放下落动画
			_animationPlayer.Play("sun_anim/fall");

			GD.Print($"[Sun] 阳光开始掉落: {startPos} -> {_targetPosition}");
		}
		
		/// <summary>
		/// 创建下落动画模板
		/// </summary>
		private void CreateFallAnimation(AnimationLibrary animLib)
		{
			// 创建一个简单的下落动画模板，实际位置会在StartFalling中动态更新
			var animation = new Animation();
			animation.Length = FallDuration;
			animation.LoopMode = Animation.LoopModeEnum.None;

			var track = animation.AddTrack(Animation.TrackType.Value);
			// 作用于阳光节点自身的位置，避免影响父节点UI
			animation.TrackSetPath(track, new NodePath("position"));

			// 默认的直线下落动画（会被动态更新）
			animation.TrackInsertKey(track, 0.0f, Vector2.Zero);
			animation.TrackInsertKey(track, FallDuration, Vector2.Down * 200);

			animLib.AddAnimation("fall", animation);
		}

		/// <summary>
		/// 创建收集动画
		/// </summary>
		private void CreateCollectAnimation(AnimationLibrary animLib)
		{
			var animation = new Animation();
			animation.Length = 0.3f;

			// 缩放消失动画 - 作用于阳光节点自身的缩放
			var scaleTrack = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(scaleTrack, new NodePath("scale"));
			animation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One);
			animation.TrackInsertKey(scaleTrack, 0.3f, Vector2.One * 2f);

			// 透明度消失动画
			var modulateTrack = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(modulateTrack, "SunSprite:modulate");
			animation.TrackInsertKey(modulateTrack, 0.0f, new Color(1.2f, 1.0f, 0.2f, 1.0f));
			animation.TrackInsertKey(modulateTrack, 0.3f, new Color(1.2f, 1.0f, 0.2f, 0.0f));

			animLib.AddAnimation("collect", animation);
		}

		/// <summary>
		/// 创建过期动画
		/// </summary>
		private void CreateExpireAnimation(AnimationLibrary animLib)
		{
			var animation = new Animation();
			animation.Length = 0.5f;

			// 缩小消失动画 - 作用于阳光节点自身的缩放
			var scaleTrack = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(scaleTrack, new NodePath("scale"));
			animation.TrackInsertKey(scaleTrack, 0.0f, Vector2.One);
			animation.TrackInsertKey(scaleTrack, 0.5f, Vector2.Zero);

			// 透明度消失动画
			var modulateTrack = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(modulateTrack, "SunSprite:modulate");
			animation.TrackInsertKey(modulateTrack, 0.0f, new Color(1.2f, 1.0f, 0.2f, 1.0f));
			animation.TrackInsertKey(modulateTrack, 0.5f, new Color(1.2f, 1.0f, 0.2f, 0.0f));

			animLib.AddAnimation("expire", animation);
		}
		
		public override void _Process(double delta)
		{
			if (_isCollected)
				return;
				
			if (!_isGrounded)
			{
				_fallTimer += (float)delta;
				if (_fallTimer >= FallDuration)
				{
					OnLanded();
				}
			}
		}
		
		/// <summary>
		/// 落地后的处理
		/// </summary>
		private void OnLanded()
		{
			_isGrounded = true;
			Position = _targetPosition;

			// 停止下落动画，开始漂浮动画
			_animationPlayer.Stop();
			_animationPlayer.Play("sun_anim/float");

			// 开始生命周期计时
			_lifetimeTimer.Start();

			// 播放落地音效（如果有的话）
			// TODO: 添加落地音效

			GD.Print($"[Sun] 阳光落地: {Position}");
		}
		
		/// <summary>
		/// 鼠标输入处理
		/// </summary>
		public override void _Input(InputEvent @event)
		{
			if (_isCollected || !_isGrounded)
				return;
				
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left)
				{
					var mousePos = GetGlobalMousePosition();
					var sunPos = GetGlobalPosition();
					var dist = mousePos.DistanceTo(sunPos);
					
					if (dist <= 20f) // 点击范围
					{
						Collect();
					}
				}
			}
		}
		
		/// <summary>
		/// 收集阳光
		/// </summary>
		public void Collect()
		{
			if (_isCollected)
				return;
				
			_isCollected = true;
			_lifetimeTimer.Stop();
			
			// 播放收集动画
			PlayCollectAnimation();
			
			// 发出收集信号
			EmitSignal(SignalName.SunCollected, SunValue);
			
			GD.Print($"[Sun] 阳光被收集: +{SunValue}");
		}
		
		/// <summary>
		/// 播放收集动画
		/// </summary>
		private void PlayCollectAnimation()
		{
			// 停止其他动画，播放收集动画
			_animationPlayer.Stop();
			_animationPlayer.Play("sun_anim/collect");

			// 动画结束后删除自己
			_animationPlayer.AnimationFinished += (animName) =>
			{
				if (animName == "sun_anim/collect")
				{
					QueueFree();
				}
			};
		}
		
		/// <summary>
		/// 生命周期结束处理
		/// </summary>
		private void OnLifetimeExpired()
		{
			if (_isCollected)
				return;
				
			EmitSignal(SignalName.SunExpired);
			PlayExpireAnimation();
		}
		
		/// <summary>
		/// 播放过期动画
		/// </summary>
		private void PlayExpireAnimation()
		{
			// 停止其他动画，播放过期动画
			_animationPlayer.Stop();
			_animationPlayer.Play("sun_anim/expire");

			// 动画结束后删除自己
			_animationPlayer.AnimationFinished += (animName) =>
			{
				if (animName == "sun_anim/expire")
				{
					QueueFree();
				}
			};
		}
	}
}
