using Godot;
using PlantsVsZombies.Game;

namespace PlantsVsZombies.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        
        private GameScene _currentGameScene;
        private bool _isPaused = false;
        
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                ProcessMode = ProcessModeEnum.Always;
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
    }
}