# 代码指南 (AI Agent 参考)

本文档旨在为 AI 开发者提供对 `Buildlash--` 项目代码结构的概述，以便于理解、导航和扩展代码库。

## 1. 项目概述

`Buildlash--` 是一款核心玩法围绕**模块化建造**和**战斗**的 Unity 游戏。玩家可以自由地将各种功能的模块拼接在一起，形成一个自定义的构造体，并用它来对抗敌人。

## 2. 核心命名空间和职责

代码主要组织在 `Assets/Scripts` 目录下，分为三个核心命名空间：`Controllers`、`Module` 和 `Enemy`。

### 2.1. `Controllers` - 控制器层

这是游戏的核心逻辑中枢，负责协调其他所有系统。

- **`BuildController.cs`**: **建造控制器**。
  - **职责**: 处理所有与模块拼接和拆除相关的逻辑。
  - **关键功能**:
    - 监听鼠标输入，通过射线检测识别玩家意图。
    - 管理拼接位置的**预览**功能 (`_previewObject`)，使用半透明材质显示拼接效果。
    - 支持**删除模式** (按住E键) 和**正常建造模式**的切换。
    - 调用 `BaseModule` 的 `AttachToFace` 和 `RemoveModule` 方法来执行实际的拼接与拆除。
    - 与 `ModuleSelector` 交互以管理当前选中的模块。
    - 实现了智能预览系统：当拖拽模块到可拼接位置时自动显示预览，离开时自动隐藏。

- **`ModulesManager.cs`**: **模块管理器**。
  - **职责**: 作为一个**数据库**，实时追踪和管理场景中所有已拼接的模块。
  - **关键功能**:
    - 维护一个 `List<ModuleInfo>`，其中 `ModuleInfo` 包含了对模块实例的引用及其在**网格坐标系**中的位置。
    - 管理**中心模块** (`centerModule`) 作为整个构造体的根节点。
    - 提供查询接口，如 `GetModuleByGridOffset`, `GetModulesOfType<T>` 等。
    - 在模块拼接 (`OnModuleAttached`) 和拆除 (`OnModuleDetached`) 时，更新模块列表。
    - 支持**网格设置** (`gridSize`) 用于精确的模块定位。
    - 配置**敌人图层掩码** (`enemyLayerMask`) 用于战斗系统的目标检测。
    - 是战斗系统计算飞船总属性（如总生命值）和索敌的重要依据。

- **`BattleManager.cs`**: **战斗管理器**。
  - **职责**: 统一管理战斗阶段的逻辑。
  - **关键功能**:
    - 管理玩家（即整个构造体）的**生命值** (`PlayerMaxHealth`, `PlayerCurrentHealth`)。
    - 通过 `CaculateMaxHealth()` 方法计算所有模块的总生命值。
    - 统一调用和更新所有**敌人 (`UpdateEnemyLogic`)** 和**我方战斗模块 (`UpdateModuleBattle`)** 的逻辑。
    - 初始化模块战斗系统 (`InitializeModuleBattleSystem`)。
    - 与 `UIController` 协作更新界面显示的生命值信息。
    - 包含部分类 `BattleManager.Enemy.cs` 和 `BattleManager.Module.cs`，分别处理敌人和我方模块的战斗细节。

- **`ModuleSelector.cs`**: **模块选择器**。
  - **职责**: 处理玩家对单个模块的**选中**和**取消选中**操作，为 `BuildController` 提供输入。

- **新增控制器**:
  - **`ModuleConfigManager.cs`**: **模块配置管理器**，负责模块配置数据的管理。
  - **`CameraScript.cs`**: **摄像机控制器**，处理游戏摄像机的行为。
  - **`SceneManager.cs`**: **场景管理器**，管理场景切换和状态。
  - **`UIController.cs`**: **UI控制器**，管理用户界面相关逻辑。

#### 2.1.1. `Controllers.Battle` - 战斗子系统

- **`BulletManager.cs`**: **子弹管理器**，统一管理场景中的所有子弹对象。
- **`Bullet.cs`**: **子弹类**，定义子弹的行为和属性。
- **`NormalAttribute.cs`**: **普通属性类**，定义战斗中的基础属性计算。

### 2.2. `Module` - 模块层

定义了所有玩家可建造模块的行为和属性。

- **`BaseModule.cs`**: **所有模块的基类** (抽象类)。
  - **职责**: 定义模块的通用行为，特别是**拼接逻辑**。
  - **关键实现 (`BaseModule.Attach.cs`)**:
    - `IAttachable` 接口实现。
    - `_attachableFaces` (`ModuleFace[]`): 定义了模块可以用于拼接的面。
    - `AttachToFace()`: 核心拼接方法。计算旋转和位移，将自身对齐并附加到目标模块上，同时建立父子关系。
    - `RemoveModule()`: 核心拆除方法。断开与父模块和子模块的连接。
    - `CanBeAttachedTarget_r()`: 检查模块是否可以作为拼接目标。
    - 支持**父子模块关系** (`parentModule`, `_childModules`)。
    - 包含**物理组件管理** (`Rigidbody`, `BoxCollider`)。
  - **战斗实现 (`BaseModule.Battle.cs`)**:
    - 包含模块在战斗中的属性和行为，如生命值、攻击逻辑等。

- **`ModuleFace.cs`**: **模块面类**，定义模块可拼接面的属性和行为。

- **`ModuleScript/`**: 存放具体的模块预制件脚本，按几何形状分类：
  - `Cone/`: 锥形模块
  - `Cube/`: 立方体模块  
  - `Cylinder/`: 圆柱形模块
  - `Sphere/`: 球形模块
  这些脚本继承自 `BaseModule` 并定义各自的特殊行为。

