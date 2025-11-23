using Godot;

namespace PlantsVsZombies.Combat
{
	/// <summary>
	/// 樱桃炸弹 - 特殊的爆炸植物
	/// 实现樱桃炸弹的独特爆炸逻辑和延迟机制
	/// </summary>
	public partial class CherryBomb : Node2D
	{
		// 樱桃炸弹属性
		[Export] public int ExplosionDamage { get; set; } = 1800;  // 樱桃炸弹高伤害
		[Export] public float ExplosionRadius { get; set; } = 150f; // 爆炸半径
		[Export] public float FuseTime { get; set; } = 2.0f;       // 引信时间
		[Export] public int Cost { get; set; } = 150;              // 阳光消耗
		
		// 状态变量
		private bool _isPlanted = false;
		private bool _isExploding = false;
		private float _currentFuseTime = 0f;
		private Sprite2D _sprite;
		private AnimationPlayer _animationPlayer;
		private AudioStreamPlayer2D _audioPlayer;
		
		// 信号
		[Signal] public delegate void OnExplodedEventHandler(Vector2 position);
		[Signal] public delegate void OnFuseStartedEventHandler();
		
		public override void _Ready()
		{
			InitializeComponents();
		}
		
		/// <summary>
		/// 初始化组件
		/// </summary>
		private void InitializeComponents()
		{
			// 创建精灵组件
			_sprite = new Sprite2D();
			AddChild(_sprite);
			
			// 创建动画播放器
			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);
			CreateAnimations();
			
			// 创建音频播放器
			_audioPlayer = new AudioStreamPlayer2D();
			AddChild(_audioPlayer);
			
			// 设置初始状态
			UpdateVisualState();
		}
		
		/// <summary>
		/// 创建动画
		/// </summary>
		private void CreateAnimations()
		{
			// 创建摇摆动画（种植后等待爆炸时）
			var swayAnimation = new Animation();
			swayAnimation.Length = 1.0f;
			swayAnimation.LoopMode = Animation.LoopModeEnum.Linear;
			
			var scaleTrack = swayAnimation.AddTrack(Animation.TrackType.Scale3D);
			swayAnimation.TrackSetPath(scaleTrack, ":scale");
			
			// 添加关键帧实现摇摆效果
			swayAnimation.TrackInsertKey(scaleTrack, 0.0f, Vector3.One * 0.9f);
			swayAnimation.TrackInsertKey(scaleTrack, 0.5f, Vector3.One * 1.1f);
			swayAnimation.TrackInsertKey(scaleTrack, 1.0f, Vector3.One * 0.9f);
			
			var swayLibrary = new AnimationLibrary();
 swayLibrary.AddAnimation("sway", swayAnimation);
 _animationPlayer.AddAnimationLibrary("sway_library", swayLibrary);
			
			// 创建爆炸动画
			var explodeAnimation = new Animation();
			explodeAnimation.Length = 0.5f;
			explodeAnimation.LoopMode = Animation.LoopModeEnum.None;
			
			var explodeScaleTrack = explodeAnimation.AddTrack(Animation.TrackType.Scale3D);
			explodeAnimation.TrackSetPath(explodeScaleTrack, ":scale");
			
			explodeAnimation.TrackInsertKey(explodeScaleTrack, 0.0f, Vector3.One);
			explodeAnimation.TrackInsertKey(explodeScaleTrack, 0.3f, Vector3.One * 1.5f);
			explodeAnimation.TrackInsertKey(explodeScaleTrack, 0.5f, Vector3.Zero);
			
			var explodeLibrary = new AnimationLibrary();
 explodeLibrary.AddAnimation("explode", explodeAnimation);
 _animationPlayer.AddAnimationLibrary("explode_library", explodeLibrary);
		}
		
		public override void _Process(double delta)
		{
			if (!_isPlanted || _isExploding)
				return;
			
			// 更新引信时间
			_currentFuseTime += (float)delta;
			
			// 检查是否该爆炸
			if (_currentFuseTime >= FuseTime)
			{
				Explode();
			}
			else
			{
				// 在最后0.5秒时快速闪烁警告
				if (FuseTime - _currentFuseTime <= 0.5f)
				{
					FlashWarning();
				}
			}
		}
		
