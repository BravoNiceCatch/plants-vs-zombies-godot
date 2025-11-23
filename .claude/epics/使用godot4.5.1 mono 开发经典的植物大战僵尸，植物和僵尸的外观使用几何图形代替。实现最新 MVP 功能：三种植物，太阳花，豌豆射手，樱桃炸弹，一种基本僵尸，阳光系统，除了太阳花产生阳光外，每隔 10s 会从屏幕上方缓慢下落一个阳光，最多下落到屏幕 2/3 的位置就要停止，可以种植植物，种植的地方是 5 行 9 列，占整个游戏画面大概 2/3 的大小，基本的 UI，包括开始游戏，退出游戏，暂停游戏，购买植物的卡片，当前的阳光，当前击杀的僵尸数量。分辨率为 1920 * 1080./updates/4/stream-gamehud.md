---
issue: 4
stream: GameHUD
agent: code-developer
started: 2025-11-23T08:32:54Z
status: in_progress
---

# Stream B: 游戏内HUD开发

## Scope
- 实现游戏内HUD (GameHUD)
- 开发植物选择卡片UI组件 (PlantCard)
- 显示当前阳光数量和击杀计数
- 暂停按钮功能
- 植物卡片状态更新逻辑

## Files
- `GameHUD.cs` - 游戏HUD主脚本
- `PlantCard.cs` - 植物卡片组件
- `GameHUD.tscn` - 游戏HUD场景
- `PlantCard.tscn` - 植物卡片场景
- `UI/game_hud/` - HUD资源目录

## Progress
- Starting implementation of GameHUD system
- Create PlantCard component with cost/selection states
- Implement sunlight display and kill counter
- Add pause button functionality
- Update plant card affordability states

## Technical Requirements
- Plant cards: Sunflower (50), Peashooter (100), Cherry Bomb (150)
- Visual feedback for selected/unaffordable states
- Real-time sunlight and kill count updates
- Proper layout at top of game screen
- Responsive to game state changes