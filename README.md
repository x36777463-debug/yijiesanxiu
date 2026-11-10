# 一介散修 · 修仙放置游戏

> 一款用 Unity 2022.3 + C# 独立完成的 2D 放置（Idle）游戏。
> 从玩法设计、系统架构、UI、存档到打包发布，全部由我自己完成。

![游戏截图](docs/screenshots/main.png)

---

## 在线试玩

- **浏览器直接玩**：https://qianyang111.itch.io/yijiesanxiu
- **演示视频**：（待补充）
- 建议使用 Chrome / Edge，首次加载约 20~40 秒

> ⚠️ 游戏为竖屏（1080 × 1920），页面上请点击右下角**全屏按钮**获得最佳体验。

---

## 玩法

```
灵气自动增长 → 花灵气升级 8 种设施 → 产出变快
            → 累计修为达标后突破境界（全局倍率翻倍）
            → 解锁更高阶设施
```

- **8 种设施**：打坐、炼丹、御剑、灵石矿、聚灵阵、炼器坊、洞府、仙脉
- **5 级境界**：练气 → 筑基 → 金丹 → 元婴 → 化神（每级 ×2 倍率）
- **离线收益**：关掉游戏再回来，按离线时长补发收益（8 小时封顶）

---

## 技术实现

### 分层架构

```
UI 层      UIManager / Panel 子类 / ItemView
           只负责显示与转发点击，不持有游戏数据
              ↑ 订阅事件          ↓ 调用方法
逻辑层     GameManager（唯一数据源）
           FacilitySystem / RealmSystem / OfflineSystem
              ↑ 读写
数据层     GameData（运行时数据） + ScriptableObject 配置表
              ↑
存档层     SaveSystem（JSON 序列化 + 版本迁移）
```

**三条约束**：

1. **唯一数据源**：只有 `GameManager` 持有 `GameData`，UI 通过 `OnSpiritChanged` 等事件订阅刷新，界面里不出现业务判断。
2. **配置与运行时分离**：设施/境界的数值全部是 `ScriptableObject` 资产，改数值不用改代码、不用重新编译。
3. **UI 不引用玩法对象**：加一个显示血量的地方，不需要改玩家脚本。

### 存档系统 `SaveSystem`

- **JSON 序列化**，存储在 `Application.persistentDataPath`
- **版本号 + 迁移**：存档带 `version` 字段，读档后按版本补齐默认值，加新字段不会让老玩家进度归零
- **坏档兜底**：整个读档流程 `try/catch`，文件损坏/格式错误时自动开新档，而不是白屏崩溃
- **字段缺失兼容**：配置新增设施时，读档阶段自动补齐缺失条目

### 离线收益 `OfflineSystem`

- 用 **UTC 时间戳**记录存档时间，玩家改时区不影响结算
- **时长上限** 8 小时，防止改系统时间刷收益
- **时钟回拨防护**：若算出的离线时长为负（系统时间被往前调），直接不发奖励
- 收益效率系数可配置（默认 50%）

### 数值设计

数值不是拍脑袋定的，而是**先用 Python 模拟再落地**：

1. 写了一个 `数值模拟.py`，模拟"最优策略玩家"（每 tick 买性价比最高的设施）
2. 跑出完整成长曲线，发现初版 **9 分钟就能通关到化神**，数值严重失衡
3. 按目标时长（筑基 3 分 / 金丹 15 分 / 元婴 45 分 / 化神 2 小时）重设五级突破门槛
4. 重新模拟验证，最终版本曲线平滑、无明显卡点

> 模拟器考虑了境界倍率、设施解锁、成本指数增长等因素，结果与真人试玩基本吻合。

### 其他

| 模块 | 实现 |
| --- | --- |
| 音效 | `AudioManager` 单例，音量存 `PlayerPrefs`，可静音 |
| UI 动画 | 协程实现 `Punch`（弹跳）与 `Breathe`（新手引导呼吸） |
| 新手引导 | 首个设施按钮做呼吸动画，玩家升过级后自动停止 |
| 字体 | TMP 动态 SDF 图集（2048，采样 64，多图集），中文按需生成 |
| 退出保护 | `OnApplicationPause` / `OnApplicationQuit` 双保险存档 |

---

## 目录结构

```
Assets/
├── Config/            设施与境界配置（ScriptableObject）
├── Font/              TMP 字体资产
├── Audio/             音效（Python 程序化合成，无版权）
├── Prefabs/           UI 预制体
├── Scenes/
│   └── Main.unity
└── Script/ProjectBase/
    ├── Core/          GameManager / ConfigDatabase / SaveSystem
    ├── Data/          GameData / FacilityData
    ├── Systems/       OfflineSystem / NumberFormatter
    ├── UI/            MainPanel / SettingsPanel / OfflinePanel ...
    └── Audio/         AudioManager / UIAnimator
```

---

## 如何运行

### 浏览器

直接访问 https://qianyang111.itch.io/yijiesanxiu

### 本地（Unity）

1. 安装 Unity **2022.3 LTS**
2. 用 Unity Hub 打开本项目根目录
3. 打开 `Assets/Scenes/Main.unity`
4. 按 Play

### 本地（WebGL 构建）

```bash
# 需要用支持 gzip 的静态服务器，普通的 http.server 不行
python Tools/serve_webgl.py 8090
# 浏览器打开 http://127.0.0.1:8090
```

### 重新打包

```bash
# 1. Unity: File → Build Settings → WebGL → Build（输出到 Build/WebGL）
# 2. 打成 itch.io 可用的 zip（正斜杠路径）
python Tools/打包webgl.py
```

---

## 开发记录

| 日期 | 内容 |
| --- | --- |
| 2026.10 下旬 | 系统学习 C# 四部曲（入门→进阶），逐节录制讲解视频 39 期 |
| 2026.11 上旬 | 完成游戏原型：单例架构、配置表、UGUI 列表、存档 |
| 2026.11 中旬 | 加入境界系统、8 设施、离线收益、音效、新手引导 |
| 2026.11 中旬 | Python 数值模拟调优，平衡曲线 |
| 2026.11 中旬 | WebGL 打包、发布 itch.io |

---

## 踩过的坑

记录几个真实调试过程，都是"编辑器里正常、打包后出问题"的典型：

1. **打包报 `Broken text PPtr`**
   配置表里某个资产的 GUID 少写了两位（GUID 必须是 32 位十六进制）。编辑器里不报错，只是静默把引用解析成 `null`，表现为"第五个境界永远不出现"。改 meta 与引用对齐后解决。

2. **itch.io 上传后子目录全部 404**
   Windows 的 `Compress-Archive` 用反斜杠 `\` 写 zip 路径，而 ZIP 规范要求 `/`。Linux 服务器把 `Build\xxx.js` 当成文件名，导致根目录能加载、子目录全 404。改用 Python `zipfile` 手动指定正斜杠的 arcname。

3. **WebGL 画面被裁切**
   Player Settings 里的 Canvas 尺寸（1080×1920）与 iframe 尺寸不一致时，Unity 不会等比缩放，而是按画布尺寸渲染后裁切。两边设成一致后解决。

---

## 已知问题 / 后续计划

- [ ] 设施数量偏少（8 个），中后期内容单薄
- [ ] 缺少成就 / 统计数据
- [ ] 未做多语言
- [ ] Android APK 版本（需要 Android Build Support 模块）
- [ ] 导表工具：CSV → ScriptableObject，当前配置仍是手动创建资产

---

## 关于

- 作者：杨峰
- 联系：x36777463@126.com
- 定位：Unity 游戏客户端开发（杭州）
