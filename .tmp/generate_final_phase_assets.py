from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def chunk(tag: bytes, data: bytes) -> bytes:
    return (
        struct.pack(">I", len(data))
        + tag
        + data
        + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
    )


def write_png(path: Path, width: int, height: int, pixel_fn) -> None:
    if path.exists():
        return

    rows = bytearray()
    for y in range(height):
        rows.append(0)
        for x in range(width):
            r, g, b, a = pixel_fn(x, y, width, height)
            rows.extend((r, g, b, a))

    ihdr = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    data = zlib.compress(bytes(rows), level=9)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", ihdr) + chunk(b"IDAT", data) + chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(png)


def clamp(value: float) -> int:
    return max(0, min(255, int(round(value))))


def blend(a: tuple[int, int, int], b: tuple[int, int, int], t: float) -> tuple[int, int, int]:
    return tuple(clamp(a[i] + (b[i] - a[i]) * t) for i in range(3))


def crystal_pixel(x: int, y: int, width: int, height: int, base_a, base_b, accent) -> tuple[int, int, int, int]:
    nx = (x + 0.5) / width
    ny = (y + 0.5) / height
    cx = nx - 0.5
    cy = ny - 0.5
    radial = max(0.0, 1.0 - math.sqrt(cx * cx + cy * cy) * 1.65)
    stripes = 0.5 + 0.5 * math.sin(nx * 22.0 + ny * 14.0)
    color = blend(base_a, base_b, ny * 0.7 + stripes * 0.3)

    facets = [
        ((0.50, 0.14), (0.76, 0.46), 0.15),
        ((0.30, 0.40), (0.56, 0.84), 0.12),
        ((0.68, 0.42), (0.92, 0.88), 0.10),
    ]
    alpha = 220 if radial > 0.15 else 160

    for (ax, ay), (bx, by), radius in facets:
        px = nx - (ax + bx) * 0.5
        py = ny - (ay + by) * 0.5
        dist = math.sqrt(px * px + py * py)
        if dist < radius:
            mix = 1.0 - dist / radius
            color = blend(color, accent, mix * 0.85)
            alpha = 255

    if 0.16 < nx < 0.84 and 0.10 < ny < 0.90 and abs(cx) + abs(cy) < 0.62:
        edge = abs(abs(cx) - 0.14) < 0.03 or abs(cx + cy * 0.45) < 0.03
        if edge:
            color = blend(color, (245, 250, 255), 0.8)

    vignette = 0.55 + radial * 0.45
    return (
        clamp(color[0] * vignette),
        clamp(color[1] * vignette),
        clamp(color[2] * vignette),
        alpha,
    )


def portal_pixel(x: int, y: int, width: int, height: int) -> tuple[int, int, int, int]:
    nx = (x + 0.5) / width
    ny = (y + 0.5) / height
    cx = nx - 0.5
    cy = ny - 0.55
    dist = math.sqrt(cx * cx + cy * cy)
    bg = blend((10, 22, 30), (16, 32, 44), ny)

    frame = (66, 86, 94)
    energy = blend((52, 196, 235), (236, 252, 255), max(0.0, 1.0 - dist / 0.32))
    ring = abs(dist - 0.23) < 0.055 and ny < 0.86
    inner = dist < 0.20 and ny < 0.86
    plinth = 0.23 < nx < 0.77 and 0.76 < ny < 0.92

    color = bg
    alpha = 255
    if ring:
        color = blend(frame, energy, 0.82)
    elif inner:
        swirl = 0.5 + 0.5 * math.sin((cx * 18.0) - (cy * 12.0))
        color = blend((34, 110, 146), energy, 0.55 + swirl * 0.35)
    elif plinth:
        color = blend((62, 74, 78), (115, 130, 136), (ny - 0.76) / 0.16)
    elif 0.18 < nx < 0.28 and 0.34 < ny < 0.84 or 0.72 < nx < 0.82 and 0.34 < ny < 0.84:
        color = frame

    spark = abs(cx) < 0.02 or abs(cy + 0.04) < 0.015
    if inner and spark:
        color = blend(color, (255, 255, 255), 0.9)

    return color[0], color[1], color[2], alpha


