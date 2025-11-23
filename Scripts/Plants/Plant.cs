using Godot;

namespace PlantsVsZombies.Plants
{
	public partial class Plant : Node2D
	{
		[Export] public int MaxHealth { get; protected set; } = 100;
		[Export] public int CurrentHealth { get; protected set; }
		[Export] public int Cost { get; protected set; } = 50;

		public bool IsAlive { get; protected set; }
		protected int gridRow;
		protected int gridCol;

		public override void _Ready()
		{
			CurrentHealth = MaxHealth;
			IsAlive = true;
		}

		public virtual void TakeDamage(int damage)
		{
			if (!IsAlive) return;

			CurrentHealth -= damage;
			PlayHitEffect();

			if (CurrentHealth <= 0)
			{
				Die();
			}
		}

		protected virtual void Die()
		{
			IsAlive = false;
			PlayDeathAnimation();
			SpawnDeathEffect();
			QueueFree();
		}

		protected virtual void PlayHitEffect()
		{
			// 默认受击效果：闪烁
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", Colors.Red, 0.1f);
			tween.TweenProperty(this, "modulate", Colors.White, 0.1f);
		}

		protected virtual void PlayDeathAnimation()
		{
			// 默认死亡动画
		}

		protected virtual void SpawnDeathEffect()
		{
			// 默认死亡特效
		}

		public void SetGridPosition(int row, int col)
		{
			gridRow = row;
			gridCol = col;
		}

		public (int row, int col) GetGridPosition()
		{
			return (gridRow, gridCol);
		}
	}
}