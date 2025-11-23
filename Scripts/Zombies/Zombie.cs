using Godot;
using PlantsVsZombies.Plants;

namespace PlantsVsZombies.Zombies
{
	public partial class Zombie : Node2D
	{
		[Export] public float MoveSpeed { get; protected set; } = 26.67f; // 1格/3秒 (80像素/3秒)
		[Export] public int MaxHealth { get; protected set; } = 100;
		[Export] public int Damage { get; protected set; } = 10; // 每秒10点伤害
		[Export] public float AttackRange { get; protected set; } = 40f; // 攻击范围
		[Export] public float AttackCooldown { get; protected set; } = 1.5f; // 攻击冷却时间

		public int CurrentHealth { get; protected set; }
		public bool IsAttacking { get; protected set; }
		public bool IsDying { get; protected set; }

		protected Plant currentTarget;
		protected Timer attackTimer;
		protected Timer damageTimer;
		protected AnimationPlayer animationPlayer;
		private Sprite2D sprite;
		private CollisionShape2D collisionShape;
		private Area2D detectionArea;

		public override void _Ready()
		{
			CurrentHealth = MaxHealth;
			IsAttacking = false;
			IsDying = false;

			SetupComponents();
			SetupMovement();
			DrawZombieShape();
		}

		public override void _Process(double delta)
		{
			if (IsDying) return;

			if (!IsAttacking)
			{
				MoveForward((float)delta);
			}

			CheckForPlants();
		}

		protected virtual void SetupComponents()
		{
			// 创建精灵
			sprite = new Sprite2D();
			AddChild(sprite);

			// 创建碰撞体
			var collisionArea = new Area2D();
			collisionShape = new CollisionShape2D();
			var shape = new RectangleShape2D();
			shape.Size = new Vector2(30, 50);
			collisionShape.Shape = shape;
			collisionArea.AddChild(collisionShape);
			AddChild(collisionArea);

			// 创建检测区域（用于检测植物）
			detectionArea = new Area2D();
			var detectionShape = new CollisionShape2D();
			var detectionCircle = new CircleShape2D();
			detectionCircle.Radius = AttackRange;
			detectionShape.Shape = detectionCircle;
			detectionArea.AddChild(detectionShape);
			AddChild(detectionArea);

			// 创建攻击计时器
			attackTimer = new Timer();
			attackTimer.WaitTime = AttackCooldown;
			attackTimer.OneShot = false;
			attackTimer.Timeout += OnAttackTimerTimeout;
			AddChild(attackTimer);

			// 创建伤害计时器（用于攻击植物）
			damageTimer = new Timer();
			damageTimer.WaitTime = 1.0f; // 每秒造成伤害
			damageTimer.OneShot = false;
			damageTimer.Timeout += OnDamageTimerTimeout;
			AddChild(damageTimer);

			// 创建动画播放器
			animationPlayer = new AnimationPlayer();
			AddChild(animationPlayer);
		}

		protected virtual void MoveForward(float delta)
		{
			Vector2 newPosition = Position;
			newPosition.X -= MoveSpeed * delta;
			Position = newPosition;
		}

		protected virtual void CheckForPlants()
		{
			if (currentTarget == null || !IsInstanceValid(currentTarget))
			{
				currentTarget = FindPlantInFront();
				if (currentTarget != null && !IsAttacking)
				{
					StartAttacking(currentTarget);
				}
			}
			else
			{
				// 检查当前目标是否还在范围内
				float distance = GlobalPosition.DistanceTo(currentTarget.GlobalPosition);
				if (distance > AttackRange)
				{
					StopAttacking();
					currentTarget = null;
				}
			}
		}

		protected virtual Plant FindPlantInFront()
		{
			var overlappedBodies = detectionArea.GetOverlappingBodies();
			foreach (Node body in overlappedBodies)
			{
				if (body is Plant plant && plant.Position.X < Position.X)
				{
					return plant;
				}
			}
			return null;
		}

		protected virtual void StartAttacking(Plant plant)
		{
			IsAttacking = true;
			currentTarget = plant;
			attackTimer.Start();
			damageTimer.Start();
			PlayAttackAnimation();
		}

		protected virtual void StopAttacking()
		{
			IsAttacking = false;
			currentTarget = null;
			attackTimer.Stop();
			damageTimer.Stop();
			PlayWalkAnimation();
		}

		protected virtual void OnAttackTimerTimeout()
		{
			if (currentTarget != null && IsInstanceValid(currentTarget))
			{
				// 攻击动画完成时的回调
			}
		}

		protected virtual void OnDamageTimerTimeout()
		{
			if (currentTarget != null && IsInstanceValid(currentTarget))
			{
				currentTarget.TakeDamage(Damage);
			}
		}

		public virtual void TakeDamage(int damage)
		{
			if (IsDying) return;

			CurrentHealth -= damage;
			PlayHitEffect();

			if (CurrentHealth <= 0)
			{
				Die();
			}
		}

		protected virtual void Die()
		{
			IsDying = true;
			IsAttacking = false;
			attackTimer.Stop();
			damageTimer.Stop();

			// 通知GameManager增加击杀数
			if (GameManager.Instance != null)
			{
				GameManager.Instance.IncrementKillCount();
			}

			PlayDeathAnimation();
			SpawnDeathEffect();

			// 延迟销毁，给死亡动画时间播放
			var deathTimer = new Timer();
			deathTimer.WaitTime = 1.0f;
			deathTimer.OneShot = true;
			deathTimer.Timeout += () => QueueFree();
			AddChild(deathTimer);
			deathTimer.Start();
		}

		protected virtual void DrawZombieShape()
		{
			// 使用几何图形绘制僵尸
			// 头部：灰色圆形
			// 身体：灰色矩形
			// 手臂：小矩形
		}

		protected virtual void PlayWalkAnimation()
		{
			// 播放行走动画
		}

		protected virtual void PlayAttackAnimation()
		{
			// 播放攻击动画
		}

		protected virtual void PlayDeathAnimation()
		{
			// 播放死亡动画
		}

		protected virtual void PlayHitEffect()
		{
			// 播放受击效果
			// 闪烁红色
			var tween = CreateTween();
			tween.TweenProperty(sprite, "modulate", Colors.Red, 0.1f);
			tween.TweenProperty(sprite, "modulate", Colors.White, 0.1f);
		}

		protected virtual void SpawnDeathEffect()
		{
			// 生成死亡特效
			var deathEffect = new Node2D();
			deathEffect.Position = Position;

			// 简单的死亡特效：缩放消失
			var tween = CreateTween();
			tween.TweenProperty(deathEffect, "scale", Vector2.Zero, 0.5f);
			tween.TweenCallback(Callable.From(() => deathEffect.QueueFree()));

			GetParent().AddChild(deathEffect);
		}

		public override void _ExitTree()
		{
			// 清理
			if (attackTimer != null)
			{
				attackTimer.Timeout -= OnAttackTimerTimeout;
			}
			if (damageTimer != null)
			{
				damageTimer.Timeout -= OnDamageTimerTimeout;
			}
		}
	}
}