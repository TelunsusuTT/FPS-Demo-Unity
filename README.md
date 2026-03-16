# FPS Demo (Unity)

这是一个使用 **Unity + C#** 实现的第一人称射击 Demo，主要用于实现和整理常见 FPS 客户端系统，包括玩家控制、武器射击、UI 反馈、敌人 AI 和对象池等模块。

项目的代码按照不同系统进行拆分，每个系统负责独立的功能，通过事件和接口进行简单解耦。

---

# 项目结构

```text
Player
 ├─ PlayerController.cs
 └─ WeaponInput.cs

Weapon
 ├─ WeaponController.cs
 ├─ BulletController.cs
 └─ RecollControl.cs

Enemy
 ├─ EnemyAI.cs
 └─ EnemyController.cs

Combat
 └─ IDamageable.cs

UI
 ├─ CrosshairUI.cs
 ├─ HitMarkerUI.cs
 └─ AmmoUI.cs

VFX
 └─ MuzzleFlashVFX.cs
```

---

# 玩家系统

玩家控制主要由 `PlayerController` 和 `WeaponInput` 两个脚本组成。

### PlayerController.cs

负责：

* 玩家移动
* 鼠标视角控制
* 跳跃
* ADS（瞄准状态）

该脚本通过 `Rigidbody` 控制移动，并管理玩家的一些基础状态，例如 `isAiming`。

### WeaponInput.cs

负责处理输入：

* 鼠标开火
* 瞄准输入
* 调用武器系统的射击逻辑

输入与武器逻辑分离，避免 `WeaponController` 直接依赖输入。

---

# 武器系统

武器逻辑集中在 `WeaponController`。

### WeaponController.cs

负责：

* 射速控制
* 弹药管理
* 射击逻辑
* 触发射击事件

射击流程：

```
WeaponInput
   ↓
WeaponController.TryFire()
   ↓
相机 Raycast 计算目标点
   ↓
生成 Bullet
```

### RecollControl.cs

负责武器后坐力：

* 射击时增加旋转
* 自动恢复

### BulletController.cs

负责子弹行为：

* 子弹移动
* 碰撞检测
* 命中目标后调用 `IDamageable`

---

# 伤害系统

项目使用一个简单接口来处理伤害。

### IDamageable.cs

```csharp
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

任何实现这个接口的对象都可以被子弹攻击。

当前实现：

* `EnemyController`

---

# UI 系统

UI 目前包含三个部分。

### CrosshairUI.cs

负责动态准星。

准星会根据：

* 玩家移动
* 鼠标旋转
* 开火

改变扩散距离。

准星由四个子物体组成：

```
LeftDash
RightDash
UpperDash
LowerDash
```

通过修改 `RectTransform` 位置实现扩散。

---

### HitMarkerUI.cs

当子弹命中敌人时显示命中提示。

由武器系统触发。

---

### AmmoUI.cs

负责显示弹药数量。

格式：

```
CurrentAmmo / ReserveAmmo
```

---

# VFX

### MuzzleFlashVFX.cs

用于控制枪口火焰效果。

在射击时生成并短时间自动销毁。

---

# 敌人 AI

敌人行为由 `EnemyAI` 控制。

### EnemyAI.cs

使用简单状态机实现敌人逻辑。

状态包括：

```
Patrol
Chase
Attack
Dead
```

主要逻辑：

* 巡逻：在出生点附近随机移动
* 发现玩家：进入追击
* 进入攻击范围：攻击玩家
* 被击杀：进入死亡状态

导航通过 **NavMeshAgent** 实现。

---

### EnemyController.cs

负责敌人生命值和死亡逻辑。

实现 `IDamageable` 接口：

```csharp
TakeDamage()
```

当生命值为 0 时触发死亡事件。

---

# 当前实现的主要系统

目前项目实现的主要模块包括：

* 玩家 3C 控制
* 武器射击系统
* 子弹与命中检测
* 动态准星
* 命中反馈
* 敌人 AI
* 简单 UI 系统

这些系统通过事件和接口进行简单连接，方便后续继续扩展。

---

# 作者

Terrence Tan
MSc Computing
Imperial College London

```
