using Godot;

namespace PlantsVsZombies.Zombies
{
	public partial class BasicZombie : Zombie
	{
		private Sprite2D sprite;

		public BasicZombie()
		{
			MoveSpeed = 26.67f; // 1格/3秒 (80像素/3秒)
			MaxHealth = 100;
			Damage = 10; // 每秒10点伤害
			AttackRange = 40f;
			AttackCooldown = 1.5f;

			// 初始化精灵
			sprite = new Sprite2D();
			AddChild(sprite);
		}

		protected override void DrawZombieShape()
		{
			// 创建僵尸的几何图形外观

			// 头部：浅灰色圆形
			var headPosition = new Vector2(0, -15);
			var headRadius = 12f;
			DrawHead(headPosition, headRadius);

			// 身体：深灰色矩形
			var bodyPosition = new Vector2(0, 5);
			var bodySize = new Vector2(25, 30);
			DrawBody(bodyPosition, bodySize);

			// 手臂：小矩形
			DrawArms(bodyPosition, bodySize);

			// 腿部：细长矩形
			DrawLegs(bodyPosition, bodySize);

			// 眼睛：红色小圆形
			DrawEyes(headPosition, headRadius);
		}

		private void DrawHead(Vector2 position, float radius)
		{
			// 头部使用浅灰色圆形
			var headTexture = CreateCircleTexture(radius * 2, Colors.LightGray);
			sprite.Texture = headTexture;
			sprite.Position = position;
		}

		private void DrawBody(Vector2 position, Vector2 size)
		{
			// 身体使用深灰色矩形
			var bodyTexture = CreateRectangleTexture(size, Colors.DarkGray);
			var bodySprite = new Sprite2D();
			bodySprite.Texture = bodyTexture;
			bodySprite.Position = position;
			AddChild(bodySprite);
		}

		private void DrawArms(Vector2 bodyPosition, Vector2 bodySize)
		{
			// 左臂
			var leftArmTexture = CreateRectangleTexture(new Vector2(8, 20), Colors.Gray);
			var leftArmSprite = new Sprite2D();
			leftArmSprite.Texture = leftArmTexture;
			leftArmSprite.Position = new Vector2(bodyPosition.X - bodySize.X/2 - 5, bodyPosition.Y - 5);
			leftArmSprite.Rotation = -0.3f; // 稍微抬起
			AddChild(leftArmSprite);

			// 右臂
			var rightArmTexture = CreateRectangleTexture(new Vector2(8, 20), Colors.Gray);
			var rightArmSprite = new Sprite2D();
			rightArmSprite.Texture = rightArmTexture;
			rightArmSprite.Position = new Vector2(bodyPosition.X + bodySize.X/2 + 5, bodyPosition.Y - 5);
			rightArmSprite.Rotation = 0.3f; // 稍微抬起
			AddChild(rightArmSprite);
		}

		private void DrawLegs(Vector2 bodyPosition, Vector2 bodySize)
		{
			// 左腿
			var leftLegTexture = CreateRectangleTexture(new Vector2(6, 15), Colors.DarkSlateGray);
			var leftLegSprite = new Sprite2D();
			leftLegSprite.Texture = leftLegTexture;
			leftLegSprite.Position = new Vector2(bodyPosition.X - 8, bodyPosition.Y + bodySize.Y/2 + 5);
			AddChild(leftLegSprite);

			// 右腿
			var rightLegTexture = CreateRectangleTexture(new Vector2(6, 15), Colors.DarkSlateGray);
			var rightLegSprite = new Sprite2D();
			rightLegSprite.Texture = rightLegTexture;
			rightLegSprite.Position = new Vector2(bodyPosition.X + 8, bodyPosition.Y + bodySize.Y/2 + 5);
			AddChild(rightLegSprite);
		}

		private void DrawEyes(Vector2 headPosition, float headRadius)
		{
			// 左眼
			var leftEyeTexture = CreateCircleTexture(4, Colors.Red);
			var leftEyeSprite = new Sprite2D();
			leftEyeSprite.Texture = leftEyeTexture;
			leftEyeSprite.Position = new Vector2(headPosition.X - 5, headPosition.Y - 2);
			AddChild(leftEyeSprite);

			// 右眼
			var rightEyeTexture = CreateCircleTexture(4, Colors.Red);
			var rightEyeSprite = new Sprite2D();
			rightEyeSprite.Texture = rightEyeTexture;
			rightEyeSprite.Position = new Vector2(headPosition.X + 5, headPosition.Y - 2);
			AddChild(rightEyeSprite);
		}

