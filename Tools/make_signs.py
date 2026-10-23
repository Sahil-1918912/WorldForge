"""Generates the text-bearing textures (signs, posters, whiteboard) used by the WorldForge scene.

Run from the project root:  python3 Tools/make_signs.py
Output: Assets/WorldForge/Textures/Signs/*.png  (the scene builder picks them up by name)
"""
import os
from PIL import Image, ImageDraw, ImageFont

OUT = os.path.join("Assets", "WorldForge", "Textures", "Signs")
BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
REG = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
MONO = "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf"


def font(path, size):
    return ImageFont.truetype(path, size)


def centered(draw, w, y, text, fnt, fill):
    tw = draw.textlength(text, font=fnt)
    draw.text(((w - tw) / 2, y), text, font=fnt, fill=fill)


def main_sign():
    w, h = 2048, 320
    im = Image.new("RGB", (w, h), (14, 22, 38))
    d = ImageDraw.Draw(im)
    d.rectangle([8, 8, w - 9, h - 9], outline=(0, 200, 255), width=6)
    centered(d, w, 40, "WORLDFORGE  ROBOTICS  LAB", font(BOLD, 150), (90, 225, 255))
    centered(d, w, 220, "Virtual Environment  •  Introduction to AR/VR  •  Assignment 02", font(REG, 56), (200, 215, 230))
    im.save(os.path.join(OUT, "Sign_Main.png"))


def zone_sign(name, title, sub, bg, fg):
    w, h = 1024, 256
    im = Image.new("RGB", (w, h), bg)
    d = ImageDraw.Draw(im)
    d.rectangle([6, 6, w - 7, h - 7], outline=fg, width=8)
    centered(d, w, 40, title, font(BOLD, 96), fg)
    centered(d, w, 165, sub, font(REG, 44), fg)
    im.save(os.path.join(OUT, f"Sign_{name}.png"))


def safety_poster():
    w, h = 512, 768
    im = Image.new("RGB", (w, h), (250, 204, 21))
    d = ImageDraw.Draw(im)
    d.rectangle([14, 14, w - 15, h - 15], outline=(20, 20, 20), width=14)
    # warning triangle
    d.polygon([(256, 70), (420, 350), (92, 350)], fill=(20, 20, 20))
    d.polygon([(256, 120), (376, 325), (136, 325)], fill=(250, 204, 21))
    d.rectangle([243, 175, 269, 270], fill=(20, 20, 20))
    d.ellipse([240, 285, 272, 317], fill=(20, 20, 20))
    centered(d, w, 400, "CAUTION", font(BOLD, 84), (20, 20, 20))
    centered(d, w, 510, "ROBOT IN", font(BOLD, 56), (20, 20, 20))
    centered(d, w, 575, "OPERATION", font(BOLD, 56), (20, 20, 20))
    centered(d, w, 670, "Stay outside the yellow zone", font(REG, 28), (20, 20, 20))
    im.save(os.path.join(OUT, "Poster_Safety.png"))


