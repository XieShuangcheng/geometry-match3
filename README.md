# 几何消除

基于 Unity 和 C# 开发的 2D 三消战斗原型。玩家交换几何方块，通过消除对敌人造成伤害，并利用敌人的颜色、形状弱点获得伤害加成。

## 开发环境

| 项目 | 版本 / 说明 |
| --- | --- |
| Unity Editor | **2022.3.57f1c2**（以 `ProjectSettings/ProjectVersion.txt` 为准） |
| 开发语言 | C# |
| 渲染管线 | Universal Render Pipeline 14.0.11，2D Renderer |
| 文本组件 | TextMesh Pro 3.0.7 |
| 主场景 | `Assets/Scenes/SampleScene.unity` |

## 打开与运行

1. 克隆或下载仓库，保留 `Assets`、`Packages`、`ProjectSettings` 三个目录。
2. 在 Unity Hub 中添加项目，选择包含这三个目录的项目根目录。
3. 使用上述 Unity 版本打开项目，等待 Package Manager 还原依赖和资源导入完成。首次打开需要访问配置的 Unity 包源。
4. 打开 `Assets/Scenes/SampleScene.unity`，点击 Play，在 Game 视图中操作。

`Library`、`UserSettings` 和 IDE 工程文件由本机重新生成，无需从仓库恢复。构建时可在 Build Settings 中确认主场景已勾选，并将输出保存到 `Build` 或 `Builds` 目录。

## 操作与规则

- **鼠标左键**：选中方块；点击空白处取消选择。
- **鼠标右键**：在选中方块的上、下、左、右方向点击，尝试与该方向相邻方块交换。
- 横向或纵向连续至少三个方块，且**颜色和形状均相同**时消除。
- 交换未产生匹配时自动换回；消除后方块下落并补充，支持连锁消除。
- 没有可用交换时自动重新生成棋盘。
- 消除对敌人造成伤害，命中颜色弱点或形状弱点各获得 1.5 倍倍率，两者同时命中时倍率相乘。
- 击败敌人后进入下一关；超过已配置的关卡数量时沿用最后一关的数据。

## 已实现内容

- 六种颜色、三种形状的方块与对应精灵映射。
- 随机棋盘、相邻交换、匹配检测、下落补充和无可用移动检测。
- 方块对象池与移动动画。
- 敌人生命值、弱点、生命阶段表情与关卡切换。
- 关卡文字、格状血条与累计伤害显示。

## 项目结构

| 路径 | 内容 |
| --- | --- |
| `Assets/Scenes/` | 主场景 |
| `Assets/Scripts/Grid/` | 方块数据、棋盘管理、鼠标输入与交换处理 |
| `Assets/Scripts/Game/` | 游戏流程、敌人、关卡数据 |
| `Assets/Scripts/UI/` | 血条、关卡与伤害显示 |
| `Assets/Resources/Prefabs/` | 方块、敌人、棋盘和 UI 等预制体 |
| `Assets/Sprites/` | 方块、敌人和 UI 图片 |
| `Assets/Settings/` | URP 与 2D 渲染设置 |
| `Assets/TextMesh Pro/` | TextMesh Pro 资源及附带的第三方声明 |
| `Packages/` | 包依赖清单与锁定文件 |
| `ProjectSettings/` | Unity 项目设置与编辑器版本 |

建议先阅读 `InputManager.cs` 和 `GridManager.cs` 理解交换与消除，再阅读 `GameManager.cs`、`Enemy.cs` 和 `LevelManager.cs` 理解伤害与关卡流程。实际参数以场景和预制体中的 Inspector 配置为准。

## 当前状态

项目为开发中的玩法原型。仓库提供 Unity 源工程，运行效果应在对应版本编辑器中验证。当前输入实现为鼠标操作，尚未实现触屏操作；本次发布准备未进行 Unity 编辑器运行或构建验证。

## 提交与隐私

- 提交 `Assets/` 及其 `.meta` 文件、`Packages/manifest.json`、`Packages/packages-lock.json` 和 `ProjectSettings/`，避免资源引用或依赖丢失。
- `.gitignore` 已排除 Unity 缓存、编辑器个人设置、IDE 生成文件、日志、构建目录和常见凭据文件。
- 提交前检查 `git status --short` 和 `git diff --cached`，避免把密钥、账号信息、本机绝对路径或个人文件写进源码与资源。
- `.gitignore` 只控制 Git 跟踪；直接压缩整个本地项目仍会包含缓存和个人配置。分享源码压缩包时应只打包上述工程目录及仓库说明文件。

## 第三方资源与许可

TextMesh Pro 附带的 Liberation Sans 字体许可和 EmojiOne 署名文件保留在原资源目录中。Unity 包与其他第三方资源遵循各自的许可条款。

本仓库尚未指定项目代码的开源许可证；公开可见不代表已授予任意使用或再分发的许可。
