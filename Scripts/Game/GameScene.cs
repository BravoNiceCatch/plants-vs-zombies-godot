using Godot;
using PlantsVsZombies.Combat;
using PlantsVsZombies.Effects;
using PlantsVsZombies.Scripts.Combat;
using PlantsVsZombies.Core;

namespace PlantsVsZombies.Game
{
	public partial class GameScene : Node2D
	{
		private GridContainer _gameGrid;
		private Label _sunCountLabel;
		private Label _zombieCountLabel;
		private Label _combatStatsLabel;
		private int _sunCount = 50; // 初始阳光
		private int _zombieKillCount = 0; // 击杀僵尸数
		
		// 战斗系统组件
		private CombatManager _combatManager;
		private Timer _sunGenerationTimer;
		private Timer _combatStatsUpdateTimer;
		
		public override void _Ready()
		{
			InitializeGameUI();
			InitializeCombatSystem();
			InitializeTimers();
			ConnectSignals();

			// 设置游戏为运行状态
			GameManager.Instance.SetGameRunning(true);
		}
		
		private void InitializeGameUI()
		{
			// 创建游戏网格 (5行9列)
			_gameGrid = new GridContainer();
			_gameGrid.Columns = 9;
			_gameGrid.Position = new Vector2I(300, 150); // 偏移以适应UI布局
			AddChild(_gameGrid);
			
			// 创建网格单元格
			for (int row = 0; row < 5; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					var slot = new Panel();
					slot.Size = new Vector2I(120, 120);
					slot.Modulate = Colors.Green;
					
					// 添加边框
					var styleBox = new StyleBoxFlat();
					styleBox.BorderColor = Colors.DarkGreen;
					styleBox.BorderWidthLeft = 2;
					styleBox.BorderWidthRight = 2;
					styleBox.BorderWidthTop = 2;
					styleBox.BorderWidthBottom = 2;
					slot.AddThemeStyleboxOverride("panel", styleBox);
					
					_gameGrid.AddChild(slot);
				}
			}
			
			// 创建阳光显示
			_sunCountLabel = new Label();
			_sunCountLabel.Text = $"阳光: {_sunCount}";
			_sunCountLabel.Position = new Vector2I(50, 50);
			_sunCountLabel.Modulate = Colors.Yellow;
			AddChild(_sunCountLabel);
			
			// 创建击杀数显示
			_zombieCountLabel = new Label();
			_zombieCountLabel.Text = $"击杀: {_zombieKillCount}";
			_zombieCountLabel.Position = new Vector2I(50, 80);
			_zombieCountLabel.Modulate = Colors.Red;
			AddChild(_zombieCountLabel);
			
			// 创建战斗统计显示
			_combatStatsLabel = new Label();
			_combatStatsLabel.Text = "战斗: 爆炸0 伤害0";
			_combatStatsLabel.Position = new Vector2I(50, 110);
			_combatStatsLabel.Modulate = Colors.Cyan;
			AddChild(_combatStatsLabel);
		}
		
		/// <summary>
		/// 初始化战斗系统
		/// </summary>
		private void InitializeCombatSystem()
		{
			// 创建战斗管理器
			_combatManager = new CombatManager();
			_combatManager.Name = "CombatManager";
			AddChild(_combatManager);
			
			GD.Print("[GameScene] 战斗系统初始化完成");
		}
		
		/// <summary>
		/// 初始化计时器
		/// </summary>
		private void InitializeTimers()
		{
			// 阳光生成计时器（每10秒）
			_sunGenerationTimer = new Timer();
			_sunGenerationTimer.WaitTime = 10.0f;
			_sunGenerationTimer.Timeout += OnSunGenerationTimer;
			_sunGenerationTimer.Autostart = true;
			AddChild(_sunGenerationTimer);
			
			// 战斗统计更新计时器（每1秒）
			_combatStatsUpdateTimer = new Timer();
			_combatStatsUpdateTimer.WaitTime = 1.0f;
			_combatStatsUpdateTimer.Timeout += UpdateCombatStats;
			_combatStatsUpdateTimer.Autostart = true;
			AddChild(_combatStatsUpdateTimer);
		}
		
