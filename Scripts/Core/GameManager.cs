using Godot;
using PlantsVsZombies.Game;
using PlantsVsZombies.Scripts.Combat;

namespace PlantsVsZombies.Core
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }

		private GameScene _currentGameScene;
		private bool _isPaused = false;
		private int _zombieKillCount = 0;
		private bool _isGameRunning = false;
		
		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				ProcessMode = ProcessModeEnum.Always;

				// 初始化碰撞管理器
				InitializeCollisionManager();
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
		
		public void TogglePause()
		{
			_isPaused = !_isPaused;
			GetTree().Paused = _isPaused;
			
			// TODO: 显示/隐藏暂停菜单
		}
		
		public void StartNewGame()
		{
			GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
		}
		
		public void ReturnToMainMenu()
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		}
		
		public void QuitGame()
		{
			GetTree().Quit();
		}

		// 僵尸相关功能
		public void IncrementKillCount()
		{
			_zombieKillCount++;

			// 通知游戏场景更新显示
			var gameScene = GetTree().CurrentScene as GameScene;
			if (gameScene != null)
			{
				gameScene.OnZombieKilled();
			}
		}

		public int GetKillCount()
		{
			return _zombieKillCount;
		}

		public void ResetKillCount()
		{
			_zombieKillCount = 0;
		}

		// 游戏状态管理
		public bool IsGameRunning => _isGameRunning;

		public void SetGameRunning(bool running)
		{
			_isGameRunning = running;
			GD.Print($"[GameManager] 游戏状态设置为: {(running ? "运行中" : "已停止")}");
		}

		// 相机震动效果
		public void ShakeCamera(float intensity, float duration)
		{
			// TODO: 实现相机震动效果
			GD.Print($"[GameManager] 相机震动: 强度={intensity}, 持续时间={duration}");
		}

		// 初始化碰撞管理器
		private void InitializeCollisionManager()
		{
			var collisionManagerScene = GD.Load<PackedScene>("res://Scripts/Combat/CollisionManager.cs");
			if (collisionManagerScene != null)
			{
				var collisionManager = collisionManagerScene.Instantiate<CollisionManager>();
				AddChild(collisionManager);
				GD.Print("[GameManager] 碰撞管理器初始化成功");
			}
			else
			{
				// 如果无法加载场景，直接创建实例
				var collisionManager = new CollisionManager();
				AddChild(collisionManager);
				GD.Print("[GameManager] 碰撞管理器直接创建成功");
			}
		}
	}
}