		private ImageTexture CreateCircleTexture(float diameter, Color color)
		{
			var image = Image.CreateEmpty((int)diameter, (int)diameter, false, Image.Format.Rgba8);
			image.Fill(color);

			// 创建圆形遮罩
			var center = new Vector2I((int)diameter / 2, (int)diameter / 2);
			var radius = (int)diameter / 2;

			for (int x = 0; x < diameter; x++)
			{
				for (int y = 0; y < diameter; y++)
				{
					var distance = Mathf.Sqrt(Mathf.Pow(x - center.X, 2) + Mathf.Pow(y - center.Y, 2));
					if (distance > radius)
					{
						image.SetPixel(x, y, Colors.Transparent);
					}
				}
			}

			var texture = ImageTexture.CreateFromImage(image);
			return texture;
		}

		private ImageTexture CreateRectangleTexture(Vector2 size, Color color)
		{
			var image = Image.CreateEmpty((int)size.X, (int)size.Y, false, Image.Format.Rgba8);
			image.Fill(color);

			// 添加边框
			for (int x = 0; x < size.X; x++)
			{
				image.SetPixel(x, 0, Colors.Black);
				image.SetPixel(x, (int)size.Y - 1, Colors.Black);
			}

			for (int y = 0; y < size.Y; y++)
			{
				image.SetPixel(0, y, Colors.Black);
				image.SetPixel((int)size.X - 1, y, Colors.Black);
			}

			var texture = ImageTexture.CreateFromImage(image);
			return texture;
		}

		protected override void PlayWalkAnimation()
		{
			// 基础僵尸的行走动画：轻微上下浮动
			var tween = CreateTween();
			tween.SetLoops();
			tween.TweenProperty(this, "position:y", Position.Y + 2, 0.3f);
			tween.TweenProperty(this, "position:y", Position.Y - 2, 0.3f);
		}

		protected override void PlayAttackAnimation()
		{
			// 基础僵尸的攻击动画：前倾
			var tween = CreateTween();
			tween.TweenProperty(this, "rotation_degrees", 15, 0.2f);
			tween.TweenProperty(this, "rotation_degrees", 0, 0.2f);
		}

		protected override void PlayDeathAnimation()
		{
			// 基础僵尸的死亡动画：旋转倒下
			var tween = CreateTween();
			tween.TweenProperty(this, "rotation_degrees", 90, 0.5f);
			tween.TweenProperty(this, "modulate:a", 0.5f, 0.5f);
			tween.Parallel().TweenProperty(this, "position:y", Position.Y + 30, 0.5f);
		}

		protected override void PlayHitEffect()
		{
			// 基础僵尸的受击效果：红色闪烁和轻微后退
			base.PlayHitEffect();

			var tween = CreateTween();
			tween.TweenProperty(this, "position:x", Position.X + 5, 0.1f);
			tween.TweenProperty(this, "position:x", Position.X, 0.1f);
		}

		protected override void SpawnDeathEffect()
		{
			base.SpawnDeathEffect();

			// 添加僵尸特有的死亡效果：小方块飞散
			for (int i = 0; i < 5; i++)
			{
				var particle = CreateDeathParticle();
				GetParent().AddChild(particle);
			}
		}

		private Sprite2D CreateDeathParticle()
		{
			var particle = new Sprite2D();
			var particleSize = new Vector2(4, 4);
			particle.Texture = CreateRectangleTexture(particleSize, Colors.Gray);
			particle.Position = Position;

			// 随机飞行方向和速度
			var direction = new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f).Normalized();
			var distance = GD.RandRange(20, 50);

			var tween = CreateTween();
			tween.TweenProperty(particle, "position", Position + direction * distance, 0.5f);
			tween.Parallel().TweenProperty(particle, "rotation", GD.Randf() * 360, 0.5f);
			tween.Parallel().TweenProperty(particle, "modulate:a", 0, 0.5f);
			tween.TweenCallback(Callable.From(() => particle.QueueFree()));

			return particle;
		}
	}
}