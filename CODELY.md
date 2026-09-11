

## Codely Structured Memories

### User
- [2026-09-08 11:29:01] 用户的 Windows 显示器开启着 HDR（2026-09-08 确认），Unity 2022.3 编辑器（SDR 应用）画面会整体发灰——排查色彩/饱和度/HDR 输出相关问题时优先考虑此因素。
### Feedback

### Project
- [2026-09-11 12:04:22] 玩家角色是 CT "Free Low Poly Cubic Humans" 的 Knight：其 CLP Avatar 骨架没有手骨（m_HasLeftHand/RightHand=0，GetBoneTransform 对手部骨骼一律返回 null）。**Why:** 所以"武器挂到手上"不能依赖 Animator 手骨，只能用场景里手动建的 Hand 子物体做挂点（PlayerWeaponHolder.handSlot 手动指定优先于自动取骨）。**How to apply:** 以后做武器/道具挂手、手部 IK、动画驱动武器时，直接走手动槽位方案，别再排查 GetBoneTransform。

### Reference

