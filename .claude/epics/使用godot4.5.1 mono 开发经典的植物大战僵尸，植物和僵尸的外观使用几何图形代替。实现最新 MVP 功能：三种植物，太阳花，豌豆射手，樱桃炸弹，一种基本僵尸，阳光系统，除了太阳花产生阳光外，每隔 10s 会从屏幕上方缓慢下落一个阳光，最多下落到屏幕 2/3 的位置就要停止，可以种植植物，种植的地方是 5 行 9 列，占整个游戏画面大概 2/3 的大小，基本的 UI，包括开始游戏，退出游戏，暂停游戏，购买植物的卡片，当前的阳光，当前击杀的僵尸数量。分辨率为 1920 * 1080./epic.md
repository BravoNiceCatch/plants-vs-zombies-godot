---
name: 使用godot4.5.1 mono 开发经典的植物大战僵尸，植物和僵尸的外观使用几何图形代替。实现最新 MVP 功能：三种植物，太阳花，豌豆射手，樱桃炸弹，一种基本僵尸，阳光系统，除了太阳花产生阳光外，每隔 10s 会从屏幕上方缓慢下落一个阳光，最多下落到屏幕 2/3 的位置就要停止，可以种植植物，种植的地方是 5 行 9 列，占整个游戏画面大概 2/3 的大小，基本的 UI，包括开始游戏，退出游戏，暂停游戏，购买植物的卡片，当前的阳光，当前击杀的僵尸数量。分辨率为 1920 * 1080.
status: backlog
created: 2025-11-23T00:43:34Z
progress: 30%
prd: .claude/prds/使用godot4.5.1 mono 开发经典的植物大战僵尸，植物和僵尸的外观使用几何图形代替。实现最新 MVP 功能：三种植物，太阳花，豌豆射手，樱桃炸弹，一种基本僵尸，阳光系统，除了太阳花产生阳光外，每隔 10s 会从屏幕上方缓慢下落一个阳光，最多下落到屏幕 2/3 的位置就要停止，可以种植植物，种植的地方是 5 行 9 列，占整个游戏画面大概 2/3 的大小，基本的 UI，包括开始游戏，退出游戏，暂停游戏，购买植物的卡片，当前的阳光，当前击杀的僵尸数量。分辨率为 1920 * 1080..md
github: https://github.com/BravoNiceCatch/plants-vs-zombies-godot/issues/1
---

# Epic: 使用 Godot 4.5.1 Mono 开发经典植物大战僵尸 MVP 版本

## Overview

基于 Godot 4.5.1 Mono 引擎开发经典植物大战僵尸塔防游戏的 MVP 版本。采用面向对象架构设计，使用 C# 脚本实现游戏逻辑，通过简单几何图形代替复杂美术资源，专注于核心游戏机制的实现和验证。

## Architecture Decisions

### 核心架构选择
- **场景树架构**: 使用 Godot 场景树组织游戏对象，每个游戏实体作为独立节点
- **组件化设计**: 植物和僵尸使用组合模式，行为组件可插拔
- **状态机模式**: 游戏状态管理使用状态机，确保状态转换清晰可控
- **观察者模式**: 事件系统使用 Godot 内置信号机制，实现松耦合

### 技术栈决策
- **开发语言**: C# (Mono) - 强类型支持，更好的代码组织和维护
- **渲染方式**: 2D 渲染，使用 CanvasItem 和基本几何图形
- **物理系统**: Godot 内置 2D 物理引擎，用于碰撞检测
- **UI 系统**: Godot Control 节点，实现游戏界面和交互

### 设计模式应用
- **单例模式**: 游戏管理器使用全局单例，统一管理游戏状态
- **工厂模式**: 植物和僵尸创建使用工厂模式，便于扩展新类型
- **策略模式**: 不同植物行为使用策略模式，统一接口多样实现
- **命令模式**: 用户操作使用命令模式，支持撤销和重做

## Technical Approach

### 核心系统组件

#### 1. 游戏管理器 (GameManager)
```csharp
// 全局游戏状态管理
class GameManager : Node
{
    // 游戏状态枚举
    public enum GameState { Menu, Playing, Paused, GameOver, Victory }

    // 核心属性
    public int CurrentSunlight { get; set; }
    public int KillCount { get; set; }
    public GameState CurrentState { get; set; }

    // 主要方法
    public void StartGame();
    public void PauseGame();
    public void ResumeGame();
    public void EndGame(bool victory);
}
```