#### 2.2.1. `Module.Battle` - 模块战斗子系统

- **`AttackAttributeFactory.cs`**: **攻击属性工厂**，负责创建和管理不同类型的攻击属性。
- **`AttackContext.cs`**: **攻击上下文**，存储攻击相关的上下文信息。
- **`ModuleParameters.cs`**: **模块参数**，定义模块的各种参数（如生命值、攻击力等）。
- **`NormalAttribute.cs`**: **普通属性**，实现基础的属性计算逻辑。

#### 2.2.2. `Module.Config` - 模块配置子系统

- **`ExcelConfigReader.cs`**: **Excel配置读取器**，从Excel文件读取模块配置数据。
- **`RuntimeConfigApplier.cs`**: **运行时配置应用器**，在运行时应用配置到模块。

#### 2.2.3. `Module.Enums` - 模块枚举

- **`ModuleType.cs`**: 模块类型枚举
- **`AttackAttribute.cs`**: 攻击属性枚举
- **`DamageType.cs`**: 伤害类型枚举
- **`TargetCount.cs`**: 目标数量枚举
- **`TargetLockType.cs`**: 目标锁定类型枚举

#### 2.2.4. `Module.Interfaces` - 模块接口

定义了模块系统中使用的各种接口，用于实现多态和解耦。

### 2.3. `Enemy` - 敌人层

定义了所有敌方单位的行为。

- **`BaseEnemy.cs`**: **所有敌人的基类**。
  - **职责**: 定义敌人的通用行为，如移动、攻击、受伤等。

#### 2.3.1. `Enemy.Interfaces` - 敌人接口

- **`IMovable.cs`**: 移动行为接口。
- **`IAttackable.cs`**: 攻击行为接口。

#### 2.3.2. `Enemy.MovementStrategies` - 移动策略

- **`StraightMovement.cs`**: 直线移动策略。

#### 2.3.3. `Enemy.AttackStrategies` - 攻击策略

- **`MeleeAttack.cs`**: 近战攻击策略。
- **`RangedAttack.cs`**: 远程攻击策略。

#### 2.3.4. `Enemy.EnemyScript` - 具体敌人实现

- **`SimpleEnemy.cs`**: 简单敌人实现。

#### 2.3.5. `Enemy.Enums` - 敌人枚举

- **`AttackType.cs`**: 攻击类型枚举。
- **`MovementType.cs`**: 移动类型枚举。

## 3. 核心工作流程

### 3.1. 模块拼接流程

1.  **玩家点击**: `BuildController` 检测到鼠标点击。
2.  **选择模块**: 如果没有模块被选中，`BuildController` 调用 `ModuleSelector` 选中被点击的模块。
3.  **预览拼接**:
    - 当玩家选中一个模块并将其悬停在另一个可拼接的模块上时，`BuildController` 会进行射线检测。
    - `BuildController.TryPreviewAssemble` 被调用，它会计算出最佳的拼接面对齐方式，并实例化一个半透明的**预览对象**来展示拼接后的效果。
    - 预览系统会智能地检测目标变化，只在必要时更新预览。
4.  **确认拼接**:
    - 玩家再次点击，`BuildController.CompleteAssemble` 被调用。
    - `BuildController` 调用选中模块的 `AttachToFace` 方法，传入目标模块和碰撞信息。
    - `BaseModule.AttachToFace` 完成对齐、建立父子关系等操作。
    - `ModulesManager.OnModuleAttached` 被调用，将新模块注册到管理器中。
    - `ModuleSelector` 取消对模块的选择。

### 3.2. 模块拆除流程

1.  **删除模式**: 玩家按住删除键（默认E键）进入删除模式。
2.  **目标选择**: 玩家点击要删除的模块。
3.  **拆除执行**: `BuildController.HandleRemoveInput` 被调用。
4.  **更新管理器**: `ModulesManager.OnModuleDetached` 更新模块列表。

### 3.3. 战斗流程

1.  **战斗开始**: `BattleManager` 被激活。
2.  **初始化**:
    - `BattleManager` 调用 `CaculateMaxHealth()` 方法，遍历所有已注册的模块，计算出玩家的总生命值。
    - `BattleManager` 调用 `InitializeModuleBattleSystem()` 初始化所有战斗模块。
    - 更新UI显示当前生命值信息。
3.  **逻辑循环 (`Update`)**:
    - `BattleManager` 在其 `Update` 方法中，会调用所有战斗模块的 `UpdateModuleBattle` 和所有敌人的 `UpdateEnemyLogic`。
    - **索敌**: 攻击模块（如炮塔）会通过 `ModulesManager` 的敌人图层掩码系统找到目标敌人。
    - **攻击**: 模块执行攻击逻辑，通过 `BulletManager` 管理子弹实例化。
    - **受伤**: 当玩家或敌人被击中时，其生命值会减少。`BattleManager` 负责更新玩家总生命值，并在生命值归零时处理死亡逻辑。

## 4. 编辑 C# 文件时的注意事项

- **XML 注释**: 请将 XML 注释写在一行，例如：`///<summary>这是一个单行注释。</summary>`
- **命名空间**: 代码按功能划分为不同的命名空间，新增代码时请遵循现有的命名空间结构。
- **单例模式**: 多个管理器类使用单例模式，注意正确实现单例的初始化和销毁。
- **部分类**: `BattleManager` 使用部分类结构，相关功能分布在不同文件中，修改时需注意文件间的关联。
