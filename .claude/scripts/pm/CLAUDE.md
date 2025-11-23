# CLAUDE.md - 项目管理脚本工具集

## 模块概述

此目录包含项目管理的 Shell 脚本工具集，用于项目管理、任务跟踪、Epic 管理和 PRD 管理等。

## 脚本工具分类

### 初始化和配置
- **[init.sh](init.sh)** - 项目初始化脚本，用于设置项目环境

### Epic 管理
- **[epic-list.sh](epic-list.sh)** - 列出所有 Epic 任务
- **[epic-show.sh](epic-show.sh)** - 显示特定 Epic 的详细信息
- **[epic-status.sh](epic-status.sh)** - 查看 Epic 状态

### PRD 管理
- **[prd-list.sh](prd-list.sh)** - 列出所有 PRD（产品需求文档）
- **[prd-status.sh](prd-status.sh)** - 查看 PRD 状态

### 任务管理
- **[in-progress.sh](in-progress.sh)** - 查看当前进行中的任务
- **[next.sh](next.sh)** - 获取下一个待办任务
- **[blocked.sh](blocked.sh)** - 查看被阻塞的任务

### 项目状态和报告
- **[status.sh](status.sh)** - 显示项目整体状态
- **[standup.sh](standup.sh)** - 生成每日站会报告
- **[search.sh](search.sh)** - 搜索项目和任务信息

### 工具和辅助
- **[help.sh](help.sh)** - 显示帮助信息和使用指南
- **[validate.sh](validate.sh)** - 验证项目配置和数据完整性

## 使用指南

### 基本用法
所有脚本都支持直接执行：
```bash
# 查看项目状态
./status.sh

# 列出所有 Epic
./epic-list.sh

# 获取下一个任务
./next.sh
```

### 常见工作流

1. **每日开始工作**
   ```bash
   ./standup.sh    # 查看今日摘要
   ./next.sh       # 获取下一个任务
   ```

2. **查看项目进展**
   ```bash
   ./status.sh         # 整体状态
   ./in-progress.sh    # 进行中任务
   ./blocked.sh        # 被阻塞任务
   ```

3. **Epic 级别管理**
   ```bash
   ./epic-list.sh      # 所有 Epic
   ./epic-show.sh [id] # 特定 Epic 详情
   ./epic-status.sh    # Epic 状态概览
   ```

### 配置和定制

#### 环境变量
脚本使用以下环境变量：
- `PROJECT_ROOT` - 项目根目录路径
- `PM_CONFIG` - 项目管理配置文件路径

#### 配置文件
项目配置存储在 `.claude/settings.local.json` 中

## 与其他模块的集成

### 与 Spec Workflow 集成
- 支持 PRD 和 Epic 的关联管理
- 自动生成项目状态报告

### 与 Claude Code 集成
- 提供命令行的项目管理接口
- 支持与 AI 代理的交互

## 最佳实践

1. **日常使用**
   - 每日开始时运行 `standup.sh`
   - 任务完成后运行 `next.sh` 获取新任务
   - 定期检查 `blocked.sh` 解决阻塞问题

2. **Epic 管理**
   - 使用 `epic-list.sh` 了解整体规划
   - 通过 `epic-status.sh` 跟踪进度

3. **数据维护**
   - 定期运行 `validate.sh` 验证数据完整性
   - 及时更新任务状态

## 故障排除

### 常见问题
- **权限问题**：确保脚本有执行权限 (`chmod +x *.sh`)
- **路径问题**：确保在正确的项目目录下执行
- **配置问题**：检查 `.claude/settings.local.json` 配置

### 调试技巧
- 使用 `bash -x` 调试脚本执行过程
- 检查脚本输出的错误信息
- 验证环境变量和路径配置