def whiteboard():
    w, h = 1536, 768
    im = Image.new("RGB", (w, h), (246, 247, 249))
    d = ImageDraw.Draw(im)
    blue, red, green, black = (30, 70, 200), (200, 40, 40), (20, 140, 70), (30, 30, 30)
    d.text((60, 40), "Pick & Place Arm  -  4 DOF", font=font(BOLD, 64), fill=blue)
    m = font(MONO, 40)
    d.text((60, 150), "yaw   = atan2(x, z)", font=m, fill=black)
    d.text((60, 210), "cos q2 = (d² - L1² - L2²) / 2 L1 L2", font=m, fill=black)
    d.text((60, 270), "q1    = atan2(r, h) - atan2(L2 sin q2, L1 + L2 cos q2)", font=m, fill=black)
    d.text((60, 330), "q3    = 180° - q1 - q2   (gripper points down)", font=m, fill=black)
    d.text((60, 430), "TODO:", font=font(BOLD, 44), fill=red)
    d.text((60, 490), "✓ calibrate conveyor speed", font=font(REG, 40), fill=green)
    d.text((60, 545), "✓ test sliding door sensor", font=font(REG, 40), fill=green)
    d.text((60, 600), "• water the garden plants :)", font=font(REG, 40), fill=black)
    # little arm sketch
    pts = [(1180, 650), (1180, 560), (1300, 400), (1420, 470), (1420, 560)]
    d.line(pts, fill=blue, width=10, joint="curve")
    for p in pts[1:4]:
        d.ellipse([p[0] - 16, p[1] - 16, p[0] + 16, p[1] + 16], outline=red, width=6)
    d.rectangle([1120, 650, 1240, 690], outline=black, width=6)
    d.text((1230, 360), "L1", font=font(BOLD, 36), fill=red)
    d.text((1370, 380), "L2", font=font(BOLD, 36), fill=red)
    im.save(os.path.join(OUT, "Whiteboard.png"))


def screen_code():
    w, h = 1024, 576
    im = Image.new("RGB", (w, h), (12, 18, 32))
    d = ImageDraw.Draw(im)
    d.rectangle([0, 0, w, 40], fill=(28, 44, 72))
    d.text((16, 8), "arm_controller.py  -  WorldForge IDE", font=font(BOLD, 22), fill=(170, 210, 255))
    kw, fn, st, cm, tx = (255, 120, 200), (110, 200, 255), (160, 230, 120), (110, 130, 150), (225, 230, 240)
    lines = [
        [(cm, "# 4-DOF pick & place controller")],
        [(kw, "import "), (tx, "numpy "), (kw, "as "), (tx, "np")],
        [],
        [(kw, "def "), (fn, "solve_ik"), (tx, "(target, L1=1.10, L2=1.00):")],
        [(tx, "    yaw = np.arctan2(target.x, target.z)")],
        [(tx, "    r, h = np.hypot(target.x, target.z), target.y - SHOULDER")],
        [(tx, "    c2 = (r*r + h*h - L1*L1 - L2*L2) / (2*L1*L2)")],
        [(tx, "    q2 = np.arccos(np.clip(c2, -1, 1))")],
        [(tx, "    q1 = np.arctan2(r, h) - np.arctan2(L2*np.sin(q2), L1 + L2*np.cos(q2))")],
        [(kw, "    return "), (tx, "yaw, q1, q2, np.pi - q1 - q2")],
        [],
        [(kw, "for "), (tx, "pad "), (kw, "in "), (tx, "["), (st, "'PAD_A'"), (tx, ", "), (st, "'PAD_B'"), (tx, "]:")],
        [(tx, "    arm.move_to(hover(pad));  arm.descend()")],
        [(tx, "    gripper.close();  log("), (st, "'picked crate'"), (tx, ")")],
        [(cm, "    # status: OK  |  cycle time 7.4 s  |  error 0.3 mm")],
    ]
    mono = font(MONO, 21)
    y = 58
    for i, line in enumerate(lines):
        d.text((14, y), f"{i + 1:>2}", font=mono, fill=(80, 95, 120))
        x = 64
        for col, txt in line:
            d.text((x, y), txt, font=mono, fill=col)
            x += d.textlength(txt, font=mono)
        y += 33
    im.save(os.path.join(OUT, "Screen_Code.png"))


