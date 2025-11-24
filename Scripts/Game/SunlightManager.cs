using Godot;
using PlantsVsZombies.Core;
using System;
using System.Collections.Generic;

namespace Plants大战僵尸.Scripts.Game
{
    /// <summary>
    /// 阳光管理器
    /// 管理游戏中的阳光数量、生成和收集逻辑
    /// </summary>
    public partial class SunlightManager : Node
    {
        #region Constants

        /// <summary>
        /// 最大阳光数量
        /// </summary>
        public const int MAX_SUNLIGHT = 999;

        /// <summary>
        /// 每个阳光的价值
        /// </summary>
        public const int SUNLIGHT_VALUE = 25;

        /// <summary>
        /// 天降阳光间隔时间（秒）
        /// </summary>
        public const float SKY_DROP_INTERVAL = 10f;

        /// <summary>
        /// 天降阳光最大下落高度（屏幕2/3位置）
        /// </summary>
        public const float MAX_FALL_HEIGHT = 720f; // 1080 * 2/3

        /// <summary>
        /// 太阳花产生阳光的间隔时间（秒）
        /// </summary>
        public const float SUNFLOWER_PRODUCTION_INTERVAL = 5f;

        #endregion

        #region Events

        /// <summary>
        /// 阳光数量变化事件
        /// </summary>
        public event Action<int> OnSunlightChanged;

        /// <summary>
        /// 阳光收集事件
        /// </summary>
        public event Action<int> OnSunlightCollected;

        #endregion

        #region Private Fields

        /// <summary>
        /// 当前阳光数量
        /// </summary>
        private int _currentSunlight = 50; // 初始阳光

        /// <summary>
        /// 天降阳光计时器
        /// </summary>
        private Timer _skyDropTimer;

        /// <summary>
        /// 活跃的阳光对象列表
        /// </summary>
        private List<Sun> _activeSuns = new List<Sun>();

        /// <summary>
        /// 太阳花列表
        /// </summary>
        private List<Node> _sunflowers = new List<Node>();

        #endregion

        #region Public Properties

        /// <summary>
        /// 当前阳光数量
        /// </summary>
        public int CurrentSunlight
        {
            get => _currentSunlight;
            private set
            {
                var oldValue = _currentSunlight;
                _currentSunlight = Mathf.Clamp(value, 0, MAX_SUNLIGHT);
                
                if (oldValue != _currentSunlight)
                {
                    OnSunlightChanged?.Invoke(_currentSunlight);
                    GD.Print($"阳光数量更新: {oldValue} -> {_currentSunlight}");
                }
            }
        }

        /// <summary>
        /// 是否达到最大阳光数量
        /// </summary>
        public bool IsMaxSunlight => CurrentSunlight >= MAX_SUNLIGHT;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            base._Ready();
            GD.Print("初始化阳光管理器");
            
            InitializeSystems();
            
