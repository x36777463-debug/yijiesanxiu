# -*- coding: utf-8 -*-
"""
把 Unity WebGL 构建目录打成 itch.io 能用的 zip。
关键点：ZIP 规范要求路径用正斜杠 `/`，Windows 的 Compress-Archive 会写反斜杠，
       Linux 服务器（itch.io）就找不到子目录。这里手动指定 arcname。
"""
import os, sys, io, zipfile

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

SRC = r"G:\Shader\GUI\Build\WebGL"
OUT = r"F:\桌面\C# Unity\一介散修-webgl.zip"

if not os.path.isdir(SRC):
    print("源目录不存在:", SRC)
    sys.exit(1)

if os.path.exists(OUT):
    os.remove(OUT)

count = 0
with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    for root, dirs, files in os.walk(SRC):
        for f in sorted(files):
            full = os.path.join(root, f)
            rel = os.path.relpath(full, SRC)
            # 强制正斜杠
            arc = rel.replace("\\", "/")
            z.write(full, arc)
            count += 1
            print(f"  {arc:<40} {os.path.getsize(full)/1024:8.1f} KB")

print()
print(f"共 {count} 个文件，zip 大小 {os.path.getsize(OUT)/1024/1024:.2f} MB")
print("输出:", OUT)

print("\n===== 校验：条目里的分隔符必须是 / =====")
with zipfile.ZipFile(OUT) as z:
    names = z.namelist()
    bad = [n for n in names if "\\" in n]
    print("  含反斜杠的条目:", len(bad) if bad else "0  ✅")
    print("  index.html 在根目录:", "index.html" in names)
    print("\n  前 6 个条目:")
    for n in names[:6]:
        print("   ", n)
