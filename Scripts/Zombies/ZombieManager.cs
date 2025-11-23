using Godot;
using System.Collections.Generic;

namespace PlantsVsZombies.Zombies
{
	public partial class ZombieManager : Node
	{
		[Export] public float FirstWaveDelay { get; set; } = 30f; // 第一波僵尸延迟时间
		[Export] public float WaveInterval { get; set; } = 20f; // 波次间隔
		[Export] public float SpawnInterval { get; set; } = 2f; // 同一波次内僵尸生成间隔
		[Export] public int MaxZombiesPerWave { get; set; } = 10; // 每波最多僵尸数
		[Export] public int IncreasePerWave { get; set; } = 1; // 每波增加的僵尸数

		public PackedScene BasicZombieScene { get; private set; }

		private Timer waveTimer;
		private Timer spawnTimer;
		private int currentWave = 0;
		private int zombiesToSpawn = 0;
		private int zombiesSpawned = 0;
		private bool isSpawningWave = false;

		// 网格系统常量
		public const int GRID_ROWS = 5;
		public const int GRID_COLUMNS = 9;
		public const int CELL_SIZE = 120; // 每个网格单元的像素大小
		public const int GRID_START_X = 300;
		public const int GRID_START_Y = 150;

		public override void _Ready()
		{
			SetupWaveTimer();
			SetupSpawnTimer();
			LoadZombieScenes();
		}

		private void SetupWaveTimer()
		{
			waveTimer = new Timer();
			waveTimer.WaitTime = FirstWaveDelay;
			waveTimer.Timeout += OnFirstWave;
			waveTimer.OneShot = true;
			AddChild(waveTimer);
			waveTimer.Start();
		}

		private void SetupSpawnTimer()
		{
			spawnTimer = new Timer();
			spawnTimer.WaitTime = SpawnInterval;
			spawnTimer.Timeout += OnSpawnTimerTimeout;
			spawnTimer.OneShot = false;
			AddChild(spawnTimer);
		}

		private void LoadZombieScenes()
		{
			// 预加载僵尸场景
			BasicZombieScene = GD.Load<PackedScene>("res://Scenes/Zombies/BasicZombie.tscn");

			if (BasicZombieScene == null)
			{
				GD.PrintErr("无法加载BasicZombie场景文件");
			}
		}

		private void OnFirstWave()
		{
			GD.Print($"第一波僵尸即将来临！");
			SpawnWave();

			// 设置后续波次
			waveTimer.WaitTime = WaveInterval;
			waveTimer.Timeout += OnSubsequentWave;
			waveTimer.OneShot = false;
			waveTimer.Start();
		}

		private void OnSubsequentWave()
		{
			currentWave++;
			GD.Print($"第 {currentWave + 1} 波僵尸即将来临！");
			SpawnWave();
		}

		private void SpawnWave()
		{
			if (isSpawningWave) return;

			isSpawningWave = true;
			zombiesToSpawn = CalculateZombieCount(currentWave);
			zombiesSpawned = 0;

			GD.Print($"生成第 {currentWave + 1} 波，共 {zombiesToSpawn} 只僵尸");

			// 开始生成僵尸
			spawnTimer.Start();
		}

		private int CalculateZombieCount(int wave)
		{
			// 基础数量 + 波次加成
			int baseCount = 3; // 第一波3只僵尸
			int additionalZombies = wave * IncreasePerWave;
			int total = baseCount + additionalZombies;

			// 限制最大数量
			return Mathf.Min(total, MaxZombiesPerWave);
		}

		private void OnSpawnTimerTimeout()
		{
			if (zombiesSpawned >= zombiesToSpawn)
			{
				spawnTimer.Stop();
				isSpawningWave = false;
				GD.Print($"第 {currentWave + 1} 波生成完成");
				return;
			}

			SpawnSingleZombie();
			zombiesSpawned++;
		}

		private void SpawnSingleZombie()
		{
			// 随机选择行
			int row = GD.RandRange(0, GRID_ROWS - 1);
			Vector2 spawnPosition = GetSpawnPosition(row);

			try
			{
				var zombie = BasicZombieScene.Instantiate<BasicZombie>();
				if (zombie != null)
				{
					zombie.Position = spawnPosition;
					zombie.AddToGroup("Zombies");

					// 获取当前游戏场景并添加僵尸
					var gameScene = GetTree().CurrentScene;
					if (gameScene != null)
					{
						gameScene.AddChild(zombie);
						GD.Print($"僵尸生成在行 {row}，位置 {spawnPosition}");
					}
					else
					{
						GD.PrintErr("无法找到当前游戏场景");
						zombie.QueueFree();
					}
				}
				else
				{
					GD.PrintErr("无法实例化BasicZombie");
				}
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"生成僵尸时出错: {e.Message}");
			}
		}

