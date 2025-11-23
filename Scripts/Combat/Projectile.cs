using Godot;

namespace PlantsVsZombies.Scripts.Combat
{
    /// <summary>
    /// 投射物基类 - 处理所有投射物的通用行为
    /// </summary>
    public abstract partial class Projectile : Area2D
    {
        [Export]
        public int Damage { get; set; } = 20; // 基础伤害值

        [Export]
        public float Speed { get; set; } = 300f; // 移动速度(像素/秒)

        [Export]
        public Vector2 Direction { get; set; } = Vector2.Right; // 移动方向

        protected bool _isActive = false;
        protected CollisionManager _collisionManager;

        // 碰撞层配置
        private const int PROJECTILE_LAYER = 4; // 投射物层
        private const int ZOMBIE_LAYER = 8;     // 僵尸碰撞层

        public override void _Ready()
        {
            // 设置碰撞层和掩码
            CollisionLayer = 0;
            CollisionMask = ZOMBIE_LAYER;

            // 添加到碰撞管理器
            _collisionManager = GetNode<CollisionManager>("/root/CollisionManager");
            if (_collisionManager != null)
            {
                _collisionManager.RegisterProjectile(this);
            }

            // 连接碰撞信号
            BodyEntered += OnBodyEntered;
            AreaEntered += OnAreaEntered;

            GD.Print($"[{GetType().Name}] 投射物初始化完成，伤害: {Damage}, 速度: {Speed}");
        }

        public override void _Process(double delta)
        {
            if (!_isActive) return;

            // 直线移动
            float deltaTime = (float)delta;
            Position += Direction * Speed * deltaTime;

            // 检查是否超出屏幕边界
            CheckBounds();
        }

        /// <summary>
        /// 激活投射物
        /// </summary>
        public virtual void Activate(Vector2 startPosition, Vector2 direction)
        {
            Position = startPosition;
            Direction = direction.Normalized();
            _isActive = true;
            Visible = true;

            GD.Print($"[{GetType().Name}] 投射物激活，位置: {startPosition}, 方向: {direction}");
        }

        /// <summary>
        /// 停用投射物
        /// </summary>
        public virtual void Deactivate()
        {
            _isActive = false;
            Visible = false;

            if (_collisionManager != null)
            {
                _collisionManager.UnregisterProjectile(this);
            }

            // 返回对象池或销毁
            QueueFree();
        }

        /// <summary>
        /// 检查是否超出屏幕边界
        /// </summary>
        protected virtual void CheckBounds()
        {
            Vector2 screenSize = GetViewport().GetVisibleRect().Size;

            if (Position.X < -50 || Position.X > screenSize.X + 50 ||
                Position.Y < -50 || Position.Y > screenSize.Y + 50)
            {
                GD.Print($"[{GetType().Name}] 投射物超出屏幕边界，销毁");
                Deactivate();
            }
        }

        /// <summary>
        /// 处理与物理体的碰撞
        /// </summary>
        protected virtual void OnBodyEntered(Node body)
        {
            if (body is IDamageable damageable)
            {
                GD.Print($"[{GetType().Name}] 击中目标: {body.Name}");
                OnHitTarget(damageable);
            }
        }

        /// <summary>
        /// 处理与区域的碰撞
        /// </summary>
        protected virtual void OnAreaEntered(Area2D area)
        {
            if (area.GetParent() is IDamageable damageable)
            {
                GD.Print($"[{GetType().Name}] 击中目标区域: {area.Name}");
                OnHitTarget(damageable);
            }
        }

        /// <summary>
        /// 击中目标时的处理
        /// </summary>
        protected virtual void OnHitTarget(IDamageable target)
        {
            // 造成伤害
            target.TakeDamage(Damage);

            // 触发击中效果
            OnHitEffect();

            // 销毁投射物
            Deactivate();
        }

        /// <summary>
        /// 击中效果（可被子类重写）
        /// </summary>
        protected virtual void OnHitEffect()
        {
            // 默认无特殊效果
        }

        public override void _ExitTree()
        {
            // 清理碰撞管理器中的引用
            if (_collisionManager != null)
            {
                _collisionManager.UnregisterProjectile(this);
            }

            // 断开信号连接
            BodyEntered -= OnBodyEntered;
            AreaEntered -= OnAreaEntered;

            base._ExitTree();
        }
    }

    /// <summary>
    /// 可造成伤害的接口
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
        int GetCurrentHealth();
        bool IsAlive();
    }
}