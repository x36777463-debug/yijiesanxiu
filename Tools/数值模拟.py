# -*- coding: utf-8 -*-
"""
修仙挂机 · 数值模拟
用当前配置模拟一个"最优策略"玩家从零开始玩 30 分钟，输出每个境界的达成时间。
"""

import sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

# ---------------- 配置（与 Unity 资产一致）----------------
# id, 名字, baseCost, costGrowth, baseOutput, startLevel, unlockRealm
FACILITIES = [
    ("dazuo",   "打坐",   10,          1.15, 1,      1, 0),
    ("liandan", "炼丹",   120,         1.16, 6,      0, 0),
    ("yujian",  "御剑",   1500,        1.17, 40,     0, 1),
    ("lingshi", "灵石矿", 12000,       1.18, 250,    0, 1),
    ("juling",  "聚灵阵", 100000,      1.19, 1500,   0, 2),
    ("lianqi",  "炼器坊", 800000,      1.20, 9000,   0, 2),
    ("dongfu",  "洞府",   6000000,     1.21, 50000,  0, 3),
    ("xianmai", "仙脉",   40000000,    1.22, 300000, 0, 4),
]

# index, 名字, 倍率, 突破到下一境界所需累计灵气
REALMS = [
    (0, "练气",  1,  1500),
    (1, "筑基",  2,  30000),
    (2, "金丹",  4,  600000),
    (3, "元婴",  8,  12000000),
    (4, "化神",  16, 0),
]

DT = 0.1            # 模拟步长（秒）
MAX_T = 30 * 60     # 最长模拟 30 分钟


def simulate(strategy="best", verbose=True):
    levels   = {f[0]: f[5] for f in FACILITIES}
    spirit   = 0.0
    total    = 0.0
    realm    = 0
    t        = 0.0

    log = []            # (时间, 事件)
    upgrades = 0

    def unlocked(f):
        return f[6] <= realm

    def rate():
        base = sum(f[4] * levels[f[0]] for f in FACILITIES if unlocked(f))
        return base * REALMS[realm][2]

    def cost(f):
        return f[2] * (f[3] ** levels[f[0]])

    while t < MAX_T:
        r = rate()
        spirit += r * DT
        total  += r * DT
        t      += DT

        # 突破：只要够就立刻突破（倍率提升远大于等待）
        while realm < len(REALMS) - 1 and total >= REALMS[realm][3]:
            realm += 1
            log.append((t, f"突破 → {REALMS[realm][1]}（倍率 ×{REALMS[realm][2]}）"))

        # 买东西
        bought = True
        while bought:
            bought = False
            cands = [f for f in FACILITIES if unlocked(f) and cost(f) <= spirit]
            if not cands:
                break
            if strategy == "cheapest":
                pick = min(cands, key=lambda f: cost(f))
            else:  # best: 单位成本产出增益最高
                pick = max(cands, key=lambda f: f[4] / cost(f))
            spirit -= cost(pick)
            levels[pick[0]] += 1
            upgrades += 1
            bought = True

    return log, t, realm, levels, rate()


def fmt_time(s):
    m, sec = divmod(int(s), 60)
    return f"{m:2d}分{sec:02d}秒"


# ---------------- 主流程 ----------------
print("=" * 62)
print("  修仙挂机 · 数值模拟（最优策略：优先买性价比最高的设施）")
print("=" * 62)

log, t, realm, levels, final_rate = simulate("best")

print(f"\n【境界进度】")
print(f"  起点              0分00秒   <-- 开局")
prev = 0
for (tt, ev) in log:
    print(f"  {ev:<28} {fmt_time(tt)}")
    prev = tt

if realm < len(REALMS) - 1:
    print(f"  （{MAX_T//60} 分钟内没达到 {REALMS[realm+1][1]}）")

print(f"\n【30 分钟时的状态】")
print(f"  最终境界：{REALMS[realm][1]}")
print(f"  每秒产出：{final_rate:,.0f}")
print(f"  累计升级：{upgrades if False else ''}")
for f in FACILITIES:
    print(f"    {f[1]:<6} Lv.{levels[f[0]]:<4}")

# ---------------- 判定 ----------------
print("\n" + "=" * 62)
print("  健康度检查")
print("=" * 62)

def check(label, t_sec, lo, hi):
    ok = lo <= t_sec <= hi
    mark = "OK " if ok else "!! "
    print(f"  {mark}{label:<22} {fmt_time(t_sec):>10}   (期望 {fmt_time(lo)} ~ {fmt_time(hi)})")
    return ok

milestones = {ev.split("→")[1].strip().split("（")[0]: tt for tt, ev in log}
ok_all = True

if "筑基" in milestones:
    ok_all &= check("首次突破（筑基）", milestones["筑基"], 120, 480)
else:
    print("  !! 10 分钟内没到筑基")
    ok_all = False

if "金丹" in milestones:
    ok_all &= check("第二次突破（金丹）", milestones["金丹"], 480, 1500)
else:
    print("  !! 25 分钟内没到金丹")
    ok_all = False

print()
if ok_all:
    print("  结论：前期节奏正常，可以继续往下调中后期。")
else:
    print("  结论：需要调整数值，详见下方建议。")