            // 初始触发阳光变化事件
            OnSunlightChanged?.Invoke(CurrentSunlight);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            
            // 清理已销毁的阳光对象
            CleanupDestroyedSuns();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            
            // 清理资源
            _skyDropTimer?.QueueFree();
            foreach (var sun in _activeSuns)
            {
                sun?.QueueFree();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化系统组件
        /// </summary>
        private void InitializeSystems()
        {
            SetupSkyDropTimer();
            GD.Print($"阳光管理器初始化完成，初始阳光: {CurrentSunlight}");
        }

        /// <summary>
        /// 设置天降阳光计时器
        /// </summary>
        private void SetupSkyDropTimer()
        {
            _skyDropTimer = new Timer();
            _skyDropTimer.WaitTime = SKY_DROP_INTERVAL;
            _skyDropTimer.Timeout += OnGenerateSkySunlight;
            _skyDropTimer.Autostart = true;
            AddChild(_skyDropTimer);
            
            GD.Print($"天降阳光计时器已设置，间隔: {SKY_DROP_INTERVAL}秒");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 添加阳光
        /// </summary>
        /// <param name="amount">添加数量</param>
        public void AddSunlight(int amount)
        {
            if (amount <= 0) return;
            
            CurrentSunlight += amount;
            OnSunlightCollected?.Invoke(amount);
        }

        /// <summary>
        /// 检查是否能够承担指定成本
        /// </summary>
        /// <param name="cost">成本</param>
        /// <returns>是否能够承担</returns>
        public bool CanAfford(int cost)
        {
            return CurrentSunlight >= cost;
        }

        /// <summary>
        /// 花费阳光
        /// </summary>
        /// <param name="cost">花费数量</param>
        /// <returns>是否花费成功</returns>
        public bool SpendSunlight(int cost)
        {
            if (!CanAfford(cost))
            {
                GD.Print($"阳光不足，无法花费 {cost}（当前: {CurrentSunlight}）");
                return false;
            }

            CurrentSunlight -= cost;
            GD.Print($"成功花费阳光: {cost}，剩余: {CurrentSunlight}");
            return true;
        }

        /// <summary>
        /// 从太阳花生产阳光
        /// </summary>
        /// <param name="sunflowerPosition">太阳花位置</param>
        public void ProduceFromSunflower(Vector2 sunflowerPosition)
        {
            if (IsMaxSunlight)
            {
                GD.Print("阳光已达到最大值，跳过太阳花生产");
                return;
            }

            CreateSunlightAtPosition(sunflowerPosition, false);
            GD.Print($"太阳花在位置 {sunflowerPosition} 生产了阳光");
        }

        /// <summary>
        /// 注册太阳花
        /// </summary>
        /// <param name="sunflower">太阳花节点</param>
        public void RegisterSunflower(Node sunflower)
        {
            if (!_sunflowers.Contains(sunflower))
            {
                _sunflowers.Add(sunflower);
                GD.Print($"注册太阳花: {sunflower.Name}");
            }
        }

        /// <summary>
        /// 注销太阳花
        /// </summary>
        /// <param name="sunflower">太阳花节点</param>
        public void UnregisterSunflower(Node sunflower)
        {
            if (_sunflowers.Contains(sunflower))
            {
                _sunflowers.Remove(sunflower);
                GD.Print($"注销太阳花: {sunflower.Name}");
            }
        }

        /// <summary>
        /// 立即生成一个天降阳光（用于测试）
        /// </summary>
        public void GenerateImmediateSkySunlight()
        {
            OnGenerateSkySunlight();
        }

        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        /// <returns>状态信息</returns>
        public string GetSystemStatus()
        {
            return $"阳光管理器状态:\n" +
                   $"当前阳光: {CurrentSunlight}/{MAX_SUNLIGHT}\n" +
                   $"活跃阳光数: {_activeSuns.Count}\n" +
                   $"注册太阳花数: {_sunflowers.Count}\n" +
                   $"天降计时器: {(_skyDropTimer?.TimeLeft ?? 0f):F1}秒";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 天降阳光计时器回调
        /// </summary>
        private void OnGenerateSkySunlight()
        {
            if (IsMaxSunlight)
            {
                GD.Print("阳光已达到最大值，跳过天降阳光生成");
                return;
            }

            Vector2 dropPosition = GetRandomSkyPosition();
            CreateSunlightAtPosition(dropPosition, true);
        }

        /// <summary>
        /// 获取随机天空位置
        /// </summary>
        /// <returns>随机位置</returns>
        private Vector2 GetRandomSkyPosition()
        {
            // 在屏幕顶部随机位置生成阳光
            float x = (float)GD.RandRange(100f, 1820f); // 避免屏幕边缘
            float y = 50f; // 从顶部开始
            return new Vector2(x, y);
        }

        /// <summary>
        /// 在指定位置创建阳光
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="isSkyDrop">是否为天降阳光</param>
        private void CreateSunlightAtPosition(Vector2 position, bool isSkyDrop)
        {
            var sunlight = new Sun();
            sunlight.Position = position;
            _activeSuns.Add(sunlight);
            
            // 订阅收集事件
            sunlight.SunCollected += OnSunCollectedHandler;

            // 添加到场景
            var gameScene = GetTree().CurrentScene;
            if (gameScene != null)
            {
                gameScene.AddChild(sunlight);
                GD.Print($"创建阳光: 位置{position}, 天降:{isSkyDrop}");
            }
            else
            {
                GD.PrintErr("无法获取当前场景，阳光创建失败");
            }
        }

        /// <summary>
        /// 阳光被收集事件处理
        /// </summary>
        /// <param name="value">阳光价值</param>
        private void OnSunCollectedHandler(int value)
        {
            AddSunlight(value);

            // 找到并移除对应的阳光对象
            if (_activeSuns.Count > 0)
            {
                var sunToRemove = _activeSuns[0]; // 简化处理，移除第一个
                _activeSuns.Remove(sunToRemove);
                CreateCollectEffect(sunToRemove.GlobalPosition);
            }

            GD.Print($"阳光被收集，价值: {value}, 当前总数: {CurrentSunlight}");
        }
        /// <summary>
        /// 创建阳光收集效果
        /// </summary>
        /// <param name="position">收集位置</param>
        private void CreateCollectEffect(Vector2 position)
        {
            var effectNode = new Node2D();
            effectNode.Position = position;

            // 创建视觉效果
            var effectSprite = new Sprite2D();
            var effectImage = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
            
            // 绘制收集效果（简单的圆圈扩散效果）
            var center = new Vector2I(32, 32);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    var pos = new Vector2I(x, y);
                    var distance = pos.DistanceTo(center);
                    
                    if (distance <= 30f)
                    {
                        var alpha = (1.0f - distance / 30f) * 0.6f;
                        var color = new Color(1.0f, 1.0f, 0.2f, alpha);
                        effectImage.SetPixel(x, y, color);
                    }
                }
            }
            
            effectSprite.Texture = ImageTexture.CreateFromImage(effectImage);
            effectSprite.Centered = true;
            effectNode.AddChild(effectSprite);

            // 添加到场景
            GetTree().Root.AddChild(effectNode);

            // 播放动画
            var tween = effectNode.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(effectNode, "scale", Vector2.One * 2f, 0.3f);
            tween.TweenProperty(effectNode, "modulate:a", 0f, 0.3f);
            tween.TweenCallback(Callable.From(() => effectNode.QueueFree()));
        }

        /// <summary>
        /// 清理已销毁的阳光对象
        /// </summary>
        private void CleanupDestroyedSuns()
        {
            for (int i = _activeSuns.Count - 1; i >= 0; i--)
            {
                var sun = _activeSuns[i];
                if (!IsInstanceValid(sun) || sun.IsQueuedForDeletion())
                {
                    _activeSuns.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 重置阳光管理器（用于重新开始游戏）
        /// </summary>
        public void Reset()
        {
            CurrentSunlight = 50; // 重置为初始值
            
            // 清理所有活跃的阳光
            foreach (var sun in _activeSuns)
            {
                if (IsInstanceValid(sun))
                {
                    sun.QueueFree();
                }
            }
            _activeSuns.Clear();
            
            // 重置太阳花列表
            _sunflowers.Clear();
            
            // 重启天降计时器
            if (_skyDropTimer != null)
            {
                _skyDropTimer.Stop();
                _skyDropTimer.Start();
            }
            
            GD.Print("阳光管理器已重置");
        }

        /// <summary>
        /// 暂停阳光系统
        /// </summary>
        public void Pause()
        {
            if (_skyDropTimer != null)
            {
                _skyDropTimer.Paused = true;
            }
            GD.Print("阳光系统已暂停");
        }

        /// <summary>
        /// 恢复阳光系统
        /// </summary>
        public void Resume()
        {
            if (_skyDropTimer != null)
            {
                _skyDropTimer.Paused = false;
            }
            GD.Print("阳光系统已恢复");
        }

        #endregion
    }
}