		/// <summary>
		/// 种植樱桃炸弹
		/// </summary>
		/// <param name="position">种植位置</param>
		public void Plant(Vector2 position)
		{
			if (_isPlanted)
				return;
			
			GlobalPosition = position;
			_isPlanted = true;
			_currentFuseTime = 0f;
			
			// 播放摇摆动画
			_animationPlayer.Play("sway");
			
			// 发出引信开始信号
			EmitSignal(SignalName.OnFuseStarted);
			
			GD.Print($"[CherryBomb] 樱桃炸弹种植: 位置={position}, 引信时间={FuseTime}秒");
		}
		
		/// <summary>
		/// 触发爆炸
		/// </summary>
		public void Explode()
		{
			if (_isExploding)
				return;
			
			_isExploding = true;
			
			// 停止摇摆动画，播放爆炸动画
			_animationPlayer.Stop();
			_animationPlayer.Play("explode");
			
			// 播放爆炸音效
			PlayExplosionSound();
			
			// 创建爆炸效果
			CreateCherryExplosion();
			
			// 发出爆炸信号
			EmitSignal(SignalName.OnExploded, GlobalPosition);
			
			// 延迟销毁对象
			GetTree().CreateTimer(0.6f).Timeout += () => QueueFree();
			
			GD.Print($"[CherryBomb] 樱桃炸弹爆炸: 位置={GlobalPosition}, 伤害={ExplosionDamage}, 半径={ExplosionRadius}");
		}
		
		/// <summary>
		/// 创建樱桃炸弹的特殊爆炸效果
		/// </summary>
		private void CreateCherryExplosion()
		{
			// 通过战斗管理器创建爆炸
			if (CombatManager.Instance != null)
			{
				CombatManager.Instance.CreateExplosion(
					GlobalPosition, 
					ExplosionRadius, 
					ExplosionDamage, 
					ExplosionType.Cherry
				);
			}
			
			// 创建额外的视觉效果（二次小爆炸）
			GetTree().CreateTimer(0.1f).Timeout += CreateSecondaryExplosion;
		}
		
		/// <summary>
		/// 创建二次小爆炸效果
		/// </summary>
		private void CreateSecondaryExplosion()
		{
			if (CombatManager.Instance != null)
			{
				// 在周围创建2个小爆炸
				var offset1 = new Vector2(50, 0);
				var offset2 = new Vector2(-50, 0);
				
				CombatManager.Instance.CreateExplosion(
					GlobalPosition + offset1, 
					ExplosionRadius * 0.5f, 
					ExplosionDamage / 4, 
					ExplosionType.Splash
				);
				
				CombatManager.Instance.CreateExplosion(
					GlobalPosition + offset2, 
					ExplosionRadius * 0.5f, 
					ExplosionDamage / 4, 
					ExplosionType.Splash
				);
			}
		}
		
		/// <summary>
		/// 播放爆炸音效
		/// </summary>
		private void PlayExplosionSound()
		{
			// TODO: 添加爆炸音效
			// _audioPlayer.Stream = GD.Load<AudioStream>("res://Sounds/explosion.wav");
			// _audioPlayer.Play();
		}
		
		/// <summary>
		/// 闪烁警告效果
		/// </summary>
		private void FlashWarning()
		{
			// 快速闪烁红光警告即将爆炸
			var flashColor = Mathf.Sin(Time.GetTicksMsec() * 0.02f) > 0 ? Colors.White : Colors.Red;
			_sprite.Modulate = flashColor;
		}
		
		/// <summary>
		/// 更新视觉状态
		/// </summary>
		private void UpdateVisualState()
		{
			// 创建简单的圆形表示樱桃炸弹
			var circle = new CircleShape2D();
			circle.Radius = 30;
			
			// 设置颜色（樱桃红色）
			_sprite.Modulate = Colors.DarkRed;
		}
		
		/// <summary>
		/// 获取剩余引信时间
		/// </summary>
		/// <returns>剩余时间（秒）</returns>
		public float GetRemainingFuseTime()
		{
			if (!_isPlanted || _isExploding)
				return 0f;
			
			return Mathf.Max(0f, FuseTime - _currentFuseTime);
		}
		
		/// <summary>
		/// 是否即将爆炸（小于1秒）
		/// </summary>
		/// <returns>是否即将爆炸</returns>
		public bool IsAboutToExplode()
		{
			return GetRemainingFuseTime() <= 1.0f;
		}
		
		/// <summary>
		/// 手动触发爆炸（用于调试或特殊情况）
		/// </summary>
		public void ForceExplode()
		{
			if (_isPlanted && !_isExploding)
			{
				Explode();
			}
		}
	}
}