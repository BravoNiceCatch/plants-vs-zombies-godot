# CLAUDE.md - Claude Code 配置和代理

## 模块概述

此目录包含 Claude Code 的配置文件、代理定义和钩子脚本，用于定制 AI 代理的行为和项目交互方式。

## 目录结构

### 代理系统 (`agents/`)
包含各种专业化的 AI 代理定义：

- **[parallel-worker.md](agents/parallel-worker.md)** - 并行工作代理，支持多任务并行执行
- **[test-runner.md](agents/test-runner.md)** - 测试运行代理，专门处理测试执行和结果分析
- **[file-analyzer.md](agents/file-analyzer.md)** - 文件分析代理，用于深度分析文件内容
- **[code-analyzer.md](agents/code-analyzer.md)** - 代码分析代理，专注于代码质量检查和问题诊断
- **[CLAUDE.md](agents/CLAUDE.md)** - 代理系统的主配置文档

### 钩子系统 (`hooks/`)
- **[README.md](hooks/README.md)** - 钩子系统说明文档

### 上下文系统 (`context/`)
- **[README.md](context/README.md)** - 上下文系统说明文档

### 配置文件
- **[settings.local.json](settings.local.json)** - 本地设置配置文件

## 核心功能

### 1. 专业化代理
每个代理都有特定的专长和使用场景：

**parallel-worker**
- 支持多任务并行处理
- 自动任务分配和协调
- 提高复杂任务的执行效率

**test-runner**
- 智能测试选择和执行
- 测试结果分析和报告
- 测试覆盖率统计

**file-analyzer**
- 深度文件内容分析
- 结构化信息提取
- 跨文件关联分析

**code-analyzer**
- 代码质量评估
- 潜在问题识别
- 最佳实践建议

### 2. 钩子系统
提供事件驱动的自动化处理：
- 工具调用前后钩子
- 任务执行状态钩子
- 用户交互钩子

### 3. 上下文管理
智能上下文保持和传递：
- 项目上下文缓存
- 会话状态管理
- 跨代理信息共享

## 配置说明

### settings.local.json
包含项目特定的 Claude Code 设置：
- 代理行为参数
- 钩子触发条件
- 上下文保留策略

## 使用指南

### 选择合适的代理
根据任务类型选择相应的代理：
- 复杂多步骤任务 → parallel-worker
- 测试相关工作 → test-runner
- 文档分析 → file-analyzer
- 代码审查 → code-analyzer

### 自定义代理
可以创建新的代理定义：
1. 在 `agents/` 目录创建新的 `.md` 文件
2. 定义代理的行为和能力
3. 配置相应的工具和权限

### 钩子定制
- 在 `hooks/` 目录添加自定义钩子脚本
- 配置触发条件和执行逻辑
- 与现有工作流集成

## 与其他模块的集成

### 与 Spec Workflow 集成
- 支持规范化的文档处理
- 自动任务分解和规划

### 与 GDAI MCP 插件集成
- 提供 Godot 特定的工具调用
- 场景和脚本操作的智能辅助

## 最佳实践

- 根据任务复杂度选择合适的代理
- 定期更新代理定义以提高性能
- 保持配置文件的一致性和可维护性
- 利用钩子自动化重复性任务

## 故障排除

常见问题和解决方案：
- 代理响应异常 → 检查代理定义文件
- 钩子未触发 → 验证钩子配置
- 上下文丢失 → 检查上下文设置