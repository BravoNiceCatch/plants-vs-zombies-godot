# CLAUDE.md - Spec Workflow 系统

## 模块概述

Spec Workflow 系统提供了一套完整的项目规划和开发工作流，包含需求分析、产品设计、技术规范、项目结构规划等模板。

## 目录结构

### 模板系统 (`templates/`)
- **[requirements-template.md](templates/requirements-template.md)** - 需求文档模板
- **[product-template.md](templates/product-template.md)** - 产品设计文档模板
- **[design-template.md](templates/design-template.md)** - 设计文档模板
- **[tech-template.md](templates/tech-template.md)** - 技术规范文档模板
- **[structure-template.md](templates/structure-template.md)** - 项目结构规划模板
- **[tasks-template.md](templates/tasks-template.md)** - 任务规划模板

### 用户模板 (`user-templates/`)
- **[README.md](user-templates/README.md)** - 用户模板说明文档

## 使用指南

### 1. 需求分析阶段
使用 `requirements-template.md` 创建项目需求文档，明确：
- 项目目标和范围
- 功能需求列表
- 非功能性需求
- 用户故事和用例

### 2. 产品设计阶段
使用 `product-template.md` 创建产品设计文档，包含：
- 产品定位和价值主张
- 目标用户分析
- 功能架构设计
- 用户体验规划

### 3. 技术设计阶段
使用 `design-template.md` 和 `tech-template.md`：
- 系统架构设计
- 技术栈选择和说明
- 数据模型设计
- 接口和API设计

### 4. 项目结构规划
使用 `structure-template.md` 规划：
- 代码组织结构
- 文件和目录命名规范
- 模块划分策略
- 依赖关系设计

### 5. 任务分解和执行
使用 `tasks-template.md` 将项目分解为：
- 开发里程碑
- 具体任务列表
- 时间和资源规划
- 风险评估

## 工作流程

1. **启动阶段** - 从需求分析开始
2. **规划阶段** - 完成产品和技术设计
3. **实施阶段** - 按照任务规划执行
4. **迭代阶段** - 根据反馈调整和优化

## 最佳实践

- 按顺序使用各个模板，确保逻辑连贯性
- 在每个阶段都要有明确的产出物
- 定期回顾和更新文档内容
- 保持文档与实际开发同步

## 集成说明

此 Spec Workflow 系统与项目的 GDAI MCP 插件和 Claude Code 紧密集成，支持：
- AI 辅助的文档生成
- 自动化的任务分解
- 智能化的进度跟踪
- 协作式的项目管理