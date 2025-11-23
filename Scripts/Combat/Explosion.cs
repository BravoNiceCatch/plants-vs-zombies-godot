using Godot;

namespace PlantsVsZombies.Combat
{
	/// <summary>
	/// 爆炸效果类 - 处理爆炸的逻辑和状态
	/// </summary>
	public partial class Explosion : Node
	{
		// 爆炸属性
		public Vector2 Position { get; private set; }
		public float Radius { get; private set; }
		public int Damage { get; private set; }
		public ExplosionType ExplosionType { get; private set; }
		
		// 爆炸状态
		private float _duration = 1.0f;  // 爆炸持续时间
		private float _currentTime = 0f;
		private bool _isFinished = false;
		
		// 爆炸伤害范围（可以随时间变化）
		private float _currentRadius;
		private float _maxRadius;
		
		public bool IsFinished => _isFinished;
		public float CurrentRadius => _currentRadius;
		public float Progress => _currentTime / _duration;
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="position">爆炸位置</param>
		/// <param name="radius">爆炸半径</param>
		/// <param name="damage">爆炸伤害</param>
		/// <param name="explosionType">爆炸类型</param>
		public Explosion(Vector2 position, float radius, int damage, ExplosionType explosionType)
		{
			Position = position;
			Radius = radius;
			Damage = damage;
			ExplosionType = explosionType;
			
			// 根据爆炸类型设置不同参数
			SetupExplosionType();
			
			_currentRadius = 0f; // 爆炸从0半径开始扩散
			_maxRadius = radius;
		}
		
		/// <summary>
		/// 根据爆炸类型设置参数
		/// </summary>
		private void SetupExplosionType()
		{
			switch (ExplosionType)
			{
				case ExplosionType.Cherry:
					_duration = 1.5f;  // 樱桃炸弹爆炸持续时间较长
					break;
				case ExplosionType.Splash:
					_duration = 0.8f;  // 溅射伤害持续时间较短
					break;
				case ExplosionType.Normal:
				default:
					_duration = 1.0f;  // 普通爆炸
					break;
			}
		}
		
		/// <summary>
		/// 更新爆炸状态
		/// </summary>
		/// <param name="delta">时间增量</param>
		public void Update(float delta)
		{
			if (_isFinished)
				return;
			
			_currentTime += delta;
			
			// 更新爆炸半径（使用缓动函数让扩散更自然）
			_currentRadius = Mathf.Lerp(0f, _maxRadius, GetEasedProgress());
			
			// 检查是否完成
			if (_currentTime >= _duration)
			{
				_currentTime = _duration;
				_currentRadius = _maxRadius;
				_isFinished = true;
				OnExplosionFinished();
			}
		}
		
		/// <summary>
		/// 获取缓动的进度值（用于平滑动画）
		/// </summary>
		/// <returns>缓动后的进度值（0-1）</returns>
		private float GetEasedProgress()
		{
			var t = Progress;
			
			// 使用二次缓出函数，让爆炸开始时快速扩散，然后减慢
			return 1f - Mathf.Pow(1f - t, 3f);
		}
		
		/// <summary>
		/// 检查点是否在爆炸范围内
		/// </summary>
		/// <param name="point">要检查的点</param>
		/// <returns>是否在范围内</returns>
		public bool IsPointInRange(Vector2 point)
		{
			var distance = point.DistanceTo(Position);
			return distance <= _currentRadius;
		}
		
		/// <summary>
		/// 获取某点受到的伤害（考虑距离衰减）
		/// </summary>
		/// <param name="point">目标点</param>
		/// <returns>受到的伤害值</returns>
		public int GetDamageAtPoint(Vector2 point)
		{
			if (!IsPointInRange(point))
				return 0;
			
			var distance = point.DistanceTo(Position);
			if (distance <= 0f)
				return Damage;
			
			// 距离越远伤害越低
			var damageMultiplier = 1.0f - (distance / _currentRadius) * 0.5f;
			return (int)(Damage * damageMultiplier);
		}
		
		/// <summary>
		/// 爆炸完成时调用
		/// </summary>
		private void OnExplosionFinished()
		{
			GD.Print($"[Explosion] 爆炸完成: 类型={ExplosionType}, 位置={Position}, 总伤害={Damage}");
		}
		
		/// <summary>
		/// 获取爆炸信息字符串
		/// </summary>
		/// <returns>爆炸信息</returns>
		public override string ToString()
		{
			return $"爆炸[{ExplosionType}] 位置:{Position} 半径:{_currentRadius:F1}/{_maxRadius} " +
			       $"伤害:{Damage} 进度:{Progress:P1}";
		}
	}
}