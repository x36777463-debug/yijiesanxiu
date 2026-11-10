# -*- coding: utf-8 -*-
"""
本地预览 Unity WebGL 包的小服务器（多线程 + 局域网可访问）。
用法：
    python serve_webgl.py [端口] [目录]
默认：端口 8090，目录 G:\Shader\GUI\Build\WebGL

绑定 0.0.0.0 是为了让手机能通过局域网 IP 访问。
"""
import os, sys, http.server, socket

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8090
ROOT = sys.argv[2] if len(sys.argv) > 2 else r"G:\Shader\GUI\Build\WebGL"
HOST = "0.0.0.0"


class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {
        **http.server.SimpleHTTPRequestHandler.extensions_map,
        ".wasm": "application/wasm",
        ".js": "application/javascript",
        ".data": "application/octet-stream",
    }

    protocol_version = "HTTP/1.1"

    def end_headers(self):
        path = self.translate_path(self.path)

        if not os.path.exists(path) and os.path.exists(path + ".gz"):
            self.path = self.path.split("?")[0] + ".gz"
            path = path + ".gz"

        if path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")

        self.send_header("Cache-Control", "no-store")
        super().end_headers()

    def log_message(self, fmt, *args):
        sys.stdout.write("%s - %s\n" % (self.address_string(), fmt % args))
        sys.stdout.flush()


def local_ips():
    ips = []
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ips.append(s.getsockname()[0])
        s.close()
    except Exception:
        pass
    return ips


if not os.path.isdir(ROOT):
    print("目录不存在:", ROOT)
    sys.exit(1)

os.chdir(ROOT)

print("=" * 60)
print("  目录:", ROOT)
print("  电脑访问:  http://127.0.0.1:%d" % PORT)
for ip in local_ips():
    print("  手机访问:  http://%s:%d" % (ip, PORT))
print("  按 Ctrl+C 停止")
print("=" * 60, flush=True)

srv = http.server.ThreadingHTTPServer((HOST, PORT), Handler)
srv.daemon_threads = True
srv.serve_forever()
