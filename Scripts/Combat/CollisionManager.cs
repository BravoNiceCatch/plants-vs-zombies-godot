using Godot;
using System.Collections.Generic;

namespace PlantsVsZombies.Scripts.Combat
{
    /// <summary>
    /// 碰撞检测管理器 - 负责管理游戏中的碰撞检测和对象池优化
    /// </summary>
    public partial class CollisionManager : Node
    {
        // 单例实例
        private static CollisionManager _instance;
        public static CollisionManager Instance => _instance;

        // 活跃的投射物列表
        private readonly List<Projectile> _activeProjectiles = new();

        // 可造成伤害的对象列表
        private readonly List<IDamageable> _damageableObjects = new();

        // 性能统计
        private int _totalCollisionsChecked = 0;
        private int _totalDamageDealt = 0;

        // 空间分区网格（用于性能优化）
        private readonly Dictionary<Vector2I, List<Projectile>> _spatialGrid = new();
        private readonly int _gridSize = 100; // 网格大小（像素）

        // 对象池
        private readonly Queue<Projectile> _projectilePool = new();
        private const int MAX_POOL_SIZE = 50;

        public override void _Ready()
        {
            // 设置单例
            if (_instance == null)
            {
                _instance = this;
                SetProcess(true);
            }
            else
            {
                QueueFree();
                return;
            }

            GD.Print("[CollisionManager] 碰撞检测管理器初始化完成");
        }

        public override void _Process(double delta)
        {
            // 只在游戏运行时执行碰撞检测
            if (!GameManager.Instance.IsGameRunning) return;

            // 更新空间分区
            UpdateSpatialGrid();

            // 执行碰撞检测
            PerformCollisionDetection();
        }

        /// <summary>
        /// 注册投射物
        /// </summary>
        public void RegisterProjectile(Projectile projectile)
        {
            if (!_activeProjectiles.Contains(projectile))
            {
                _activeProjectiles.Add(projectile);
                GD.Print($"[CollisionManager] 注册投射物: {projectile.Name} (总数: {_activeProjectiles.Count})");
            }
        }

        /// <summary>
        /// 注销投射物
        /// </summary>
        public void UnregisterProjectile(Projectile projectile)
        {
            if (_activeProjectiles.Contains(projectile))
            {
                _activeProjectiles.Remove(projectile);

                // 从空间网格中移除
                RemoveFromSpatialGrid(projectile);

                GD.Print($"[CollisionManager] 注销投射物: {projectile.Name} (剩余: {_activeProjectiles.Count})");
            }
        }

        /// <summary>
        /// 注册可伤害对象
        /// </summary>
        public void RegisterDamageableObject(IDamageable damageable)
        {
            if (!_damageableObjects.Contains(damageable))
            {
                _damageableObjects.Add(damageable);
                GD.Print($"[CollisionManager] 注册可伤害对象: {damageable} (总数: {_damageableObjects.Count})");
            }
        }

        /// <summary>
        /// 注销可伤害对象
        /// </summary>
        public void UnregisterDamageableObject(IDamageable damageable)
        {
            if (_damageableObjects.Contains(damageable))
            {
                _damageableObjects.Remove(damageable);
                GD.Print($"[CollisionManager] 注销可伤害对象: {damageable} (剩余: {_damageableObjects.Count})");
            }
        }

        /// <summary>
        /// 更新空间分区网格
        /// </summary>
        private void UpdateSpatialGrid()
        {
            // 清空网格
            _spatialGrid.Clear();

            // 将投射物分配到网格中
            foreach (var projectile in _activeProjectiles)
            {
                if (projectile == null || !IsInstanceValid(projectile)) continue;

                var gridPos = WorldToGrid(projectile.Position);

                if (!_spatialGrid.ContainsKey(gridPos))
                {
                    _spatialGrid[gridPos] = new List<Projectile>();
                }

                _spatialGrid[gridPos].Add(projectile);
            }
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        private Vector2I WorldToGrid(Vector2 worldPos)
        {
            return new Vector2I(
                (int)Mathf.Floor(worldPos.X / _gridSize),
                (int)Mathf.Floor(worldPos.Y / _gridSize)
            );
        }

        /// <summary>
        /// 从空间网格中移除投射物
        /// </summary>
        private void RemoveFromSpatialGrid(Projectile projectile)
        {
            var gridPos = WorldToGrid(projectile.Position);

            if (_spatialGrid.ContainsKey(gridPos))
            {
                _spatialGrid[gridPos].Remove(projectile);

                // 如果网格为空，移除网格条目
                if (_spatialGrid[gridPos].Count == 0)
                {
                    _spatialGrid.Remove(gridPos);
                }
            }
        }

        /// <summary>
        /// 执行碰撞检测
        /// </summary>
        private void PerformCollisionDetection()
        {
            var damageableToRemove = new List<IDamageable>();

            foreach (var damageable in _damageableObjects)
            {
                if (damageable == null || !damageable.IsAlive())
                {
                    damageableToRemove.Add(damageable);
                    continue;
                }

                // 获取可伤害对象的位置（需要转换为Vector2）
                Vector2 damageablePos = GetDamageablePosition(damageable);
                var gridPos = WorldToGrid(damageablePos);

                // 检查周围的网格
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        var checkGridPos = new Vector2I(gridPos.X + x, gridPos.Y + y);

                        if (_spatialGrid.ContainsKey(checkGridPos))
                        {
                            CheckCollisionsInGrid(_spatialGrid[checkGridPos], damageable, damageablePos);
                        }
                    }
                }
            }

