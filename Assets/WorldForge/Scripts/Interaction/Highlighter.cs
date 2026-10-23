using System.Collections.Generic;
using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Shows which object is under the crosshair by temporarily swapping its materials for
    /// brighter copies (a glow in the object's own colour plus a hint of blue).
    /// Only one object is highlighted at a time.
    /// </summary>
    public static class Highlighter
    {
        static readonly Color Tint = new Color(0.20f, 0.55f, 1.00f);
        static readonly Dictionary<Material, Material> Variants = new Dictionary<Material, Material>();
        static readonly Dictionary<Renderer, Material[]> Swapped = new Dictionary<Renderer, Material[]>();

        public static void Apply(Component target)
        {
            if (target == null) return;
            foreach (var r in target.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || Swapped.ContainsKey(r)) continue;
                var original = r.sharedMaterials;
                var lit = new Material[original.Length];
                for (int i = 0; i < original.Length; i++) lit[i] = Variant(original[i]);
                Swapped[r] = original;
                r.sharedMaterials = lit;
            }
        }

        public static void ClearAll()
        {
            foreach (var pair in Swapped)
                if (pair.Key != null) pair.Key.sharedMaterials = pair.Value;
            Swapped.Clear();
        }

        static Material Variant(Material source)
        {
            if (source == null) return null;
            if (Variants.TryGetValue(source, out var variant)) return variant;

            variant = new Material(source) { name = source.name + " (highlight)" };
            var emission = source.HasProperty("_EmissionColor") && source.IsKeywordEnabled("_EMISSION")
                ? source.GetColor("_EmissionColor")
                : Color.black;
            variant.EnableKeyword("_EMISSION");
            var baseColor = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : Color.white;
            variant.SetColor("_EmissionColor", emission + baseColor * 0.3f + Tint * 0.05f);
            variant.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            Variants[source] = variant;
            return variant;
        }
    }
}
