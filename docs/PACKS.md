# Packs Layout

`packs/` 用于承载所有内容包，包括基础游戏内容和未来的 Mod。

设计原则：

- `basegame` 与未来 Mod 同级。
- 游戏核心不对 `basegame` 做特殊路径判断。
- 内容扫描、注册、启停未来都基于 `packs/*` 展开。

推荐结构：

```text
packs/
  basegame/
    assets/
    defs/
    scenes/

  portalmod/
    assets/
    defs/
    scenes/
```

约定：

- `assets/`：贴图、音频、字体等静态资源
- `defs/`：物品、配方、地形、生物等数据定义
- `scenes/`：该 pack 专属的 Godot 场景

当前仓库状态：

- `packs/basegame` 是默认内容包。
- 后续新增内容包时，保持同样层级，不要再把内容直接放到仓库根目录。

