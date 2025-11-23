using Godot;
using PlantsVsZombies.Combat;
using PlantsVsZombies.Core;

namespace PlantsVsZombies.Effects
{
	/// <summary>
	/// 爆炸视觉效果 - 负责显示爆炸的视觉和特效
	/// </summary>
	public partial class ExplosionEffect : Node2D
	{
		// 视觉效果组件
		private Sprite2D _explosionSprite;
		private Sprite2D _shockwaveSprite;
		private GpuParticles2D _particleSystem;
		// private Light2D _explosionLight; // 暂时禁用
		
		// 效果参数
		private float _radius = 100f;
		private ExplosionType _explosionType = ExplosionType.Normal;
		private float _duration = 1.0f;
		private float _currentTime = 0f;
		private bool _isPlaying = false;
		
		// 颜色配置
		private readonly Color _cherryBombColor = new Color(1.0f, 0.3f, 0.0f, 1.0f); // 橙红色
		private readonly Color _normalExplosionColor = new Color(1.0f, 0.8f, 0.2f, 1.0f); // 黄色
		private readonly Color _splashColor = new Color(0.8f, 0.9f, 1.0f, 1.0f); // 浅蓝色
		
		public override void _Ready()
		{
			InitializeEffects();
		}
		
		public override void _Process(double delta)
		{
			if (!_isPlaying)
				return;
			
			_currentTime += (float)delta;
			UpdateEffects();
			
			// 检查是否播放完成
			if (_currentTime >= _duration)
			{
				StopEffects();
			}
		}
		
		/// <summary>
		/// 初始化视觉效果组件
		/// </summary>
		private void InitializeEffects()
		{
			// 创建爆炸主精灵
			_explosionSprite = new Sprite2D();
			_explosionSprite.Texture = CreateExplosionTexture();
			_explosionSprite.Modulate = _normalExplosionColor;
			AddChild(_explosionSprite);
			
			// 创建冲击波精灵
			_shockwaveSprite = new Sprite2D();
			_shockwaveSprite.Texture = CreateShockwaveTexture();
			_shockwaveSprite.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			AddChild(_shockwaveSprite);
			
			// 创建粒子系统
			CreateParticleSystem();
			
			// 创建光源
			CreateExplosionLight();
		}
		
