# CLAUDE.md - Godot 插件和扩展

## 模块概述

此目录包含项目的 Godot 插件和扩展组件，主要集成了 GDAI MCP 插件以提供 AI 辅助开发功能。

## 目录结构

### GDAI MCP 插件 (`gdai-mcp-plugin-godot/`)
GDAI MCP Plugin v0.2.6 - AI 辅助开发插件的核心组件

#### 核心文件
- **[gdai_mcp_plugin.gd](gdai-mcp-plugin-godot/gdai_mcp_plugin.gd)** - 插件主入口文件
- **[gdai_mcp_runtime.gd](gdai-mcp-plugin-godot/gdai_mcp_runtime.gd)** - 运行时脚本，自动加载为全局单例
- **[plugin.cfg](gdai-mcp-plugin-godot/plugin.cfg)** - 插件配置文件

#### 文档文件
- **[README.md](gdai-mcp-plugin-godot/README.md)** - 插件使用说明
- **[LICENSE.md](gdai-mcp-plugin-godot/LICENSE.md)** - 许可证文件
- **[CHANGELOG.md](gdai-mcp-plugin-godot/CHANGELOG.md)** - 版本更新日志

## GDAI MCP 插件功能

### 核心能力
- **场景操作** - 创建、修改、删除场景和节点
- **脚本编写** - 自动生成和编辑 GDScript/C# 脚本
- **资源管理** - 管理项目资源和导入设置
- **调试支持** - 查看错误日志、调试输出
- **文件搜索** - 搜索项目文件和代码内容

### AI 集成特性
- **自然语言接口** - 通过中文描述操作项目
- **上下文感知** - 理解项目结构和代码关系
- **智能建议** - 提供最佳实践和优化建议
- **错误诊断** - 自动识别和修复常见问题

## 配置和集成

### 项目配置
在 `project.godot` 中已配置：
```ini
[autoload]
GDAIMCPRuntime="*res://addons/gdai-mcp-plugin-godot/gdai_mcp_runtime.gd"

[editor_plugins]
enabled=PackedStringArray("res://addons/gdai-mcp-plugin-godot/plugin.cfg")
```

### 使用方式
1. **编辑器集成** - 插件自动在 Godot 编辑器中启用
2. **全局访问** - 通过 `GDAIMCPRuntime` 单例全局访问插件功能
3. **MCP 连接** - 与外部 MCP 客户端建立通信连接

## 开发指南

### 扩展插件
- 可以基于现有插件扩展自定义功能
- 遵循 Godot 插件开发规范
- 保持与 MCP 协议的兼容性

### 最佳实践
- 充分利用插件的 AI 辅助能力提高开发效率
- 结合 Spec Workflow 系统进行结构化开发
- 保持插件版本的及时更新

## 注意事项

- 插件目录通常不应提交到公共代码库
- 确保 MCP 客户端与插件版本的兼容性
- 开发时保持插件与 Godot 编辑器的连接状态