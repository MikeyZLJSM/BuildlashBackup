# Excel模块配置系统使用指南

## 系统概述

该系统使用ExcelDataReader插件读取Excel配置表，自动为不同模块设置数值参数，包括生命值、攻击力、攻击速度、攻击范围等。

## 文件结构

```
Assets/Scripts/Module/Config/
├── ModuleConfig.cs              # 模块配置数据类
├── ExcelConfigReader.cs         # Excel文件读取器
├── ModuleConfigManager.cs       # 配置管理器（单例）
├── RuntimeConfigApplier.cs      # 运行时配置应用器
├── ExcelConfigTemplateGenerator.cs # 配置表模板生成器
└── Editor/
    └── ModuleConfigDebugWindow.cs # 编辑器调试工具
```

## Excel配置表格式

### 文件位置
- 路径：`Assets/Config/数值表.xlsx`
- 工作表名：`ModuleConfig`

### 表格结构
| ModuleType | Health | AttackCD | Damage | AttackSpeed | AttackRange | BulletSpeed | TargetLockType | DamageType | AttackAttribute | BulletCount | TargetCount | SplashRadius |
|------------|--------|----------|--------|-------------|-------------|-------------|----------------|------------|-----------------|-------------|-------------|--------------|
| BaseCube | 80 | 1.0 | 8 | 1.0 | 5.0 | 10.0 | Nearest | Physical | None | 1 | 1 | 0 |
| NormalCube | 100 | 1.0 | 10 | 1.0 | 5.0 | 10.0 | Nearest | Physical | None | 1 | 1 | 0 |
| NormalCylinder | 120 | 0.8 | 12 | 1.2 | 6.0 | 12.0 | Nearest | Physical | None | 1 | 1 | 0 |
| NormalSphere | 90 | 0.6 | 15 | 1.5 | 4.5 | 15.0 | Nearest | Physical | None | 3 | 1 | 0 |
| NormalCone | 110 | 1.2 | 14 | 0.8 | 5.5 | 8.0 | Nearest | Physical | Splash | 1 | 3 | 2.5 |

### 参数说明
- **ModuleType**: 模块类型（BaseCube, NormalCube, NormalCylinder, NormalSphere, NormalCone）
- **Health**: 生命值
- **AttackCD**: 攻击冷却时间
- **Damage**: 攻击伤害
- **AttackSpeed**: 攻击速度
- **AttackRange**: 攻击范围
- **BulletSpeed**: 子弹速度
- **TargetLockType**: 目标锁定类型（Nearest, Farthest, Random）
- **DamageType**: 伤害类型（Physical, Magical）
- **AttackAttribute**: 攻击属性（None, Splash, Piercing）
- **BulletCount**: 子弹数量
- **TargetCount**: 目标数量
- **SplashRadius**: 溅射半径（仅当AttackAttribute为Splash时有效）

## 使用方法

### 1. 自动加载（推荐）
模块在Awake时会自动从Excel配置表加载数值，无需手动操作。

### 2. 运行时动态更新
在场景中添加`RuntimeConfigApplier`组件：
- **F5键**: 重新加载Excel配置
- **F6键**: 应用配置到所有现有模块

### 3. 编辑器调试
通过菜单 `Tools > 模块配置调试器` 打开调试窗口：
- 查看所有模块的当前配置
- 重新加载Excel配置
- 实时查看配置变更

### 4. 代码调用
```csharp
// 获取特定模块配置
var config = ModuleConfigManager.Instance.GetModuleConfig(ModuleType.NormalCube);

// 应用配置到模块
ModuleConfigManager.Instance.ApplyConfigToModule(module);

// 重新加载配置
ModuleConfigManager.Instance.ReloadConfigs();
```

## 系统特性

### 1. 容错机制
- Excel文件不存在时使用默认配置
- 单行数据解析失败时跳过该行，继续解析其他行
- 列名不匹配时使用默认值

### 2. 调试支持
- 详细的日志输出
- 编辑器调试窗口
- 运行时配置查看

### 3. 性能优化
- 单例模式避免重复加载
- 懒加载机制
- 内存中缓存配置数据

## 扩展配置

如需添加新的配置参数：

1. 在`ModuleConfig.cs`中添加新字段
2. 在Excel表格中添加对应列
3. 在`ExcelConfigReader.cs`的`ParseModuleConfig`方法中添加解析逻辑

## 故障排除

### 常见问题
1. **Excel文件读取失败**: 检查文件路径和权限
2. **配置不生效**: 确保Excel格式正确，列名匹配
3. **枚举值错误**: 检查Excel中的枚举值拼写

### 调试步骤
1. 查看Unity Console中的日志信息
2. 使用编辑器调试窗口检查配置加载状态
3. 确认Excel文件格式和数据正确性

## 注意事项

- Excel文件需要关闭后才能被程序读取
- 修改Excel后需要重新加载配置才能生效
- 建议在开发时备份Excel配置文件
