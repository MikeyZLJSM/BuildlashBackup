# Unity项目代码结构总结 - AI助手参考

## 项目类型识别
- **引擎**: Unity 3D游戏引擎
- **语言**: C# (.NET)
- **项目类型**: 模块化建造+战斗游戏
- **架构模式**: 面向对象设计，策略模式，单例模式，组件化架构

## 核心命名空间层次结构

### 1. Module命名空间 - 模块化建造系统
```
Module/
├── BaseModule (抽象基类，分部类设计)
│   ├── BaseModule.Attach.cs - 模块连接逻辑
│   └── BaseModule.Battle.cs - 战斗相关属性
├── ModuleScript/ - 具体模块实现
│   ├── Cube/ (BaseCube, NormalCube)
│   ├── Sphere/ (NormalSphere, CrystalBall)
│   ├── Cylinder/ (NormalCylinder)
│   └── Cone/ (NormalCone)
├── Battle/ - 战斗机制
│   ├── AttackParameters, AttackContext
│   ├── AttackAttributeFactory
│   └── NormalAttribute
├── Interfaces/ - 接口定义
│   ├── IAttachable - 可连接接口
│   ├── IAttackable - 可攻击接口
│   └── IAttackAttribute - 攻击属性接口
└── Enums/ - 枚举定义
    ├── ModuleType, DamageType
    ├── AttackAttribute, TargetCount
    └── TargetLockType
```

### 2. Enemy命名空间 - 敌人系统
```
Enemy/
├── BaseEnemy (抽象基类)
├── EnemyScript/ (SimpleEnemy等具体实现)
├── AttackStrategies/ (MeleeAttack, RangedAttack)
├── MovementStrategies/ (StraightMovement)
├── Interfaces/ (IMovable, IAttackable)
└── Enums/ (MovementType, AttackType)
```

### 3. Controllers命名空间 - 游戏控制层
```
Controllers/
├── 核心管理器:
│   ├── BattleManager (单例，分部类)
│   ├── ModulesManager
│   ├── SceneManager
│   └── UIController
├── 构建系统:
│   ├── BuildController
│   └── ModuleSelector
├── 视觉控制:
│   └── CameraScript
└── Battle/ - 战斗子系统
    ├── BattleManager.Module.cs
    ├── BattleManager.Enemy.cs
    ├── BulletManager
    ├── Bullet
    └── NormalAttribute
```

## 关键设计模式识别

### 1. 策略模式应用
- **AttackStrategies**: MeleeAttack, RangedAttack
- **MovementStrategies**: StraightMovement
- **用途**: 敌人行为多样化

### 2. 工厂模式应用
- **AttackAttributeFactory**: 创建攻击属性实例
- **用途**: 统一管理攻击属性创建逻辑

### 3. 单例模式应用
- **BattleManager.Instance**: 战斗管理器单例
- **UIController.Instance**: UI控制器单例
- **用途**: 全局状态管理和跨脚本通信

### 4. 组件化设计
- 所有核心类继承MonoBehaviour
- 使用RequireComponent确保依赖组件
- 模块化附加系统通过Transform父子关系实现

### 5. 分部类设计
- **BaseModule**: 分为Attach和Battle两个文件
- **BattleManager**: 分为主文件、Module和Enemy子文件
- **用途**: 代码组织和职责分离

## 核心系统交互关系

### 模块构建流程:
BuildController → ModuleSelector → ModulesManager → BaseModule.Attach

### 战斗执行流程:
BattleManager → Enemy.BaseEnemy → AttackStrategies → Bullet → Module.IAttackable

### UI更新流程:
各系统 → UIController.Instance → Unity UI系统

## 重要接口契约

### IAttachable (模块连接)
- 定义模块如何连接到其他模块
- 实现者: BaseModule及其子类

### IAttackable (攻击目标)
- 同时存在于Module和Enemy命名空间
- 定义可被攻击的对象接口

### IMovable (移动能力)
- Enemy系统中的移动接口
- 配合MovementStrategies使用

## 资源组织结构

### 预制件(Prefabs)层次:
- **UI/**: UI预制件
- **Modules/**: 各种形状模块预制件
- **Enemies/**: 敌人预制件
- **根级别**: Controller, Bullet等核心预制件

### 材质系统:
- 基于物理的渲染材质
- 按功能分类: BaseCube, Bullet, CrystalBall等

## 场景结构:
- **ModuleBuilding.unity**: 模块构建场景
- **Battle.unity**: 战斗场景

## 代码修改指导原则

1. **添加新模块**: 继承BaseModule，实现ModuleScript目录下
2. **添加新敌人**: 继承BaseEnemy，实现EnemyScript目录下
3. **添加新攻击方式**: 实现AttackStrategies目录下
4. **添加新移动模式**: 实现MovementStrategies目录下
5. **修改UI**: 通过UIController.Instance统一管理
6. **扩展战斗逻辑**: 在BattleManager分部类中添加

## 依赖关系图
```
Controllers → Module, Enemy
Module.Battle → Controllers.Battle
Enemy → Controllers.Battle
All Systems → UIController (单例)
```

此结构支持高度模块化的游戏开发，便于扩展和维护。
