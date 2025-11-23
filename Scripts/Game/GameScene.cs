using Godot;
using PlantsVsZombies.Combat;
using PlantsVsZombies.Effects;
using PlantsVsZombies.Scripts.Combat;
using PlantsVsZombies.Core;
using PlantsVsZombies.UI;

namespace PlantsVsZombies.Game
{
	public partial class GameScene : Node2D
	{
		private GridContainer _gameGrid;
		private Label _zombieCountLabel;
		private Label _combatStatsLabel;
		private int _zombieKillCount = 0; // 击杀僵尸数

		// 战斗系统组件
		private CombatManager _combatManager;
		private Timer _sunGenerationTimer;
		private Timer _combatStatsUpdateTimer;

		// 阳光系统组件
		private Timer _sunDropTimer;
		private PackedScene _sunScene;
		private const float SUN_DROP_INTERVAL = 8.0f; // 阳光掉落间隔（秒）

		// 新UI组件
		private SunCounter _sunCounter;
		private PlantCardSelector _plantCardSelector;
		private PauseMenu _pauseMenu;
		
		public override void _Ready()
		{
			InitializeGameUI();
			InitializeCombatSystem();
			InitializeTimers();
			ConnectSignals();

			// 设置游戏为运行状态 - 添加空引用检查
			if (GameManager.Instance != null)
			{
				GameManager.Instance.SetGameRunning(true);
			}
			else
			{
				GD.Print("[GameScene] 警告: GameManager.Instance 为 null，游戏状态可能无法正确设置");
				GD.Print("[GameScene] 建议: 请检查 GameManager 是否已正确配置为自动加载");
			}
		}
		
		private void InitializeGameUI()
		{
			// 创建新的UI组件
			InitializeNewUIComponents();

			// 创建游戏网格 (5行9列)
			_gameGrid = new GridContainer();
			_gameGrid.Columns = 9;
			_gameGrid.Position = new Vector2I(300, 180); // 调整位置以适应新的UI布局
			AddChild(_gameGrid);

			// 创建草坪格子
			for (int row = 0; row < 5; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					var lawnSlot = new LawnSlot();
					lawnSlot.GridPosition = new Vector2I(col, row);
					lawnSlot.Name = $"LawnSlot_{row}_{col}";

					_gameGrid.AddChild(lawnSlot);
				}
			}

			// 创建击杀数显示（保留原有的统计显示）
			_zombieCountLabel = new Label();
			_zombieCountLabel.Text = $"击杀: {_zombieKillCount}";
			_zombieCountLabel.Position = new Vector2I(50, 120);
			_zombieCountLabel.Modulate = Colors.Red;
			_zombieCountLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_zombieCountLabel);