def screen_dashboard():
    import math
    w, h = 1024, 576
    im = Image.new("RGB", (w, h), (10, 16, 28))
    d = ImageDraw.Draw(im)
    d.text((24, 16), "LAB TELEMETRY", font=font(BOLD, 34), fill=(120, 220, 255))
    d.text((760, 24), "● ONLINE", font=font(BOLD, 26), fill=(90, 230, 120))
    # line chart
    d.rectangle([24, 80, 620, 330], outline=(50, 80, 120), width=2)
    for gx in range(24, 620, 60):
        d.line([(gx, 80), (gx, 330)], fill=(25, 40, 62))
    pts = [(24 + i * 6, 205 - 70 * math.sin(i / 9.0) * math.cos(i / 23.0) - 15 * math.sin(i / 2.5)) for i in range(100)]
    d.line(pts, fill=(90, 220, 255), width=4)
    pts2 = [(24 + i * 6, 260 - 30 * math.cos(i / 7.0)) for i in range(100)]
    d.line(pts2, fill=(255, 170, 60), width=3)
    d.text((34, 88), "Joint torque (Nm)", font=font(REG, 20), fill=(160, 180, 200))
    # bars
    vals = [0.8, 0.55, 0.92, 0.4, 0.7]
    names = ["J1", "J2", "J3", "J4", "GRP"]
    for i, (v, n) in enumerate(zip(vals, names)):
        x0 = 660 + i * 70
        d.rectangle([x0, 330 - int(230 * v), x0 + 44, 330], fill=(60, 140 + i * 20, 255))
        d.text((x0 + 4, 338), n, font=font(BOLD, 20), fill=(180, 200, 220))
    # stat tiles
    stats = [("Cycles", "1,284"), ("Uptime", "99.7%"), ("Temp", "41 °C"), ("Crates/h", "486")]
    for i, (k, v) in enumerate(stats):
        x0 = 24 + i * 250
        d.rectangle([x0, 400, x0 + 230, 540], fill=(18, 30, 50), outline=(50, 80, 120), width=2)
        d.text((x0 + 16, 412), k, font=font(REG, 24), fill=(150, 170, 195))
        d.text((x0 + 16, 452), v, font=font(BOLD, 52), fill=(235, 245, 255))
    im.save(os.path.join(OUT, "Screen_Dashboard.png"))


def screen_map():
    import random
    rnd = random.Random(3)
    w, h = 1024, 576
    im = Image.new("RGB", (w, h), (8, 12, 20))
    d = ImageDraw.Draw(im)
    d.text((24, 16), "NAVIGATION MAP  -  SLAM", font=font(BOLD, 30), fill=(150, 255, 170))
    ox, oy, cs = 24, 70, 16
    for gy in range(30):
        for gx in range(60):
            wall = gx in (0, 59) or gy in (0, 29) or (gx == 22 and gy < 18) or (gy == 12 and 30 < gx < 52) or rnd.random() < 0.02
            col = (200, 210, 220) if wall else (24, 36, 52)
            d.rectangle([ox + gx * cs, oy + gy * cs, ox + gx * cs + cs - 2, oy + gy * cs + cs - 2], fill=col)
    path = [(4, 25), (12, 25), (12, 21), (26, 21), (26, 6), (44, 6), (54, 20)]
    d.line([(ox + x * cs + 7, oy + y * cs + 7) for x, y in path], fill=(255, 90, 160), width=6)
    x, y = path[-1]
    d.ellipse([ox + x * cs - 4, oy + y * cs - 4, ox + x * cs + 18, oy + y * cs + 18], fill=(90, 255, 140))
    im.save(os.path.join(OUT, "Screen_Map.png"))


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    main_sign()
    zone_sign("RobotCell", "ROBOT CELL", "Pick & place  •  keep clear", (250, 204, 21), (20, 20, 20))
    zone_sign("Conveyor", "CONVEYOR LINE", "Packaging & inspection", (16, 40, 70), (120, 220, 255))
    zone_sign("Workstations", "WORKSTATIONS", "Simulation & control", (16, 40, 70), (120, 220, 255))
    zone_sign("Garden", "INNOVATION GARDEN", "Open air thinking space", (24, 60, 32), (200, 240, 190))
    safety_poster()
    whiteboard()
    screen_code()
    screen_dashboard()
    screen_map()
    print("signs written to", OUT)
