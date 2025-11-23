using Godot;

namespace PlantsVsZombies.Scripts.Combat
{
    /// <summary>
    /// 豌豆子弹类 - 豌豆射手发射的投射物
    /// </summary>
    public partial class Pea : Projectile
    {
        private Sprite2D _sprite;
        private AnimationPlayer _animationPlayer;
        private GpuParticles2D _hitParticles;

        [Export]
        public Color PeaColor { get; set; } = Colors.Green; // 豌豆颜色

        [Export]
        public float PeaSize { get; set; } = 12f; // 豌豆大小

        public override void _Ready()
        {
            base._Ready();

            // 设置伤害值
            Damage = 20;

            // 创建视觉元素
            CreateVisualElements();

            GD.Print("[Pea] 豌豆子弹初始化完成");
        }

        /// <summary>
        /// 创建豌豆的视觉元素
        /// </summary>
        private void CreateVisualElements()
        {
            // 创建精灵
            _sprite = new Sprite2D();
            AddChild(_sprite);

            // 创建圆形纹理作为豌豆
            CreatePeaTexture();

            // 创建碰撞形状
            CreateCollisionShape();

            // 创建击中粒子效果
            CreateHitParticles();
        }

        /// <summary>
        /// 创建豌豆纹理
        /// </summary>
        private void CreatePeaTexture()
        {
            // 使用Godot的内置功能创建圆形纹理
            int imageSize = (int)PeaSize * 2;
            var image = Image.CreateEmpty(imageSize, imageSize, false, Image.Format.Rgba8);

            // 填充透明背景
            image.Fill(Colors.Transparent);

            // 画绿色圆形
            var center = imageSize / 2;
            for (int x = 0; x < imageSize; x++)
            {
                for (int y = 0; y < imageSize; y++)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(x - center, 2) + Mathf.Pow(y - center, 2));
                    if (distance <= PeaSize)
                    {
                        // 添加简单的光照效果
                        var color = PeaColor;
                        float lightFactor = 1f - (distance / PeaSize) * 0.3f;
                        color = color.Lightened(lightFactor);
                        image.SetPixel(x, y, color);
                    }
                }
            }

            var texture = ImageTexture.CreateFromImage(image);
            _sprite.Texture = texture;
            _sprite.Centered = true;
        }

        /// <summary>
        /// 创建碰撞形状
        /// </summary>
        private void CreateCollisionShape()
        {
            var collisionShape = new CollisionShape2D();
            var circleShape = new CircleShape2D();
            circleShape.Radius = PeaSize;
            collisionShape.Shape = circleShape;
            AddChild(collisionShape);
        }

        /// <summary>
        /// 创建击中粒子效果
        /// </summary>
        private void CreateHitParticles()
        {
            _hitParticles = new GpuParticles2D();
            AddChild(_hitParticles);

            // 配置粒子系统
            var particleMaterial = new ParticleProcessMaterial();

            // 发射方向
            particleMaterial.Direction = Vector3.Zero;
            particleMaterial.Spread = Mathf.Pi; // 全方向发射

            // 初始速度 - 使用 Vector3
            particleMaterial.InitialVelocityMin = 50f;
            particleMaterial.InitialVelocityMax = 150f;

            // 粒子大小
            particleMaterial.ScaleMin = 0.2f;
            particleMaterial.ScaleMax = 0.5f;

            // 颜色
            particleMaterial.Color = PeaColor;

            // 生命周期 - GpuParticles2D 控制生命周期，不在这里设置

            // 发射设置通过 GpuParticles2D 的 Amount 属性控制

            _hitParticles.ProcessMaterial = particleMaterial;
            _hitParticles.OneShot = true;
            _hitParticles.Emitting = false;

            // 设置粒子纹理
            CreateParticleTexture();
        }

        /// <summary>
        /// 创建粒子纹理
        /// </summary>
        private void CreateParticleTexture()
        {
            var particleImage = new Image();
            particleImage = Image.CreateEmpty(8, 8, false, Image.Format.Rgba8);

            // 创建小的圆形粒子
            particleImage.Fill(Colors.Transparent);
            var center = 4;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(x - center, 2) + Mathf.Pow(y - center, 2));
                    if (distance <= 3f)
                    {
                        particleImage.SetPixel(x, y, PeaColor);
                    }
                }
            }

            var particleTexture = ImageTexture.CreateFromImage(particleImage);
            _hitParticles.Texture = particleTexture;
        }

        /// <summary>
        /// 重写击中效果
        /// </summary>
        protected override void OnHitEffect()
        {
            // 播放击中粒子效果
            if (_hitParticles != null)
            {
                _hitParticles.Emitting = true;

                // 延迟销毁，让粒子效果播放完成
                var timer = new Timer();
                timer.WaitTime = 0.6f;
                timer.OneShot = true;
                timer.Timeout += () => {
                    if (IsInstanceValid(this))
                    {
                        Deactivate();
                    }
                };
                AddChild(timer);
                timer.Start();

                GD.Print("[Pea] 播放击中粒子效果");
            }
            else
            {
                base.OnHitEffect();
            }
        }

        /// <summary>
        /// 设置豌豆属性
        /// </summary>
        public void SetPeaProperties(int damage, float speed, Color color)
        {
            Damage = damage;
            Speed = speed;
            PeaColor = color;

            // 重新创建纹理以应用新颜色
            if (_sprite != null)
            {
                CreatePeaTexture();
            }

            GD.Print($"[Pea] 设置属性 - 伤害: {damage}, 速度: {speed}, 颜色: {color}");
        }

        /// <summary>
        /// 获取豌豆当前状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            return $"豌豆状态 - 激活: {_isActive}, 位置: {Position}, 方向: {Direction}, 伤害: {Damage}";
        }
    }
}