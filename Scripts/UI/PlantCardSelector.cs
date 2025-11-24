using Godot;
using System.Collections.Generic;
using PlantsVsZombies.Game;
using Plants大战僵尸.Scripts.Plants;

namespace PlantsVsZombies.UI
{
    /// <summary>
    /// 植物卡片选择器 - 管理所有植物卡片的选择逻辑
    /// </summary>
    public partial class PlantCardSelector : Control
    {
        private HBoxContainer _cardContainer;
        private List<PlantCard> _plantCards = new List<PlantCard>();
        private PlantCard _selectedCard = null;
        private GameScene _gameScene;

        // UI组件
        private Panel _backgroundPanel;
        private Label _titleLabel;

        public PlantCard SelectedCard => _selectedCard;

        public override void _Ready()
        {
            InitializeComponents();
            SetupVisuals();
            LoadPlantCards();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 设置选择器大小和位置
            Size = new Vector2I(1200, 140);
            Position = new Vector2I(360, 20);

            // 创建背景面板
            _backgroundPanel = new Panel();
            _backgroundPanel.Size = Size;
            AddChild(_backgroundPanel);

            // 创建标题标签
            _titleLabel = new Label();
            _titleLabel.Text = "植物选择";
            _titleLabel.Position = new Vector2I(10, 5);
            _titleLabel.Size = new Vector2I(100, 25);
            _titleLabel.Modulate = Colors.LightGreen;
            AddChild(_titleLabel);

            // 创建卡片容器
            _cardContainer = new HBoxContainer();
            _cardContainer.Position = new Vector2I(120, 10);
            _cardContainer.Size = new Vector2I(1070, 120);
            _cardContainer.AddThemeConstantOverride("separation", 15);
            AddChild(_cardContainer);

            // 获取游戏场景引用
            _gameScene = GetTree().CurrentScene as GameScene;
        }

        /// <summary>
        /// 设置视觉效果
        /// </summary>
        private void SetupVisuals()
        {
            // 设置背景样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.2f, 0.1f, 0.95f);
            styleBox.BorderColor = Colors.DarkGreen;
            styleBox.BorderWidthLeft = 3;
            styleBox.BorderWidthRight = 3;
            styleBox.BorderWidthTop = 3;
            styleBox.BorderWidthBottom = 3;
            styleBox.CornerRadiusTopLeft = 12;
            styleBox.CornerRadiusTopRight = 12;
            styleBox.CornerRadiusBottomLeft = 12;
            styleBox.CornerRadiusBottomRight = 12;

            // 添加内发光效果
            styleBox.ShadowColor = new Color(0, 1, 0, 0.3f);
            styleBox.ShadowSize = 2;
            styleBox.ShadowOffset = new Vector2I(1, 1);

            _backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);
        }

        /// <summary>
        /// 加载植物卡片
        /// </summary>
        private void LoadPlantCards()
        {
            // 创建向日葵卡片
            var sunflowerCard = CreateSunflowerCard();
            AddPlantCard(sunflowerCard);

            // 创建豌豆射手卡片
            var peashooterCard = CreatePeashooterCard();
            AddPlantCard(peashooterCard);

            // 创建樱桃炸弹卡片
            var cherryBombCard = CreateCherryBombCard();
            AddPlantCard(cherryBombCard);

            // 创建坚果墙卡片（示例）
            var wallNutCard = CreateWallNutCard();
            AddPlantCard(wallNutCard);
        }

        /// <summary>
        /// 创建向日葵卡片
        /// </summary>
        private PlantCard CreateSunflowerCard()
        {
            var card = new PlantCard();
            card.PlantType = PlantType.Sunflower;
            card.PlantScene = GD.Load<PackedScene>("res://Scenes/Plants/Sunflower.tscn");

            return card;
        }

        /// <summary>
        /// 创建豌豆射手卡片
        /// </summary>
        private PlantCard CreatePeashooterCard()
        {
            var card = new PlantCard();
            card.PlantType = PlantType.Peashooter;
            card.PlantScene = GD.Load<PackedScene>("res://Scenes/Plants/Peashooter.tscn");

            return card;
        }

        /// <summary>
        /// 创建樱桃炸弹卡片
        /// </summary>
        private PlantCard CreateCherryBombCard()
        {
            var card = new PlantCard();
            card.PlantType = PlantType.CherryBomb;
            card.PlantScene = GD.Load<PackedScene>("res://Scenes/Plants/CherryBomb.tscn");

            return card;
        }