		/// <summary>
		/// 创建爆炸纹理
		/// </summary>
		/// <returns>爆炸纹理</returns>
		private ImageTexture CreateExplosionTexture()
		{
			var imageSize = 256;
			var image = Image.CreateEmpty(imageSize, imageSize, false, Image.Format.Rgba8);
			
			// 创建径向渐变爆炸效果
			var center = new Vector2I(imageSize / 2, imageSize / 2);
			var maxRadius = imageSize / 2;
			
			for (int y = 0; y < imageSize; y++)
			{
				for (int x = 0; x < imageSize; x++)
				{
					var pos = new Vector2I(x, y);
					var distance = pos.DistanceTo(center);
					
					if (distance <= maxRadius)
					{
						var intensity = 1.0f - (distance / maxRadius);
						var color = new Color(1.0f, 1.0f, 1.0f, intensity);
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
		/// 创建冲击波纹理
		/// </summary>
		/// <returns>冲击波纹理</returns>
		private ImageTexture CreateShockwaveTexture()
		{
			var imageSize = 256;
			var image = Image.CreateEmpty(imageSize, imageSize, false, Image.Format.Rgba8);
			
			// 创建环形冲击波效果
			var center = new Vector2I(imageSize / 2, imageSize / 2);
			var outerRadius = imageSize / 2;
			var innerRadius = outerRadius - 10;
			
			for (int y = 0; y < imageSize; y++)
			{
				for (int x = 0; x < imageSize; x++)
				{
					var pos = new Vector2I(x, y);
					var distance = pos.DistanceTo(center);
					
					if (distance >= innerRadius && distance <= outerRadius)
					{
						var intensity = 1.0f - Mathf.Abs(distance - (innerRadius + outerRadius) / 2) / 5.0f;
						var color = new Color(1.0f, 1.0f, 1.0f, intensity * 0.7f);
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
		/// 创建粒子系统
		/// </summary>
		private void CreateParticleSystem()
		{
			_particleSystem = new GpuParticles2D();
			_particleSystem.OneShot = true;
			_particleSystem.Emitting = false;
			
			// 设置粒子材质
			var particleMaterial = new ParticleProcessMaterial();
			particleMaterial.Direction = Vector3.Zero;
			particleMaterial.Spread = Mathf.Pi;
			particleMaterial.InitialVelocityMin = 50.0f;
			particleMaterial.InitialVelocityMax = 200.0f;
			particleMaterial.LinearAccel = Vector2.Zero;
			particleMaterial.Color = _normalExplosionColor;
			particleMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
			particleMaterial.EmissionSphereRadius = 20.0f;
			
			_particleSystem.ProcessMaterial = particleMaterial;
			
			// 设置粒子纹理
			var particleImage = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
			particleImage.Fill(Colors.White);
			var particleTexture = ImageTexture.CreateFromImage(particleImage);
			_particleSystem.Texture = particleTexture;
			
			AddChild(_particleSystem);
		}
		
		/// <summary>
		/// 创建爆炸光源
		/// </summary>
		private void CreateExplosionLight()
		{
			// Light2D 在 Godot 4.x 中暂时禁用
			// 如果需要光源效果，可以后续实现
		}
		
		/// <summary>
		/// 初始化爆炸效果
		/// </summary>
		/// <param name="radius">爆炸半径</param>
		/// <param name="explosionType">爆炸类型</param>
		public void Initialize(float radius, ExplosionType explosionType)
		{
			_radius = radius;
			_explosionType = explosionType;
			
			// 根据类型设置参数
			switch (explosionType)
			{
				case ExplosionType.Cherry:
					_duration = 1.5f;
					_explosionSprite.Modulate = _cherryBombColor;
					break;
				case ExplosionType.Splash:
					_duration = 0.8f;
					_explosionSprite.Modulate = _splashColor;
					break;
				case ExplosionType.Normal:
				default:
					_duration = 1.0f;
					_explosionSprite.Modulate = _normalExplosionColor;
					break;
			}
			
			// 设置初始状态
			_currentTime = 0f;
			_isPlaying = true;
			Scale = Vector2.Zero; // 从0开始放大
			
			// 开始播放效果
			StartEffects();
		}
		
		/// <summary>
		/// 开始播放效果
		/// </summary>
		private void StartEffects()
		{
			// 光源效果暂时禁用
			/*_explosionLight.Enabled = true;
			_explosionLight.Energy = 3.0f;

			// 根据爆炸类型设置光源颜色
			switch (_explosionType)
			{
				case ExplosionType.Cherry:
					_explosionLight.Color = _cherryBombColor;
					break;
				case ExplosionType.Splash:
					_explosionLight.Color = _splashColor;
					break;
				case ExplosionType.Normal:
				default:
					_explosionLight.Color = Colors.White;
					break;
			}*/
			
			// 开始粒子效果
			_particleSystem.Emitting = true;
			
			// 震动效果（如果有游戏管理器）
			if (GameManager.Instance != null && GameManager.Instance.HasMethod("ShakeCamera"))
			{
				GameManager.Instance.Call("ShakeCamera", 0.5f, 10.0f);
			}
		}
		
		/// <summary>
		/// 更新效果
		/// </summary>
		private void UpdateEffects()
		{
			var progress = _currentTime / _duration;
			
			// 更新缩放（从小到大）
			var targetScale = _radius / 50.0f; // 基础半径为50
			var currentScale = Mathf.Lerp(0f, targetScale, GetEasedProgress(progress));
			Scale = new Vector2(currentScale, currentScale);
			
			// 更新透明度（逐渐消失）
			var alpha = Mathf.Lerp(1.0f, 0.0f, progress);
			_explosionSprite.Modulate = new Color(_explosionSprite.Modulate.R, _explosionSprite.Modulate.G, _explosionSprite.Modulate.B, alpha);
			
			// 更新冲击波
			_shockwaveSprite.Modulate = new Color(1.0f, 1.0f, 1.0f, alpha * 0.5f);
			_shockwaveSprite.Scale = new Vector2(1.0f + progress * 2.0f, 1.0f + progress * 2.0f);
			
			// 更新光源强度（逐渐减弱）- 暂时禁用
			// _explosionLight.Energy = Mathf.Lerp(3.0f, 0.0f, progress * progress);
			
			// 粒子效果会自动停止
			if (progress >= 0.6f)
			{
				_particleSystem.Emitting = false;
			}
		}
		
		/// <summary>
		/// 获取缓动进度
		/// </summary>
		/// <param name="t">进度值(0-1)</param>
		/// <returns>缓动后的进度值</returns>
		private float GetEasedProgress(float t)
		{
			// 使用缓出函数，让效果更自然
			return 1.0f - Mathf.Pow(1.0f - t, 3.0f);
		}
		
		/// <summary>
		/// 停止效果并清理
		/// </summary>
		private void StopEffects()
		{
			_isPlaying = false;
			_particleSystem.Emitting = false;
			// _explosionLight.Enabled = false; // 暂时禁用
			
			// 延迟销毁对象，确保效果完全播放完
			GetTree().CreateTimer(0.2f).Timeout += QueueFree;
		}
	}
}