#### 2. 网格系统 (GridSystem)
```csharp
// 种植网格管理
class GridSystem : Node2D
{
    // 网格配置 (5行 x 9列)
    public const int ROWS = 5;
    public const int COLS = 9;
    public const int CELL_SIZE = 80; // 每格80像素

    // 核心方法
    public Vector2 GridToWorld(int row, int col);
    public Vector2Int WorldToGrid(Vector2 worldPos);
    public bool CanPlantAt(int row, int col);
    public Plant GetPlantAt(int row, int col);
}
```

#### 3. 植物系统架构
```csharp
// 植物基类
abstract class Plant : Node2D
{
    public int Cost { get; protected set; }
    public int Health { get; set; }
    public GridPosition Position { get; set; }

    public abstract void OnPlanted();
    public abstract void Update(float deltaTime);
    public abstract void TakeDamage(int damage);
}

// 具体植物实现
class Sunflower : Plant { /* 太阳光产生逻辑 */ }
class Peashooter : Plant { /* 豌豆发射逻辑 */ }
class CherryBomb : Plant { /* 爆炸攻击逻辑 */ }
```

#### 4. 僵尸系统架构
```csharp
// 僵尸基类
class Zombie : Node2D
{
    public int Health { get; set; }
    public float MoveSpeed { get; protected set; }
    public int Damage { get; protected set; }

    public abstract void Update(float deltaTime);
    public virtual void TakeDamage(int damage);
}

// 基础僵尸实现
class BasicZombie : Zombie
{
    public BasicZombie() {
        Health = 100;
        MoveSpeed = 26.67f; // 1格/3秒 (80像素/3秒)
        Damage = 10;
    }
}
```

#### 5. 阳光系统
```csharp
// 阳光管理器
class SunlightManager : Node
{
    // 阳光生成
    public void GenerateFromSky(); // 天降阳光
    public void OnSunflowerProduced(Sunflower sunflower); // 太阳花产生

    // 阳光收集
    public void OnSunlightClicked(Sunlight sunlight);
    public void AddSunlight(int amount);

    // 定时器配置
    private Timer skyDropTimer; // 10秒间隔
    private Timer sunflowerProductionTimer; // 5秒间隔
}
```

#### 6. 战斗系统
```csharp
// 战斗管理器
class CombatManager : Node
{
    // 伤害计算
    public void RegisterProjectile(Projectile projectile);
    public void CheckCollisions();

    // 爆炸处理
    public void TriggerExplosion(Vector2 position, float radius, int damage);

    // 战斗状态
    public bool CheckZombieVictory();
    public bool CheckPlayerVictory();
}
```

### 前端 UI 组件

#### 1. 主菜单界面 (MainMenu)
- 开始游戏按钮
- 退出游戏按钮
- 游戏标题显示

#### 2. 游戏内 HUD (GameHUD)
- 植物选择卡片 (3种植物)
- 当前阳光显示
- 击杀计数显示
- 暂停按钮

#### 3. 游戏状态界面
- 暂停界面
- 胜利界面
- 失败界面

### 资源管理

#### 图形资源
- **几何图形**: 使用 Godot 内置绘图 API 创建圆形、方形等基本形状
- **动画**: 通过代码控制位置、大小、颜色变化实现简单动画效果
- **粒子效果**: 使用 Godot 粒子系统实现爆炸、阳光收集等特效

#### 音频资源 (可选)
- 基础音效接口预留，MVP 版本可暂时跳过

## Implementation Strategy

### 开发阶段规划

#### 阶段1: 核心架构搭建 (1周)
- [x] 项目初始化和基础场景结构
- [x] 游戏管理器基础框架
- [x] 网格系统实现
- [x] 基础 UI 框架搭建

#### 阶段2: 植物系统实现 (1.5周)
- [x] 植物基类和继承体系
- [x] 太阳光花实现 (5秒生产周期)
- [x] 豌豆射手实现 (每秒发射)
- [x] 樱桃炸弹实现 (2秒延时爆炸)

#### 阶段3: 僵尸和战斗系统 (1周)
- [x] 基础僵尸实现
- [x] 豌豆子弹和碰撞检测
- [x] 爆炸范围伤害系统
- [x] 战斗状态管理

#### 阶段4: 阳光经济系统 (0.5周)
- [x] 阳光生成和收集逻辑
- [x] 植物购买和种植验证
- [x] 经济平衡调整

#### 阶段5: 游戏流程完善 (1周)
- [x] 完整游戏流程 (开始→游戏→结束)
- [x] 胜利/失败条件实现
- [x] 游戏暂停和恢复功能
- [x] 基础性能优化

### 风险缓解策略

#### 技术风险
- **性能瓶颈**: 提前进行性能测试，优化对象池和碰撞检测
- **内存泄漏**: 建立内存使用监控，定期检查节点释放
- **状态同步**: 使用 Godot 内置信号系统确保状态一致性