		private Vector2 GetSpawnPosition(int row)
		{
			// 计算指定行的Y坐标
			float y = GRID_START_Y + row * CELL_SIZE + CELL_SIZE / 2; // 行中心位置

			// 从屏幕右侧稍微外部开始
			float x = 1920 + 50; // 屏幕宽度为1920，从右侧50像素外开始

			return new Vector2(x, y);
		}

		// 获取指定行的所有僵尸
		public List<Zombie> GetZombiesInRow(int row)
		{
			var zombiesInRow = new List<Zombie>();
			var zombies = GetTree().GetNodesInGroup("Zombies");

			float rowTop = GRID_START_Y + row * CELL_SIZE;
			float rowBottom = rowTop + CELL_SIZE;

			foreach (Node node in zombies)
			{
				if (node is Zombie zombie)
				{
					if (zombie.Position.Y >= rowTop && zombie.Position.Y <= rowBottom)
					{
						zombiesInRow.Add(zombie);
					}
				}
			}

			return zombiesInRow;
		}

		// 检查指定行是否有僵尸
		public bool HasZombieInRow(int row)
		{
			return GetZombiesInRow(row).Count > 0;
		}

		// 获取所有存活的僵尸
		public List<Zombie> GetAllAliveZombies()
		{
			var aliveZombies = new List<Zombie>();
			var zombies = GetTree().GetNodesInGroup("Zombies");

			foreach (Node node in zombies)
			{
				if (node is Zombie zombie && GodotObject.IsInstanceValid(zombie) && !zombie.IsDying)
				{
					aliveZombies.Add(zombie);
				}
			}

			return aliveZombies;
		}

		// 获取指定位置的僵尸（用于植物攻击检测）
		public Zombie GetZombieAtPosition(Vector2 position, float range)
		{
			var zombies = GetAllAliveZombies();

			foreach (Zombie zombie in zombies)
			{
				float distance = zombie.GlobalPosition.DistanceTo(position);
				if (distance <= range)
				{
					return zombie;
				}
			}

			return null;
		}

		// 清除所有僵尸（游戏结束或重新开始时使用）
		public void ClearAllZombies()
		{
			var zombies = GetTree().GetNodesInGroup("Zombies");

			foreach (Node node in zombies)
			{
				if (node is Zombie zombie)
				{
					zombie.QueueFree();
				}
			}
		}

		// 暂停/恢复僵尸生成
		public void PauseSpawning()
		{
			waveTimer.Paused = true;
			spawnTimer.Paused = true;
		}

		public void ResumeSpawning()
		{
			waveTimer.Paused = false;
			spawnTimer.Paused = false;
		}

		// 获取当前波次信息
		public string GetWaveInfo()
		{
			return $"波次: {currentWave + 1} | 剩余: {zombiesToSpawn - zombiesSpawned}";
		}

		// 获取僵尸统计信息
		public Dictionary<string, int> GetZombieStats()
		{
			var stats = new Dictionary<string, int>
			{
				["TotalZombies"] = GetAllAliveZombies().Count,
				["CurrentWave"] = currentWave + 1,
				["WaveInProgress"] = isSpawningWave ? 1 : 0
			};

			// 统计每行的僵尸数量
			for (int row = 0; row < GRID_ROWS; row++)
			{
				stats[$"Row_{row}_Zombies"] = GetZombiesInRow(row).Count;
			}

			return stats;
		}

		// 立即生成测试僵尸（用于调试）
		public void SpawnTestZombie(int row = -1)
		{
			if (row < 0 || row >= GRID_ROWS)
			{
				row = GD.RandRange(0, GRID_ROWS - 1);
			}

			Vector2 spawnPosition = GetSpawnPosition(row);

			var zombie = BasicZombieScene.Instantiate<BasicZombie>();
			if (zombie != null)
			{
				zombie.Position = spawnPosition;
				zombie.AddToGroup("Zombies");

				var gameScene = GetTree().CurrentScene;
				if (gameScene != null)
				{
					gameScene.AddChild(zombie);
					GD.Print($"测试僵尸生成在行 {row}，位置 {spawnPosition}");
				}
			}
		}

		public override void _ExitTree()
		{
			// 清理计时器
			if (waveTimer != null)
			{
				waveTimer.Timeout -= OnFirstWave;
				waveTimer.Timeout -= OnSubsequentWave;
			}
			if (spawnTimer != null)
			{
				spawnTimer.Timeout -= OnSpawnTimerTimeout;
			}
		}
	}
}