			// 创建战斗统计显示（保留原有的统计显示）
			_combatStatsLabel = new Label();
			_combatStatsLabel.Text = "战斗: 爆炸0 伤害0";
			_combatStatsLabel.Position = new Vector2I(50, 150);
			_combatStatsLabel.Modulate = Colors.Cyan;
			_combatStatsLabel.AddThemeStyleboxOverride("normal", CreateLabelStyleBox());
			AddChild(_combatStatsLabel);
		}

		/// <summary>
		/// 初始化新的UI组件
		/// </summary>
		private void InitializeNewUIComponents()
		{
			// 创建阳光计数器
			_sunCounter = new SunCounter();
			_sunCounter.Position = new Vector2I(50, 20);
			_sunCounter.SetSun(50); // 初始阳光
			AddChild(_sunCounter);

			// 创建植物卡片选择器
			_plantCardSelector = new PlantCardSelector();
			AddChild(_plantCardSelector);

			// 创建暂停菜单
			_pauseMenu = new PauseMenu();
			AddChild(_pauseMenu);

			// 连接新UI组件的信号
			_sunCounter.SunCollected += OnSunCollectedFromUI;
			_pauseMenu.Resumed += OnPauseMenuResumed;
			_pauseMenu.RestartRequested += OnPauseMenuRestartRequested;
			_pauseMenu.MainMenuRequested += OnPauseMenuMainMenuRequested;
			_pauseMenu.ExitRequested += OnPauseMenuExitRequested;
		}

		/// <summary>
		/// 创建标签样式框
		/// </summary>
		private StyleBoxFlat CreateLabelStyleBox()
		{
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = new Color(0, 0, 0, 0.5f);
			styleBox.CornerRadiusTopLeft = 5;
			styleBox.CornerRadiusTopRight = 5;
			styleBox.CornerRadiusBottomLeft = 5;
			styleBox.CornerRadiusBottomRight = 5;
			return styleBox;
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
			
			// 阳光掉落计时器（每8秒）
			_sunDropTimer = new Timer();
			_sunDropTimer.WaitTime = SUN_DROP_INTERVAL;
			_sunDropTimer.Timeout += OnSunDropTimer;
			_sunDropTimer.Autostart = true;
			AddChild(_sunDropTimer);
			
			// 战斗统计更新计时器（每1秒）
			_combatStatsUpdateTimer = new Timer();
			_combatStatsUpdateTimer.WaitTime = 1.0f;
			_combatStatsUpdateTimer.Timeout += UpdateCombatStats;
			_combatStatsUpdateTimer.Autostart = true;
			AddChild(_combatStatsUpdateTimer);
			
			// 预加载阳光场景
			_sunScene = GD.Load<PackedScene>("res://Scenes/Game/Sun.tscn");
			if (_sunScene == null)
			{
				GD.Print("[GameScene] 警告: 无法加载阳光场景，将动态创建");
			}
		}
		
		private void ConnectSignals()
		{
			// 连接游戏相关信号
			// 这里可以连接植物种植、僵尸生成等信号
		}
		
		public void AddSun(int amount)
		{
			if (_sunCounter != null)
			{
				_sunCounter.AddSun(amount);
			}
		}

		public bool SpendSun(int amount)
		{
			if (_sunCounter != null)
			{
				return _sunCounter.SpendSun(amount);
			}
			return false;
		}

		public bool HasEnoughSun(int amount)
		{
			if (_sunCounter != null)
			{
				return _sunCounter.HasEnoughSun(amount);
			}
			return false;
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
			GD.Print("[GameScene] 自动阳光生成: +25");
		}
		
		/// <summary>
		/// 阳光掉落计时器回调
		/// </summary>
		private void OnSunDropTimer()
		{
			DropSunFromSky();
		}
		
		/// <summary>
		/// 从天空掉落阳光
		/// </summary>
		private void DropSunFromSky()
		{
			Sun sun;
			
			// 尝试从场景加载，如果失败则动态创建
			if (_sunScene != null)
			{
				sun = _sunScene.Instantiate<Sun>();
			}
			else
			{
				sun = new Sun();
			}
			
			if (sun != null)
			{
				// 随机掉落位置
				var randomX = GD.RandRange(100, 1600);
				var startPos = new Vector2(randomX, -50); // 从屏幕上方
				var targetY = GD.RandRange(400, 600); // 草坪区域高度
				
				// 设置阳光参数
				sun.SunValue = 25;
				sun.Name = "Sun_" + Time.GetUnixTimeFromSystem();
				
				// 连接信号
				sun.SunCollected += OnSunCollected;
				sun.SunExpired += OnSunExpired;
				
				// 添加到场景
				AddChild(sun);
				
				// 开始掉落
				sun.StartFalling(startPos, targetY);
				
				GD.Print($"[GameScene] 阳光掉落: 位置 X={randomX}");
			}
			else
			{
				GD.Print("[GameScene] 错误: 无法创建阳光对象");
			}
		}
		
		/// <summary>
		/// 阳光被收集的回调
		/// </summary>
		/// <param name="value">阳光值</param>
		private void OnSunCollected(int value)
		{
			AddSun(value);
			GD.Print($"[GameScene] 阳光收集成功: +{value}");
		}
		
		/// <summary>
		/// 阳光过期的回调
		/// </summary>
		private void OnSunExpired()
		{
			GD.Print("[GameScene] 阳光过期未收集");
		}

		/// <summary>
		/// UI阳光收集回调
		/// </summary>
		/// <param name="value">阳光值</param>
		private void OnSunCollectedFromUI(int value)
		{
			// 这里可以添加收集阳光的额外效果
			GD.Print($"[GameScene] UI阳光收集: +{value}");
		}

		/// <summary>
		/// 暂停菜单恢复回调
		/// </summary>
		private void OnPauseMenuResumed()
		{
			GD.Print("[GameScene] 游戏恢复");
		}

		/// <summary>
		/// 暂停菜单重启请求回调
		/// </summary>
		private void OnPauseMenuRestartRequested()
		{
			GD.Print("[GameScene] 重新开始游戏");
			GetTree().ReloadCurrentScene();
		}

		/// <summary>
		/// 暂停菜单返回主菜单请求回调
		/// </summary>
		private void OnPauseMenuMainMenuRequested()
		{
			GD.Print("[GameScene] 返回主菜单");
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		}

		/// <summary>
		/// 暂停菜单退出请求回调
		/// </summary>
		private void OnPauseMenuExitRequested()
		{
			GD.Print("[GameScene] 退出游戏");
			GetTree().Quit();
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
				if (mouseEvent.Pressed)
				{
					switch (mouseEvent.ButtonIndex)
					{
						case MouseButton.Left:
							HandleLeftClick(mouseEvent);
							break;
						case MouseButton.Right:
							// 右键点击创建测试爆炸
							CreateTestExplosion(GetGlobalMousePosition());
							break;
						case MouseButton.Middle:
							// 中键点击发射测试豌豆
							var startPos = new Vector2(350, 300); // 从左侧发射
							var direction = Vector2.Right;
							CreateTestPea(startPos, direction);
							break;
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
		}

		/// <summary>
		/// 处理左键点击
		/// </summary>
		private void HandleLeftClick(InputEventMouseButton mouseEvent)
		{
			var globalMousePos = GetGlobalMousePosition();

			// 检查是否点击在草坪网格内
			var gridPos = WorldToGridPosition(globalMousePos);
			if (IsValidGridPosition(gridPos))
			{
				// 如果有选中的植物卡片，尝试种植
				if (_plantCardSelector != null && _plantCardSelector.SelectedCard != null)
				{
					if (_plantCardSelector.UseSelectedPlant(gridPos))
					{
						// 播放种植动画
						PlayPlantAnimation(gridPos);
					}
					else
					{
						// 阳光不足，播放警告动画
						if (_sunCounter != null)
						{
							_sunCounter.PlayWarningAnimation();
						}
					}
				}
				else if (mouseEvent.DoubleClick)
				{
					// 双击直接种植樱桃炸弹（保留原有功能）
					PlantCherryBomb(gridPos);
				}
			}
		}

		/// <summary>
		/// 播放种植动画
		/// </summary>
		private void PlayPlantAnimation(Vector2I gridPosition)
		{
			// 找到对应的草坪格子并播放动画
			var lawnSlotName = $"LawnSlot_{gridPosition.Y}_{gridPosition.X}";
			var lawnSlot = GetNodeOrNull(lawnSlotName) as LawnSlot;
			if (lawnSlot != null)
			{
				lawnSlot.PlayPlantAnimation();
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
			// 设置游戏为停止状态 - 添加空引用检查
			if (GameManager.Instance != null)
			{
				GameManager.Instance.SetGameRunning(false);
			}
			else
			{
				GD.Print("[GameScene] 警告: GameManager.Instance 为 null，无法设置游戏状态");
			}

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