def terrain_pixel(x: int, y: int, width: int, height: int, colors) -> tuple[int, int, int, int]:
    nx = x / max(1, width - 1)
    ny = y / max(1, height - 1)
    wave = 0.5 + 0.5 * math.sin(nx * 10.0 + ny * 6.0)
    color = blend(colors[0], colors[1], wave * 0.55 + ny * 0.45)
    crack = ((x + y) % 11 == 0) or ((x * 2 + y * 3) % 17 == 0)
    if crack:
        color = blend(color, colors[2], 0.72)
    return color[0], color[1], color[2], 255


def spear_pixel(x: int, y: int, width: int, height: int) -> tuple[int, int, int, int]:
    color = (42, 26, 12)
    if abs(x - width // 2) <= 1 and 4 < y < height - 3:
        color = (109, 78, 46)
    if y <= 5 and abs(x - width // 2) <= (6 - y):
        color = (190, 207, 214)
    if y > 9 and abs(x - width // 2) == 2 and y < height - 4:
        color = (163, 112, 64)
    return color[0], color[1], color[2], 255


def bench_upgrade_pixel(x: int, y: int, width: int, height: int) -> tuple[int, int, int, int]:
    bg = blend((46, 34, 22), (89, 61, 33), y / max(1, height - 1))
    if 3 < x < width - 4 and 4 < y < height - 4:
        bg = blend(bg, (162, 118, 63), 0.35)
    if abs(x - width // 2) <= 1 or abs(y - height // 2) <= 1:
        bg = blend(bg, (118, 188, 214), 0.72)
    return bg[0], bg[1], bg[2], 255


def badge_pixel(x: int, y: int, width: int, height: int) -> tuple[int, int, int, int]:
    nx = (x + 0.5) / width
    ny = (y + 0.5) / height
    base = blend((14, 48, 66), (24, 103, 130), ny)
    if 0.1 < nx < 0.9 and 0.1 < ny < 0.9:
        base = blend(base, (34, 148, 190), 0.35)
    if abs(nx - 0.5) + abs(ny - 0.5) < 0.34:
        base = blend(base, (232, 249, 255), 0.75)
    return base[0], base[1], base[2], 255


ASSETS = [
    ("packs/basegame/assets/items/resources/ice_crystal.png", 16, 16, lambda x, y, w, h: crystal_pixel(x, y, w, h, (88, 180, 224), (214, 245, 255), (255, 255, 255))),
    ("packs/basegame/assets/items/weapons/wood_spear.png", 16, 16, spear_pixel),
    ("packs/basegame/assets/items/resources/upgrade_workbench_weapons_bench.png", 16, 16, bench_upgrade_pixel),
    ("packs/portalmod/assets/world/ruins/portal.png", 32, 32, portal_pixel),
    ("packs/portalmod/assets/world/terrain/ground/dimfrag.png", 32, 32, lambda x, y, w, h: terrain_pixel(x, y, w, h, ((20, 48, 64), (54, 110, 140), (220, 248, 255)))),
    ("packs/portalmod/assets/world/nature/ores/frostcore.png", 32, 32, lambda x, y, w, h: crystal_pixel(x, y, w, h, (48, 118, 168), (205, 240, 255), (242, 252, 255))),
    ("packs/portalmod/assets/items/resources/frostcore_item.png", 16, 16, lambda x, y, w, h: crystal_pixel(x, y, w, h, (48, 118, 168), (205, 240, 255), (242, 252, 255))),
    ("packs/portalmod/assets/ui/portalmod_badge.png", 32, 32, badge_pixel),
]


def main() -> None:
    for relative_path, width, height, pixel_fn in ASSETS:
        write_png(ROOT / relative_path, width, height, pixel_fn)

    print(f"Generated or confirmed {len(ASSETS)} final-phase assets.")


if __name__ == "__main__":
    main()