        /// <summary>
        /// 创建坚果墙卡片
        /// </summary>
        private PlantCard CreateWallNutCard()
        {
            var card = new PlantCard();
            card.PlantType = PlantType.None; // 暂时使用None，因为WallNut尚未实现
            // card.PlantScene = GD.Load<PackedScene>("res://Scenes/Plants/WallNut.tscn");

            return card;
        }

        /// <summary>
        /// 添加植物卡片
        /// </summary>
        private void AddPlantCard(PlantCard card)
        {
            _plantCards.Add(card);
            _cardContainer.AddChild(card);

            // 连接卡片信号
            card.CardSelected += OnCardSelected;
            card.CardDeselected += OnCardDeselected;
        }

        /// <summary>
        /// 卡片选择处理
        /// </summary>
        private void OnCardSelected(PlantCard card)
        {
            // 如果点击了已选中的卡片，取消选择
            if (_selectedCard == card)
            {
                card.DeselectCard();
                _selectedCard = null;
                return;
            }

            // 取消之前选中的卡片
            if (_selectedCard != null)
            {
                _selectedCard.DeselectCard();
            }

            // 选择新卡片
            _selectedCard = card;
            GD.Print($"[PlantCardSelector] 选择植物: {card.PlantName}");
        }

        /// <summary>
        /// 卡片取消选择处理
        /// </summary>
        private void OnCardDeselected(PlantCard card)
        {
            if (_selectedCard == card)
            {
                _selectedCard = null;
            }
        }

        /// <summary>
        /// 使用选中的植物
        /// </summary>
        /// <param name="gridPosition">种植位置</param>
        /// <returns>是否使用成功</returns>
        public bool UseSelectedPlant(Vector2I gridPosition)
        {
            if (_selectedCard == null || !_selectedCard.IsAvailable)
            {
                GD.Print("[PlantCardSelector] 没有选中的可用植物");
                return false;
            }

            if (_gameScene == null)
            {
                GD.Print("[PlantCardSelector] 游戏场景引用为空");
                return false;
            }

            // 检查阳光是否足够
            if (_gameScene.SpendSun(_selectedCard.SunCost))
            {
                // 播放使用动画
                _selectedCard.PlayUseAnimation();

                // 根据植物类型执行不同的种植逻辑
                var success = false;
                switch (_selectedCard.PlantName)
                {
                    case "樱桃炸弹":
                        success = _gameScene.PlantCherryBomb(gridPosition);
                        break;
                    // TODO: 添加其他植物的种植逻辑
                    default:
                        GD.Print($"[PlantCardSelector] 未实现的植物类型: {_selectedCard.PlantName}");
                        // 如果种植失败，退还阳光
                        _gameScene.AddSun(_selectedCard.SunCost);
                        return false;
                }

                if (success)
                {
                    // 开始冷却
                    _selectedCard.StartCooldown();

                    // 取消选择
                    _selectedCard.DeselectCard();
                    _selectedCard = null;

                    GD.Print($"[PlantCardSelector] 成功种植: {_selectedCard?.PlantName}");
                    return true;
                }
                else
                {
                    // 种植失败，退还阳光
                    _gameScene.AddSun(_selectedCard.SunCost);
                    GD.Print($"[PlantCardSelector] 种植失败: {_selectedCard.PlantName}");
                    return false;
                }
            }
            else
            {
                GD.Print($"[PlantCardSelector] 阳光不足: 需要 {_selectedCard.SunCost}");
                return false;
            }
        }

        /// <summary>
        /// 取消当前选择
        /// </summary>
        public void CancelSelection()
        {
            if (_selectedCard != null)
            {
                _selectedCard.DeselectCard();
                _selectedCard = null;
            }
        }

        /// <summary>
        /// 检查指定卡片是否可用
        /// </summary>
        public bool IsCardAvailable(string plantName)
        {
            foreach (var card in _plantCards)
            {
                if (card.PlantName == plantName)
                {
                    return card.IsAvailable && !card.IsOnCooldown;
                }
            }
            return false;
        }

        /// <summary>
        /// 更新所有卡片状态（根据阳光数量）
        /// </summary>
        public void UpdateCardAvailability(int currentSun)
        {
            foreach (var card in _plantCards)
            {
                if (!card.IsOnCooldown && currentSun >= card.SunCost)
                {
                    // 卡片通过阳光计数器来检查可用性
                    // 这里可以添加额外的状态更新逻辑
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            // ESC键取消选择
            if (@event.IsActionPressed("ui_cancel"))
            {
                CancelSelection();
            }
        }
    }
}