            // 移除死亡的对象
            foreach (var deadObject in damageableToRemove)
            {
                UnregisterDamageableObject(deadObject);
            }
        }

        /// <summary>
        /// 检查网格内的碰撞
        /// </summary>
        private void CheckCollisionsInGrid(List<Projectile> projectiles, IDamageable damageable, Vector2 damageablePos)
        {
            foreach (var projectile in projectiles)
            {
                if (projectile == null || !IsInstanceValid(projectile)) continue;

                _totalCollisionsChecked++;

                // 简单的距离检测
                float distance = projectile.Position.DistanceTo(damageablePos);
                if (distance <= 30f) // 碰撞半径
                {
                    // 手动触发碰撞处理
                    GD.Print($"[CollisionManager] 检测到碰撞: {projectile.Name} -> {damageable}");

                    // 造成伤害
                    damageable.TakeDamage(projectile.Damage);
                    _totalDamageDealt += projectile.Damage;

                    // 销毁投射物
                    projectile.Deactivate();
                }
            }
        }

        /// <summary>
        /// 获取可伤害对象的位置（需要根据具体类型实现）
        /// </summary>
        private Vector2 GetDamageablePosition(IDamageable damageable)
        {
            // 尝试转换为Node2D获取位置
            if (damageable is Node2D node2D)
            {
                return node2D.Position;
            }

            // 如果无法获取位置，返回原点
            return Vector2.Zero;
        }

        /// <summary>
        /// 从对象池获取投射物
        /// </summary>
        public T GetProjectileFromPool<T>() where T : Projectile, new()
        {
            if (_projectilePool.Count > 0)
            {
                var projectile = (T)_projectilePool.Dequeue();
                if (IsInstanceValid(projectile))
                {
                    return projectile;
                }
            }

            // 创建新的投射物
            var newProjectile = new T();
            return newProjectile;
        }

        /// <summary>
        /// 将投射物返回到对象池
        /// </summary>
        public void ReturnProjectileToPool(Projectile projectile)
        {
            if (_projectilePool.Count < MAX_POOL_SIZE && projectile != null)
            {
                // 重置投射物状态
                projectile.Visible = false;
                projectile.Position = Vector2.Zero;

                _projectilePool.Enqueue(projectile);
            }
            else
            {
                // 如果池已满，直接销毁
                if (projectile != null && IsInstanceValid(projectile))
                {
                    projectile.QueueFree();
                }
            }
        }

        /// <summary>
        /// 获取性能统计信息
        /// </summary>
        public string GetPerformanceStats()
        {
            return $"碰撞统计 - 总检查: {_totalCollisionsChecked}, 总伤害: {_totalDamageDealt}, " +
                   $"活跃投射物: {_activeProjectiles.Count}, 可伤害对象: {_damageableObjects.Count}, " +
                   $"空间网格数: {_spatialGrid.Count}";
        }

        /// <summary>
        /// 清理所有活跃投射物
        /// </summary>
        public void ClearAllProjectiles()
        {
            foreach (var projectile in _activeProjectiles.ToArray())
            {
                if (projectile != null && IsInstanceValid(projectile))
                {
                    projectile.Deactivate();
                }
            }

            _activeProjectiles.Clear();
            _spatialGrid.Clear();

            GD.Print("[CollisionManager] 清理所有投射物");
        }

        public override void _ExitTree()
        {
            // 清理单例引用
            if (_instance == this)
            {
                _instance = null;
            }

            // 清理对象池
            while (_projectilePool.Count > 0)
            {
                var projectile = _projectilePool.Dequeue();
                if (projectile != null && IsInstanceValid(projectile))
                {
                    projectile.QueueFree();
                }
            }

            GD.Print("[CollisionManager] 碰撞检测管理器已清理");
        }
    }
}