#### 进度风险
- **功能裁剪准备**: 定义功能优先级，必要时可快速裁剪
- **模块化开发**: 各系统独立开发，降低相互依赖
- **增量验证**: 每个阶段结束进行可玩性验证

### 测试策略

#### 单元测试
- 植物行为逻辑测试
- 僵尸AI行为测试
- 经济系统平衡性测试

#### 集成测试
- 完整游戏流程测试
- 性能压力测试 (50+对象)
- 多场景切换测试

#### 用户测试
- 新手引导和可玩性测试
- 游戏平衡性验证
- Bug 收集和修复

## Task Breakdown Preview

- [ ] **项目基础搭建**: Godot项目初始化、基础场景结构、C#脚本配置
- [ ] **核心系统开发**: 游戏管理器、网格系统、状态机实现
- [ ] **植物系统实现**: 三种植物的完整功能开发和测试
- [ ] **僵尸和战斗**: 僵尸AI、战斗系统、伤害计算实现
- [ ] **UI界面开发**: 主菜单、游戏HUD、状态界面实现
- [ ] **阳光经济系统**: 阳光生成、收集、购买逻辑实现
- [ ] **游戏流程完善**: 完整游戏循环、胜负条件、暂停功能
- [ ] **性能优化和测试**: 性能调优、Bug修复、最终测试

## Dependencies

### 外部依赖
- **Godot 4.5.1 Mono**: 核心开发引擎
- **.NET 8.0**: C# 运行时环境
- **开发设备**: 支持目标分辨率的显示设备

### 内部依赖
- **开发团队**: 1名主程序 + 1名测试/辅助开发
- **项目管理**: Claude Code + GDAI MCP 工具链
- **版本控制**: Git 仓库和分支管理

### 技术依赖关系
```
游戏管理器 ← 网格系统 ← 植物系统
    ↓         ↓         ↓
  UI界面 ← 阳光系统 ← 战斗系统
    ↓         ↓         ↓
  用户交互 ← 经济系统 ← 僵尸AI
```

## Tasks Created
- [ ] 001.md - 项目基础搭建 (parallel: false)
- [ ] 002.md - 核心系统开发 (parallel: true)
- [ ] 003.md - UI界面开发 (parallel: true)
- [ ] 004.md - 植物系统实现 (parallel: true)
- [ ] 005.md - 僵尸和战斗系统 (parallel: true)
- [ ] 006.md - 阳光经济系统 (parallel: true)
- [ ] 007.md - 游戏流程完善 (parallel: false)
- [ ] 008.md - 性能优化和测试 (parallel: false)

Total tasks: 8
Parallel tasks: 5 (任务002-006可以并行执行)
Sequential tasks: 3 (任务001 → 任务007 → 任务008)
Estimated total effort: 176小时

## Success Criteria (Technical)

### 性能指标
- **帧率**: 稳定60 FPS，50+对象时不低于30 FPS
- **响应时间**: 用户操作响应延迟 < 100ms
- **内存使用**: 总内存占用 < 512MB
- **启动时间**: 游戏启动时间 < 5秒

### 质量标准
- **代码覆盖率**: 核心逻辑代码测试覆盖率 > 80%
- **Bug密度**: 发布版本严重Bug数量 = 0
- **代码质量**: 遵循C#编码规范，代码可维护性良好

### 功能完整性
- [ ] 所有PRD功能需求100%实现
- [ ] 游戏可从头到尾完整体验
- [ ] 胜负条件正确触发
- [ ] UI交互流畅自然

## Estimated Effort

### 总体时间估算
- **总开发周期**: 7周
- **核心开发**: 5周
- **测试优化**: 1.5周
- **发布准备**: 0.5周

### 人力分配
- **主程序**: 全程5周开发 + 1.5周测试 = 6.5周
- **辅助开发**: 3周开发 + 1周测试 = 4周
- **测试验证**: 2周并行进行

### 关键路径
1. **基础架构** → **植物系统** → **战斗系统** → **完整游戏循环**
2. **UI开发** 与 **游戏逻辑** 并行进行
3. **性能优化** 在功能基本完成后进行

### 里程碑检查点
- **Week 1**: 基础架构完成，可显示空游戏场景
- **Week 2.5**: 植物系统完成，可种植和观察植物行为
- **Week 3.5**: 战斗系统完成，僵尸出现和攻击功能正常
- **Week 5**: 完整游戏流程实现，基础可玩版本
- **Week 6.5**: 性能优化和Bug修复完成
- **Week 7**: 最终测试和发布准备完成