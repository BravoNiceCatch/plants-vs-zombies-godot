---
issue: 6
title: 僵尸和战斗系统
analyzed: 2025-11-23T03:14:04Z
estimated_hours: 28
parallelization_factor: 3.2
---

# Parallel Work Analysis: Issue #6

## Overview
实现基础僵尸AI系统、战斗机制和伤害计算，包括僵尸的移动、攻击、死亡逻辑，以及豌豆子弹的飞行和碰撞检测系统。这是一个复杂的游戏系统，需要多个独立的子系统协同工作。

## Parallel Streams

### Stream A: 僵尸基础系统
**Scope**: 实现僵尸基类、基础僵尸类型和僵尸管理器
**Files**:
- `Scripts/Zombies/Zombie.cs` - 僵尸基类
- `Scripts/Zombies/BasicZombie.cs` - 基础僵尸实现
- `Scripts/Zombies/ZombieManager.cs` - 僵尸生成和波次管理
- `Scenes/Zombies/BasicZombie.tscn` - 基础僵尸场景
**Agent Type**: backend-specialist (游戏逻辑)
**Can Start**: immediately
**Estimated Hours**: 12
**Dependencies**: none

### Stream B: 战斗和投射物系统
**Scope**: 实现豌豆子弹、碰撞检测和基础战斗机制
**Files**:
- `Scripts/Combat/Pea.cs` - 豌豆子弹类
- `Scripts/Combat/Projectile.cs` - 投射物基类
- `Scripts/Combat/CollisionManager.cs` - 碰撞检测管理
- `Scenes/Combat/Pea.tscn` - 豌豆子弹场景
**Agent Type**: backend-specialist (物理系统)
**Can Start**: immediately
**Estimated Hours**: 8
**Dependencies**: none

### Stream C: 战斗管理器和爆炸系统
**Scope**: 实现战斗管理器、爆炸效果和AOE伤害系统
**Files**:
- `Scripts/Combat/CombatManager.cs` - 战斗管理器
- `Scripts/Combat/Explosion.cs` - 爆炸效果类
- `Scripts/Combat/CherryBomb.cs` - 樱桃炸弹爆炸逻辑
- `Scripts/Effects/ExplosionEffect.cs` - 爆炸视觉效果
**Agent Type**: backend-specialist (游戏系统)
**Can Start**: immediately
**Estimated Hours**: 8
**Dependencies**: none

## Coordination Points

### Shared Files
需要协调修改的共享文件：
- `Scripts/GameManager.cs` - Streams A & B & C (需要添加僵尸计数和战斗统计)
- `Scripts/Core/GridSystem.cs` - Stream A (僵尸需要使用网格系统)
- `Scripts/Plants/Plant.cs` - Stream A (僵尸需要检测植物)
- 项目配置文件 - 无额外依赖需求

### Sequential Requirements
必须按顺序完成的部分：
1. 僵尸基础系统完成后，可以测试僵尸移动
2. 投射物系统完成后，可以测试与僵尸的交互
3. 爆炸系统可以独立开发，但需要僵尸对象进行测试

## Conflict Risk Assessment
- **Low Risk**: 各流处理不同的目录和类，冲突概率低
- **Medium Risk**: GameManager 可能需要多流协调修改
- **High Risk**: 共享接口和事件系统需要定义清晰的契约

## Parallelization Strategy

**Recommended Approach**: parallel

所有三个流可以立即开始并行开发，因为它们处理相对独立的功能模块。关键协调点：
- Stream A 定义僵尸接口和事件
- Stream B 实现投射物与僵尸的交互
- Stream C 实现高级战斗功能

启动所有流：A, B, C 同时开始。

## Expected Timeline

With parallel execution:
- Wall time: 12 hours (最长流的时长)
- Total work: 28 hours
- Efficiency gain: 57% (从28小时减少到12小时)

Without parallel execution:
- Wall time: 28 hours

## Notes
### 关键协调要点：
1. **僵尸健康系统**: Stream A 定义健康属性，Stream B & C 使用这些属性
2. **碰撞检测**: 需要统一的碰撞层和掩码设置
3. **性能考虑**: 50+战斗对象同时运行时需要优化碰撞检测
4. **视觉效果**: 所有几何图形渲染需要保持一致的风格

### 技术债务预防：
- 提前定义清晰的接口 (ICombatant, IDamageable)
- 使用对象池管理投射物和爆炸效果
- 实现高效的碰撞检测算法（空间分区）

### 测试策略：
- 每个流独立测试核心功能
- 集成测试验证流间交互
- 性能测试确保50+对象运行流畅