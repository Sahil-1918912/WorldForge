using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WorldForge.EditorTools
{
    /// <summary>
    /// Procedurally paints the tileable surface textures (albedo + normal maps) used by the scene
    /// and saves them as PNG assets under Assets/WorldForge/Textures/Generated.
    /// All noise is periodic, so every texture tiles without seams.
    /// </summary>
    public static class TextureFactory
    {
        public const string Folder = "Assets/WorldForge/Textures/Generated";
        const int Size = 512;

        public static Dictionary<string, Texture2D> GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            var made = new List<(string name, bool normal, bool alpha)>();

            void Albedo(string name, Func<float, float, Color> paint, int size = Size, bool alpha = false)
            {
                Save(name, Paint(size, size, paint));
                made.Add((name, false, alpha));
            }

            void Normal(string name, Func<float, float, float> height, float strength)
            {
                Save(name, NormalFromHeight(Size, height, strength));
                made.Add((name, true, false));
            }

            Albedo("FloorTiles", FloorTiles);
            Normal("FloorTiles_N", FloorTilesHeight, 4f);
            Albedo("Concrete", Concrete);
            Normal("Concrete_N", (u, v) => Fbm(u, v, 8, 5, 11), 1.2f);
            Albedo("Brick", Brick);
            Normal("Brick_N", BrickHeight, 5f);
            Albedo("Wood", Wood);
            Albedo("Metal", BrushedMetal);
            Normal("Metal_N", (u, v) => Noise(u * 2, v * 160, 2, 160, 21) * 0.5f, 1.0f);
            Albedo("Grass", Grass);
            Normal("Grass_N", (u, v) => Fbm(u, v, 32, 3, 31), 2f);
            Albedo("Bark", Bark);
            Normal("Bark_N", (u, v) => Noise(u * 24, v * 3, 24, 3, 41) * 0.7f + Fbm(u, v, 16, 2, 42) * 0.3f, 6f);
            Albedo("Leaves", Leaves);
            Normal("Leaves_N", (u, v) => Fbm(u, v, 16, 3, 51), 3f);
            Albedo("StonePath", StonePath);
            Normal("StonePath_N", StonePathHeight, 6f);
            Albedo("Hazard", Hazard);
            Albedo("Belt", Belt);
            Normal("Belt_N", (u, v) => Frac(u * 16f) < 0.15f ? 1f : 0f, 3f);
            Albedo("TreadPlate", (u, v) => Gray(0.55f + 0.08f * Fbm(u, v, 8, 4, 61)));
            Normal("TreadPlate_N", TreadHeight, 6f);
            Albedo("Crate", Crate);
            Normal("Crate_N", CrateHeight, 4f);
            Albedo("Fabric", Fabric);
            Albedo("Water", Water);
            Normal("Water_N", (u, v) => Fbm(u, v, 6, 4, 71), 2.5f);
            Albedo("Globe", Globe, 1024);
            Albedo("ServerFront", ServerFront);
            Albedo("ServerFront_E", ServerLeds);
            Albedo("SoftDot", (u, v) =>
            {
                float d = Mathf.Clamp01(1f - new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f);
                return new Color(1f, 1f, 1f, d * d);
            }, 64, true);

            AssetDatabase.Refresh();
            var result = new Dictionary<string, Texture2D>();
            foreach (var (name, normal, alpha) in made)
            {
                string path = $"{Folder}/{name}.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.anisoLevel = 8;
                importer.mipmapEnabled = true;
                importer.alphaIsTransparency = alpha;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
                result[name] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return result;
        }

        // ---------------------------------------------------------------- painters

        static Color FloorTiles(float u, float v)
        {
            const int n = 4;
            int cx = (int)(u * n), cy = (int)(v * n);
            float fx = Frac(u * n), fy = Frac(v * n);
            const float grout = 0.025f;
            if (fx < grout || fx > 1 - grout || fy < grout || fy > 1 - grout) return new Color(0.30f, 0.30f, 0.31f);
            float shade = 0.60f + (Hash(cx, cy, 1) - 0.5f) * 0.07f + (Fbm(u, v, 16, 4, 2) - 0.5f) * 0.08f;
            return new Color(shade * 0.97f, shade * 0.99f, shade * 1.02f);
        }

        static float FloorTilesHeight(float u, float v)
        {
            float fx = Frac(u * 4), fy = Frac(v * 4);
            float edge = Mathf.Min(Mathf.Min(fx, 1 - fx), Mathf.Min(fy, 1 - fy));
            return Mathf.Clamp01((edge - 0.02f) / 0.03f) + Fbm(u, v, 32, 2, 3) * 0.05f;
        }

        static Color Concrete(float u, float v)
        {
            float n = 0.88f + 0.16f * (Fbm(u, v, 8, 5, 11) - 0.5f) * 2f;
            float speck = Hash((int)(u * Size), (int)(v * Size), 12) < 0.02f ? 0.85f : 1f;
            float k = n * speck;
            return new Color(0.80f * k, 0.79f * k, 0.76f * k);
        }

        static void BrickCell(float u, float v, out int row, out int col, out float fx, out float fy)
        {
            const int rows = 8, cols = 4;
            row = (int)(v * rows);
            float shifted = u * cols + (row % 2) * 0.5f;
            col = (int)Mathf.Repeat(Mathf.Floor(shifted), cols);
            fx = Frac(shifted);
            fy = Frac(v * rows);
        }

        static Color Brick(float u, float v)
        {
            BrickCell(u, v, out int row, out int col, out float fx, out float fy);
            if (fx < 0.035f || fy < 0.08f)
            {
                float m = 0.70f + 0.05f * Fbm(u, v, 32, 2, 13);
                return new Color(m, m * 0.97f, m * 0.92f);
            }
            float h = Hash(col, row, 14);
            float n = 0.85f + 0.3f * Fbm(u, v, 32, 4, 15);
            var baseColor = Color.Lerp(new Color(0.55f, 0.22f, 0.14f), new Color(0.68f, 0.33f, 0.20f), h);
            if (Hash(col, row, 16) > 0.85f) baseColor *= 0.75f;
            return baseColor * n;
        }

        static float BrickHeight(float u, float v)
        {
            BrickCell(u, v, out _, out _, out float fx, out float fy);
            float edge = Mathf.Min(Mathf.Clamp01((fx - 0.035f) / 0.03f), Mathf.Clamp01((fy - 0.08f) / 0.06f));
            return edge * (0.9f + 0.1f * Fbm(u, v, 32, 3, 17));
        }

        static Color Wood(float u, float v)
        {
            float warp = Noise(u * 4, v * 24, 4, 24, 18) * 2.5f;
            float grain = 0.5f + 0.5f * Mathf.Sin((v * 30f + warp) * Mathf.PI * 2f);
            float fine = Noise(u * 8, v * 256, 8, 256, 19);
            float plank = Frac(v * 4f) < 0.012f ? 0.55f : 1f;
            var c = Color.Lerp(new Color(0.50f, 0.32f, 0.17f), new Color(0.76f, 0.56f, 0.34f), grain * 0.7f + fine * 0.3f);
            return c * plank;
        }

        static Color BrushedMetal(float u, float v)
        {
            float streak = Noise(u * 2, v * 160, 2, 160, 21);
            float fine = Hash((int)(u * Size), (int)(v * Size), 22);
            return Gray(0.58f + 0.10f * streak + 0.04f * fine);
        }

        static Color Grass(float u, float v)
        {
            float patch = Fbm(u, v, 4, 4, 31);
            var c = Color.Lerp(new Color(0.18f, 0.34f, 0.10f), new Color(0.32f, 0.50f, 0.17f), patch);
            float blade = Hash((int)(u * Size), (int)(v * Size), 32);
            if (blade > 0.93f) c *= 1.25f;
            else if (blade < 0.07f) c *= 0.7f;
            return c * (0.9f + 0.2f * Fbm(u, v, 64, 2, 33));
        }

        static Color Bark(float u, float v)
        {
            float groove = Noise(u * 24, v * 3, 24, 3, 41);
            float n = Fbm(u, v, 16, 3, 42);
            return Color.Lerp(new Color(0.16f, 0.11f, 0.07f), new Color(0.38f, 0.27f, 0.18f), groove * 0.7f + n * 0.3f);
        }

        static Color Leaves(float u, float v)
        {
            // Clusters of small leaves: light tops, darker gaps, no hard-edged blotches.
            float n = Fbm(u, v, 16, 4, 51);
            float t = Fbm(u, v, 32, 2, 52);
            var leaf = Color.Lerp(new Color(0.16f, 0.36f, 0.11f), new Color(0.32f, 0.54f, 0.18f), t);
            return Color.Lerp(new Color(0.09f, 0.22f, 0.07f), leaf, Mathf.SmoothStep(0.25f, 0.5f, n));
        }

        static void Voronoi(float u, float v, int cells, int seed, out float f1, out float f2, out int id)
        {
            f1 = f2 = 10f;
            id = 0;
            int cx = (int)(u * cells), cy = (int)(v * cells);
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                int gx = cx + dx, gy = cy + dy;
                int wx = Mod(gx, cells), wy = Mod(gy, cells);
                float px = (gx + 0.15f + 0.7f * Hash(wx, wy, seed)) / cells;
                float py = (gy + 0.15f + 0.7f * Hash(wx, wy, seed + 1)) / cells;
                float d = new Vector2(u - px, v - py).magnitude;
                if (d < f1) { f2 = f1; f1 = d; id = wx * 131 + wy; }
                else if (d < f2) f2 = d;
            }
        }

        static Color StonePath(float u, float v)
        {
            Voronoi(u, v, 5, 61, out float f1, out float f2, out int id);
            if (f2 - f1 < 0.018f) return new Color(0.22f, 0.21f, 0.19f);
            float shade = 0.56f + 0.16f * Hash(id, 0, 62) + 0.1f * (Fbm(u, v, 16, 3, 63) - 0.5f);
            return new Color(shade, shade * 0.97f, shade * 0.9f);
        }

        static float StonePathHeight(float u, float v)
        {
            Voronoi(u, v, 5, 61, out float f1, out float f2, out _);
            return Mathf.Clamp01((f2 - f1 - 0.012f) / 0.04f) + Fbm(u, v, 16, 3, 63) * 0.1f;
        }

        static Color Hazard(float u, float v)
        {
            bool yellow = Frac((u + v) * 4f) < 0.5f;
            var c = yellow ? new Color(0.95f, 0.74f, 0.07f) : new Color(0.07f, 0.07f, 0.07f);
            return c * (0.85f + 0.15f * Fbm(u, v, 16, 3, 64));
        }

        static Color Belt(float u, float v)
        {
            float ridge = Frac(u * 16f) < 0.15f ? 0.16f : 0.09f;
            return Gray(ridge + 0.03f * Fbm(u, v, 32, 3, 65));
        }

        static float TreadHeight(float u, float v)
        {
            const int n = 8;
            int cx = (int)(u * n), cy = (int)(v * n);
            float fx = Frac(u * n) - 0.5f, fy = Frac(v * n) - 0.5f;
            float angle = ((cx + cy) % 2 == 0 ? 45f : -45f) * Mathf.Deg2Rad;
            float a = fx * Mathf.Cos(angle) + fy * Mathf.Sin(angle);
            float b = -fx * Mathf.Sin(angle) + fy * Mathf.Cos(angle);
            return Mathf.Clamp01(1f - (a * a / (0.32f * 0.32f) + b * b / (0.07f * 0.07f)));
        }

        static Color Crate(float u, float v)
        {
            float border = Mathf.Min(Mathf.Min(u, 1 - u), Mathf.Min(v, 1 - v));
            bool frame = border < 0.11f;
            bool brace = Mathf.Abs(u - v) < 0.07f;
            Color c = Wood(u * 0.5f, v * 0.5f);
            if (frame || brace) c *= 0.78f;
            else if (Frac(v * 4f) < 0.02f) c *= 0.6f;
            return c;
        }

        static float CrateHeight(float u, float v)
        {
            float border = Mathf.Min(Mathf.Min(u, 1 - u), Mathf.Min(v, 1 - v));
            if (border < 0.11f || Mathf.Abs(u - v) < 0.07f) return 1f;
            return Frac(v * 4f) < 0.02f ? 0f : 0.6f;
        }

        static Color Fabric(float u, float v)
        {
            float weave = 0.5f + 0.25f * Mathf.Sin(u * Size * 1.2f) * Mathf.Sin(v * Size * 1.2f);
            var c = new Color(0.12f, 0.17f, 0.28f);
            return c * (0.85f + 0.3f * weave * Fbm(u, v, 32, 2, 66) + 0.1f);
        }

        static Color Water(float u, float v)
        {
            float n = Fbm(u, v, 6, 4, 71);
            float caustic = Mathf.Pow(1f - Mathf.Abs(Fbm(u, v, 12, 3, 72) * 2f - 1f), 6f);
            return Color.Lerp(new Color(0.07f, 0.26f, 0.34f), new Color(0.16f, 0.45f, 0.52f), n) + new Color(0.2f, 0.3f, 0.3f) * caustic;
        }

        static Color Globe(float u, float v)
        {
            float lon = Frac(u * 24f), lat = Frac(v * 12f);
            bool grid = lon < 0.04f || lat < 0.04f;
            float land = Fbm(u, v * 0.5f, 6, 5, 81);
            float c = 0f;
            if (land > 0.53f) c = 0.45f;
            if (Mathf.Abs(land - 0.53f) < 0.012f) c = 1f;
            if (grid) c = Mathf.Max(c, 0.6f);
            return new Color(0.25f * c, 0.85f * c, 1f * c);
        }

        static void ServerCell(float u, float v, out int row, out float fx, out float fy)
        {
            row = (int)(v * 16);
            fx = u;
            fy = Frac(v * 16);
        }

        static Color ServerFront(float u, float v)
        {
            ServerCell(u, v, out int row, out float fx, out float fy);
            if (fy < 0.06f) return Gray(0.25f);
            bool vent = fx > 0.35f && fx < 0.95f && Frac(fx * 40f) < 0.5f && fy > 0.3f && fy < 0.7f;
            if (vent) return Gray(0.02f);
            return Lit(u, v, row) ? Leds(u, v, row) : Gray(0.08f + 0.02f * Hash(row, 0, 91));
        }

        static Color ServerLeds(float u, float v)
        {
            ServerCell(u, v, out int row, out _, out _);
            return Lit(u, v, row) ? Leds(u, v, row) : Color.black;
        }

        static bool Lit(float u, float v, int row)
        {
            float fy = Frac(v * 16);
            int slot = (int)(u * 20);
            return u < 0.3f && Frac(u * 20) > 0.3f && fy > 0.4f && fy < 0.6f && Hash(slot, row, 92) > 0.35f;
        }

        static Color Leds(float u, float v, int row)
        {
            float h = Hash((int)(u * 20), row, 93);
            return h < 0.6f ? new Color(0.2f, 1f, 0.35f) : h < 0.85f ? new Color(1f, 0.65f, 0.1f) : new Color(0.25f, 0.6f, 1f);
        }

        // ---------------------------------------------------------------- noise helpers

        static float Frac(float x) => x - Mathf.Floor(x);
        static int Mod(int a, int m) => ((a % m) + m) % m;
        static Color Gray(float g) => new Color(g, g, g);

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }

        /// <summary>Value noise that repeats every (periodX, periodY) lattice cells.</summary>
        static float Noise(float x, float y, int periodX, int periodY, int seed)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float fx = x - xi, fy = y - yi;
            int x0 = Mod(xi, periodX), x1 = Mod(xi + 1, periodX);
            int y0 = Mod(yi, periodY), y1 = Mod(yi + 1, periodY);
            float sx = fx * fx * (3 - 2 * fx), sy = fy * fy * (3 - 2 * fy);
            float a = Mathf.Lerp(Hash(x0, y0, seed), Hash(x1, y0, seed), sx);
            float b = Mathf.Lerp(Hash(x0, y1, seed), Hash(x1, y1, seed), sx);
            return Mathf.Lerp(a, b, sy);
        }

        /// <summary>Tileable fractal noise in [0,1] over the unit square.</summary>
        static float Fbm(float u, float v, int basePeriod, int octaves, int seed)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            int period = basePeriod;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * Noise(u * period, v * period, period, period, seed + o * 17);
                norm += amp;
                amp *= 0.5f;
                period *= 2;
            }
            return sum / norm;
        }

        // ---------------------------------------------------------------- output

        static Texture2D Paint(int w, int h, Func<float, float, Color> paint)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = paint((x + 0.5f) / w, (y + 0.5f) / h);
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static Texture2D NormalFromHeight(int size, Func<float, float, float> height, float strength)
        {
            var hgt = new float[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                hgt[y * size + x] = height((x + 0.5f) / size, (y + 0.5f) / size);

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float l = hgt[y * size + Mod(x - 1, size)], r = hgt[y * size + Mod(x + 1, size)];
                float d = hgt[Mod(y - 1, size) * size + x], u = hgt[Mod(y + 1, size) * size + x];
                var n = new Vector3((l - r) * strength, (d - u) * strength, 1f).normalized;
                px[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static void Save(string name, Texture2D tex)
        {
            File.WriteAllBytes($"{Folder}/{name}.png", tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