		private void ConnectSignals()
		{
			// 连接游戏相关信号
			// 这里可以连接植物种植、僵尸生成等信号
		}
		
		public void AddSun(int amount)
		{
			_sunCount += amount;
			UpdateSunDisplay();
		}
		
		public bool SpendSun(int amount)
		{
			if (_sunCount >= amount)
			{
				_sunCount -= amount;
				UpdateSunDisplay();
				return true;
			}
			return false;
		}
		
		private void UpdateSunDisplay()
		{
			_sunCountLabel.Text = $"阳光: {_sunCount}";
		}
		
		public void OnZombieKilled()
		{
			_zombieKillCount++;
			_zombieCountLabel.Text = $"击杀: {_zombieKillCount}";
		}
		
		/// <summary>
		/// 阳光生成计时器回调
		/// </summary>
		private void OnSunGenerationTimer()
		{
			AddSun(25); // 每10秒生成25阳光
			GD.Print("[GameScene] 阳光生成: +25");
		}
		
		/// <summary>
		/// 更新战斗统计显示
		/// </summary>
		private void UpdateCombatStats()
		{
			if (_combatManager != null)
			{
				var stats = _combatManager.GetCombatStats();
				_combatStatsLabel.Text = $"战斗: 爆炸{stats.TotalExplosions} 伤害{stats.TotalDamageDealt}";
			}
		}
		
		/// <summary>
		/// 种植樱桃炸弹
		/// </summary>
		/// <param name="gridPosition">网格位置</param>
		/// <returns>是否种植成功</returns>
		public bool PlantCherryBomb(Vector2I gridPosition)
		{
			if (!IsValidGridPosition(gridPosition))
				return false;
				
			// 检查阳光是否足够
			const int cherryBombCost = 150;
			if (!SpendSun(cherryBombCost))
			{
				GD.Print("[GameScene] 阳光不足种植樱桃炸弹");
				return false;
			}
			
			// 计算实际位置
			var worldPosition = GridToWorldPosition(gridPosition);
			
			// 创建樱桃炸弹
			var cherryBomb = new CherryBomb();
			cherryBomb.Position = worldPosition;
			AddChild(cherryBomb);
			
			// 注册到战斗管理器
			_combatManager.RegisterCombatEntity(cherryBomb);
			
			// 连接爆炸信号
			cherryBomb.OnExploded += OnCherryBombExploded;
			
			// 种植樱桃炸弹
			cherryBomb.Plant(worldPosition);
			
			GD.Print($"[GameScene] 樱桃炸弹种植: 网格{gridPosition}, 世界{worldPosition}");
			return true;
		}
		
		/// <summary>
		/// 樱桃炸弹爆炸回调
		/// </summary>
		/// <param name="position">爆炸位置</param>
		private void OnCherryBombExploded(Vector2 position)
		{
			GD.Print($"[GameScene] 樱桃炸弹爆炸: 位置{position}");
			// 爆炸效果由CombatManager处理
		}
		
		/// <summary>
		/// 创建测试爆炸（用于调试）
		/// </summary>
		/// <param name="position">爆炸位置</param>
		public void CreateTestExplosion(Vector2 position)
		{
			if (_combatManager != null)
			{
				_combatManager.CreateExplosion(position, 100f, 500, ExplosionType.Normal);
			}
		}
		
		/// <summary>
		/// 验证网格位置是否有效
		/// </summary>
		/// <param name="gridPosition">网格位置</param>
		/// <returns>是否有效</returns>
		private bool IsValidGridPosition(Vector2I gridPosition)
		{
			return gridPosition.X >= 0 && gridPosition.X < 9 && gridPosition.Y >= 0 && gridPosition.Y < 5;
		}
		
