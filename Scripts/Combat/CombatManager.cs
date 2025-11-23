using Godot;
using System.Collections.Generic;
using PlantsVsZombies.Effects;
using System.Linq;

namespace PlantsVsZombies.Combat
{
	/// <summary>
	/// 战斗管理器 - 负责管理所有战斗相关的逻辑
	/// 包括AOE伤害、爆炸效果、战斗状态等
	/// </summary>
	public partial class CombatManager : Node
	{
		public static CombatManager Instance { get; private set; }
		
		// 所有活跃的爆炸对象
		private List<Explosion> _activeExplosions = new List<Explosion>();
		
		// 所有战斗中的实体（植物、僵尸等）
		private List<Node2D> _combatEntities = new List<Node2D>();
		
		// 战斗统计
		private int _totalDamageDealt = 0;
		private int _totalExplosions = 0;
		
		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				ProcessMode = ProcessModeEnum.Always;
				Name = "CombatManager";
			}
			else
			{
				QueueFree();
			}
		}
		
		public override void _Process(double delta)
		{
			// 更新所有活跃的爆炸
			UpdateExplosions(delta);
		}
		
		/// <summary>
		/// 注册战斗实体
		/// </summary>
		/// <param name="entity">战斗实体（植物、僵尸等）</param>
		public void RegisterCombatEntity(Node2D entity)
		{
			if (!_combatEntities.Contains(entity))
			{
				_combatEntities.Add(entity);
				GD.Print($"[CombatManager] 注册战斗实体: {entity.Name}");
			}
		}
		
		/// <summary>
		/// 注销战斗实体
		/// </summary>
		/// <param name="entity">要注销的战斗实体</param>
		public void UnregisterCombatEntity(Node2D entity)
		{
			_combatEntities.Remove(entity);
			GD.Print($"[CombatManager] 注销战斗实体: {entity.Name}");
		}
		
		/// <summary>
		/// 创建爆炸
		/// </summary>
		/// <param name="position">爆炸位置</param>
		/// <param name="radius">爆炸半径</param>
		/// <param name="damage">爆炸伤害</param>
		/// <param name="explosionType">爆炸类型</param>
		public void CreateExplosion(Vector2 position, float radius, int damage, ExplosionType explosionType = ExplosionType.Normal)
		{
			var explosion = new Explosion(position, radius, damage, explosionType);
			_activeExplosions.Add(explosion);
			
			// 创建爆炸视觉效果
			CreateExplosionEffect(position, radius, explosionType);
			
			// 应用AOE伤害
			ApplyAOEDamage(position, radius, damage);
			
			_totalExplosions++;
			GD.Print($"[CombatManager] 创建爆炸: 位置={position}, 半径={radius}, 伤害={damage}");
		}
		
		/// <summary>
		/// 应用AOE（范围）伤害
		/// </summary>
		/// <param name="center">爆炸中心</param>
		/// <param name="radius">伤害半径</param>
		/// <param name="damage">伤害值</param>
		private void ApplyAOEDamage(Vector2 center, float radius, int damage)
		{
			var damagedEntities = new List<Node2D>();
			
			foreach (var entity in _combatEntities)
			{
				if (entity == null || !IsInstanceValid(entity))
					continue;
				
				var distance = entity.GlobalPosition.DistanceTo(center);
				if (distance <= radius)
				{
					// 根据距离计算伤害衰减（距离越远伤害越低）
					var damageMultiplier = 1.0f - (distance / radius) * 0.5f; // 最小伤害为50%
					var finalDamage = (int)(damage * damageMultiplier);
					
					// 尝试对目标造成伤害
					if (TryDamageEntity(entity, finalDamage))
					{
						damagedEntities.Add(entity);
						_totalDamageDealt += finalDamage;
					}
				}
			}
			
			GD.Print($"[CombatManager] AOE伤害完成: 影响{damagedEntities.Count}个目标, 总伤害{_totalDamageDealt}");
		}
		
		/// <summary>
		/// 尝试对实体造成伤害
		/// </summary>
		/// <param name="entity">目标实体</param>
		/// <param name="damage">伤害值</param>
		/// <returns>是否成功造成伤害</returns>
		private bool TryDamageEntity(Node2D entity, int damage)
		{
			// 检查是否是僵尸（通过名称或组判断）
			if (entity.IsInGroup("zombies"))
			{
				// 如果僵尸有健康组件，调用伤害方法
				if (entity.HasMethod("TakeDamage"))
				{
					entity.Call("TakeDamage", damage);
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// 创建爆炸视觉效果
		/// </summary>
		/// <param name="position">爆炸位置</param>
		/// <param name="radius">爆炸半径</param>
		/// <param name="explosionType">爆炸类型</param>
		private void CreateExplosionEffect(Vector2 position, float radius, ExplosionType explosionType)
		{
			// 直接创建爆炸效果实例
			var effect = new ExplosionEffect();
			effect.GlobalPosition = position;
			
			// 将效果添加到当前场景
			GetTree().CurrentScene.AddChild(effect);
			
			// 初始化爆炸效果
			effect.Initialize(radius, explosionType);
		}
		
		/// <summary>
		/// 更新所有活跃的爆炸
		/// </summary>
		/// <param name="delta">时间增量</param>
		private void UpdateExplosions(double delta)
		{
			for (int i = _activeExplosions.Count - 1; i >= 0; i--)
			{
				var explosion = _activeExplosions[i];
				explosion.Update((float)delta);
				
				if (explosion.IsFinished)
				{
					_activeExplosions.RemoveAt(i);
				}
			}
		}
		
		/// <summary>
		/// 获取当前战斗统计
		/// </summary>
		/// <returns>战斗统计信息</returns>
		public CombatStats GetCombatStats()
		{
			return new CombatStats
			{
				TotalDamageDealt = _totalDamageDealt,
				TotalExplosions = _totalExplosions,
				ActiveExplosions = _activeExplosions.Count,
				CombatEntities = _combatEntities.Count
			};
		}
		
		/// <summary>
		/// 清理所有战斗实体（用于游戏重启）
		/// </summary>
		public void ClearAllCombatEntities()
		{
			_combatEntities.Clear();
			_activeExplosions.Clear();
			_totalDamageDealt = 0;
			_totalExplosions = 0;
			GD.Print("[CombatManager] 清理所有战斗实体");
		}
	}
	
	/// <summary>
	/// 爆炸类型枚举
	/// </summary>
	public enum ExplosionType
	{
		Normal,    // 普通爆炸
		Cherry,    // 樱桃炸弹
		Splash     // 溅射伤害
	}
	
	/// <summary>
	/// 战斗统计信息
	/// </summary>
	public class CombatStats
	{
		public int TotalDamageDealt { get; set; }
		public int TotalExplosions { get; set; }
		public int ActiveExplosions { get; set; }
		public int CombatEntities { get; set; }
		
		public override string ToString()
		{
			return $"总伤害: {TotalDamageDealt}, 总爆炸: {TotalExplosions}, 活跃爆炸: {ActiveExplosions}, 战斗实体: {CombatEntities}";
		}
	}
}