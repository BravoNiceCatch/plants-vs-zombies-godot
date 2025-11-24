using Godot;
using PlantsVsZombies.Game;
using PlantsVsZombies.Scripts.Combat;
using Plants大战僵尸.Scripts.Game;
using Plants大战僵尸.Scripts.Plants;
using System;

namespace PlantsVsZombies.Core
{
	public partial class GameManager : Node
	{
		#region Singleton
		public static GameManager Instance { get; private set; }
		#endregion

		#region Private Fields
		private GameScene _currentGameScene;
		private bool _isPaused = false;
		private int _zombieKillCount = 0;
		private bool _isGameRunning = false;

		// 系统管理器引用
		private SunlightManager _sunlightManager;
		private PlantPurchaseSystem _plantPurchaseSystem;
		#endregion

		#region Public Properties
		public GameScene CurrentGameScene => _currentGameScene;
		public bool IsPaused => _isPaused;
		public int ZombieKillCount => _zombieKillCount;
		public bool IsGameRunning => _isGameRunning;

		// 系统访问器
		public SunlightManager SunlightManager => _sunlightManager;
		public PlantPurchaseSystem PlantPurchaseSystem => _plantPurchaseSystem;
		#endregion

		#region Godot Lifecycle
		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				ProcessMode = ProcessModeEnum.Always;