		/// <summary>
		/// 将网格位置转换为世界位置
		/// </summary>
		/// <param name="gridPosition">网格位置</param>
		/// <returns>世界位置</returns>
		private Vector2 GridToWorldPosition(Vector2I gridPosition)
		{
			var slotSize = new Vector2I(120, 120);
			var gridOffset = new Vector2I(300, 150);
			
			return new Vector2(
				gridOffset.X + gridPosition.X * slotSize.X + slotSize.X / 2,
				gridOffset.Y + gridPosition.Y * slotSize.Y + slotSize.Y / 2
			);
		}
		
		public override void _Input(InputEvent @event)
		{
			// 处理输入事件
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
				{
					// 右键点击创建测试爆炸
					CreateTestExplosion(GetGlobalMousePosition());
				}
				else if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Middle)
				{
					// 中键点击发射测试豌豆
					var startPos = new Vector2(350, 300); // 从左侧发射
					var direction = Vector2.Right;
					CreateTestPea(startPos, direction);
				}
				else if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick)
				{
					// 左键双击种植樱桃炸弹
					var gridPos = WorldToGridPosition(GetGlobalMousePosition());
					if (IsValidGridPosition(gridPos))
					{
						PlantCherryBomb(gridPos);
					}
				}
			}

			// 按1键创建樱桃炸弹在鼠标位置
			if (@event.IsActionPressed("ui_accept")) // Enter键或Space
			{
				var gridPos = WorldToGridPosition(GetGlobalMousePosition());
				if (IsValidGridPosition(gridPos))
				{
					PlantCherryBomb(gridPos);
				}
			}
			// 按2键发射测试豌豆
			else if (@event.IsActionPressed("ui_cancel")) // ESC键改为发射豌豆
			{
				var startPos = new Vector2(350, 300); // 从左侧发射
				var direction = Vector2.Right;
				CreateTestPea(startPos, direction);
			}
		}
		
		/// <summary>
		/// 将世界位置转换为网格位置
		/// </summary>
		/// <param name="worldPosition">世界位置</param>
		/// <returns>网格位置</returns>
		private Vector2I WorldToGridPosition(Vector2 worldPosition)
		{
			var slotSize = new Vector2I(120, 120);
			var gridOffset = new Vector2I(300, 150);
			
			var gridX = (int)((worldPosition.X - gridOffset.X) / slotSize.X);
			var gridY = (int)((worldPosition.Y - gridOffset.Y) / slotSize.Y);
			
			return new Vector2I(gridX, gridY);
		}
		
		/// <summary>
		/// 创建测试豌豆投射物
		/// </summary>
		/// <param name="startPosition">起始位置</param>
		/// <param name="direction">方向</param>
		public void CreateTestPea(Vector2 startPosition, Vector2 direction)
		{
			// 加载豌豆场景
			var peaScene = GD.Load<PackedScene>("res://Scenes/Combat/Pea.tscn");
			if (peaScene != null)
			{
				var pea = peaScene.Instantiate<Pea>();
				AddChild(pea);

				// 激活豌豆
				pea.Activate(startPosition, direction);

				GD.Print($"[GameScene] 创建测试豌豆: 位置{startPosition}, 方向{direction}");
			}
			else
			{
				GD.Print("[GameScene] 无法加载豌豆场景");
			}
		}

		/// <summary>
		/// 游戏结束时清理
		/// </summary>
		public void CleanupGame()
		{
			// 设置游戏为停止状态
			GameManager.Instance.SetGameRunning(false);

			if (_combatManager != null)
			{
				_combatManager.ClearAllCombatEntities();
			}

			// 清理碰撞管理器中的投射物
			if (CollisionManager.Instance != null)
			{
				CollisionManager.Instance.ClearAllProjectiles();
			}

			_sunGenerationTimer?.Stop();
			_combatStatsUpdateTimer?.Stop();
		}
	}
}