				InitializeGameSystems();
				GD.Print("[GameManager] 游戏管理器初始化完成");
			}
			else
			{
				QueueFree();
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("ui_cancel")) // ESC键
			{
				TogglePause();
			}
		}

		public override void _ExitTree()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
		#endregion

		#region Initialization
		/// <summary>
		/// 初始化所有游戏系统
		/// </summary>
		private void InitializeGameSystems()
		{
			try
			{
				InitializeCollisionManager();
				InitializeSunlightManager();
				InitializePlantPurchaseSystem();
				
				GD.Print("[GameManager] 所有游戏系统初始化完成");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[GameManager] 系统初始化失败: {ex.Message}");
			}
		}

		/// <summary>
		/// 初始化阳光管理器
		/// </summary>
		private void InitializeSunlightManager()
		{
			_sunlightManager = new SunlightManager();
			_sunlightManager.Name = "SunlightManager";
			AddChild(_sunlightManager);
			
			// 订阅阳光事件
			_sunlightManager.OnSunlightChanged += OnSunlightChanged;
			_sunlightManager.OnSunlightCollected += OnSunlightCollected;
			
			GD.Print("[GameManager] 阳光管理器初始化成功");
		}

		/// <summary>
		/// 初始化植物购买系统
		/// </summary>
		private void InitializePlantPurchaseSystem()
		{
			_plantPurchaseSystem = new PlantPurchaseSystem();
			_plantPurchaseSystem.Name = "PlantPurchaseSystem";
			AddChild(_plantPurchaseSystem);
			
			// 订阅购买事件
			_plantPurchaseSystem.OnPlantSelected += OnPlantSelected;
			_plantPurchaseSystem.OnPlantPurchased += OnPlantPurchased;
			_plantPurchaseSystem.OnPurchaseFailed += OnPurchaseFailed;
			_plantPurchaseSystem.OnPlantDeselected += OnPlantDeselected;
			
			GD.Print("[GameManager] 植物购买系统初始化成功");
		}

		/// <summary>
		/// 初始化碰撞管理器
		/// </summary>
		private void InitializeCollisionManager()
		{
			var collisionManagerScene = GD.Load<PackedScene>("res://Scripts/Combat/CollisionManager.cs");
			if (collisionManagerScene != null)
			{
				var collisionManager = collisionManagerScene.Instantiate<CollisionManager>();
				collisionManager.Name = "CollisionManager";
				AddChild(collisionManager);
				GD.Print("[GameManager] 碰撞管理器初始化成功");
			}
			else
			{
				// 如果无法加载场景，直接创建实例
				var collisionManager = new CollisionManager();
				collisionManager.Name = "CollisionManager";
				AddChild(collisionManager);
				GD.Print("[GameManager] 碰撞管理器直接创建成功");
			}
		}
		#endregion

		#region Game Control Methods
		/// <summary>
		/// 暂停/恢复游戏
		/// </summary>
		public void TogglePause()
		{
			_isPaused = !_isPaused;
			GetTree().Paused = _isPaused;
			
			// 暂停/恢复各个系统
			if (_sunlightManager != null)
			{
				if (_isPaused)
				{
					_sunlightManager.Pause();
				}
				else
				{
					_sunlightManager.Resume();
				}
			}
			
			// TODO: 显示/隐藏暂停菜单
			GD.Print($"[GameManager] 游戏暂停状态: {_isPaused}");
		}

		/// <summary>
		/// 开始新游戏
		/// </summary>
		public void StartNewGame()
		{
			ResetGame();
			GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
			GD.Print("[GameManager] 开始新游戏");
		}

		/// <summary>
		/// 返回主菜单
		/// </summary>
		public void ReturnToMainMenu()
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
			GD.Print("[GameManager] 返回主菜单");
		}

		/// <summary>
		/// 退出游戏
		/// </summary>
		public void QuitGame()
		{
			GetTree().Quit();
			GD.Print("[GameManager] 退出游戏");
		}

		/// <summary>
		/// 重置游戏状态
		/// </summary>
		public void ResetGame()
		{
			ResetKillCount();
			SetGameRunning(false);
			
			// 重置各个系统
			if (_sunlightManager != null)
			{
				_sunlightManager.Reset();
			}
			
			if (_plantPurchaseSystem != null)
			{
				// 重置植物购买系统状态（如果需要的话）
				_plantPurchaseSystem.DeselectPlant();
			}
			
			GD.Print("[GameManager] 游戏状态已重置");
		}
		#endregion

		#region Zombie Management
		/// <summary>
		/// 增加击杀数量
		/// </summary>
		public void IncrementKillCount()
		{
			_zombieKillCount++;
			GD.Print($"[GameManager] 击杀僵尸数量: {_zombieKillCount}");

			// 通知游戏场景更新显示
			_currentGameScene = GetTree().CurrentScene as GameScene;
			if (_currentGameScene != null)
			{
				_currentGameScene.OnZombieKilled();
			}
		}

		/// <summary>
		/// 获取击杀数量
		/// </summary>
		public int GetKillCount()
		{
			return _zombieKillCount;
		}

		/// <summary>
		/// 重置击杀数量
		/// </summary>
		public void ResetKillCount()
		{
			_zombieKillCount = 0;
			GD.Print("[GameManager] 击杀数量已重置");
		}
		#endregion

		#region Game State Management
		/// <summary>
		/// 设置游戏运行状态
		/// </summary>
		public void SetGameRunning(bool running)
		{
			_isGameRunning = running;
			GD.Print($"[GameManager] 游戏状态设置为: {(running ? "运行中" : "已停止")}");
		}

		/// <summary>
		/// 设置当前游戏场景
		/// </summary>
		public void SetCurrentGameScene(GameScene gameScene)
		{
			_currentGameScene = gameScene;
			GD.Print($"[GameManager] 当前游戏场景已设置");
		}
		#endregion

		#region Camera Effects
		/// <summary>
		/// 相机震动效果
		/// </summary>
		public void ShakeCamera(float intensity, float duration)
		{
			// TODO: 实现相机震动效果
			GD.Print($"[GameManager] 相机震动: 强度={intensity}, 持续时间={duration}");
		}
		#endregion

		#region Event Handlers
		/// <summary>
		/// 阳光数量变化事件处理
		/// </summary>
		private void OnSunlightChanged(int newAmount)
		{
			// 通知UI更新显示
			_currentGameScene = GetTree().CurrentScene as GameScene;
			if (_currentGameScene != null)
			{
				// TODO: 实现 GameScene.OnSunlightChanged 方法
				// _currentGameScene.OnSunlightChanged(newAmount);
			}
			
			GD.Print($"[GameManager] 阳光数量变化: {newAmount}");
		}

		/// <summary>
		/// 阳光收集事件处理
		/// </summary>
		private void OnSunlightCollected(int amount)
		{
			GD.Print($"[GameManager] 收集了 {amount} 阳光");
		}

		/// <summary>
		/// 植物选择事件处理
		/// </summary>
		private void OnPlantSelected(PlantType plantType)
		{
			GD.Print($"[GameManager] 植物已选择: {plantType.GetDisplayName()}");
		}

		/// <summary>
		/// 植物购买事件处理
		/// </summary>
		private void OnPlantPurchased(PlantType plantType, Vector2Int gridPosition)
		{
			GD.Print($"[GameManager] 植物已购买: {plantType.GetDisplayName()} 位置: {gridPosition}");
			
			// 可以在这里添加购买成功的音效、特效等
		}

		/// <summary>
		/// 植物购买失败事件处理
		/// </summary>
		private void OnPurchaseFailed(PlantType plantType, string reason)
		{
			GD.Print($"[GameManager] 植物购买失败: {plantType.GetDisplayName()} 原因: {reason}");
			
			// 可以在这里添加购买失败的音效、UI提示等
		}

		/// <summary>
		/// 植物取消选择事件处理
		/// </summary>
		private void OnPlantDeselected()
		{
			GD.Print("[GameManager] 植物选择已取消");
		}
		#endregion

		#region Utility Methods
		/// <summary>
		/// 获取系统状态信息
		/// </summary>
		public string GetSystemStatus()
		{
			var status = "=== 游戏管理器状态 ===\n";
			status += $"游戏运行: {IsGameRunning}\n";
			status += $"游戏暂停: {IsPaused}\n";
			status += $"击杀数量: {ZombieKillCount}\n\n";

			if (_sunlightManager != null)
			{
				status += _sunlightManager.GetSystemStatus() + "\n";
			}

			if (_plantPurchaseSystem != null)
			{
				status += _plantPurchaseSystem.GetSystemStatus();
			}

			return status;
		}

		/// <summary>
		/// 调试命令 - 立即生成阳光
		/// </summary>
		public void DebugGenerateSunlight()
		{
			if (_sunlightManager != null)
			{
				_sunlightManager.GenerateImmediateSkySunlight();
				GD.Print("[GameManager] 调试: 立即生成天降阳光");
			}
		}

		/// <summary>
		/// 调试命令 - 添加指定数量阳光
		/// </summary>
		public void DebugAddSunlight(int amount)
		{
			if (_sunlightManager != null)
			{
				_sunlightManager.AddSunlight(amount);
				GD.Print($"[GameManager] 调试: 添加 {amount} 阳光");
			}
		}
		#endregion
	}
}
