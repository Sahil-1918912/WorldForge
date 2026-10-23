using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Immediate-mode HUD: title, crosshair, interaction prompt, controls panel, toasts and
    /// cinematic letterbox bars during the guided tour. Drawn at 1080p and scaled to the screen.
    /// </summary>
    public class HudOverlay : MonoBehaviour
    {
        public PlayerController player;
        public PlayerInteractor interactor;
        public CameraTour tour;
        public bool visible = true;
        public bool showHelp = true;

        string toast;
        float toastUntil;
        Texture2D pixel;
        GUIStyle title, body, small, prompt, center;

        static readonly Color Accent = new Color(0.35f, 0.85f, 1f);
        static readonly Color Panel = new Color(0.03f, 0.06f, 0.10f, 0.72f);

        static readonly (string key, string action)[] Controls =
        {
            ("WASD / Arrows", "Move"),
            ("Mouse", "Look around"),
            ("Shift", "Sprint"),
            ("Space", "Jump"),
            ("E / Left click", "Interact / pick up"),
            ("Right click", "Throw held object"),
            ("R (hold)", "Rotate held object"),
            ("Mouse wheel", "Held object distance"),
            ("V", "First / third person"),
            ("T", "Guided camera tour"),
            ("P", "Screenshot"),
            ("H", "Hide this panel"),
            ("U", "Hide all UI"),
            ("Esc", "Release mouse"),
        };

        public void ShowToast(string message)
        {
            toast = message;
            toastUntil = Time.unscaledTime + 2.2f;
        }

        void OnGUI()
        {
            if (!visible) return;
            EnsureStyles();

            float scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float w = Screen.width / scale, h = 1080f;

            if (tour != null && tour.Playing)
            {
                float bar = h * 0.08f;
                Fill(new Rect(0, 0, w, bar), Color.black);
                Fill(new Rect(0, h - bar, w, bar), Color.black);
                GUI.Label(new Rect(40, h - bar + 22, 900, 40), "<b>WORLDFORGE ROBOTICS LAB</b>   •   guided tour", small);
                GUI.Label(new Rect(w - 340, h - bar + 22, 300, 40), "T  exit tour", small);
                Fill(new Rect(0, h - bar - 3, w * tour.Progress, 3), Accent);
                DrawToast(w);
                return;
            }

            // Title card
            Fill(new Rect(24, 24, 470, 86), Panel);
            Fill(new Rect(24, 24, 5, 86), Accent);
            GUI.Label(new Rect(42, 32, 450, 40), "WORLDFORGE ROBOTICS LAB", title);
            string mode = player.ThirdPerson ? "Third-person" : "First-person";
            GUI.Label(new Rect(42, 72, 420, 30), $"{mode} camera  •  H for help", small);

            if (showHelp) DrawHelp(h);

            DrawCrosshair(w, h);
            DrawToast(w);

            if (Cursor.lockState != CursorLockMode.Locked && !Application.isBatchMode)
            {
                Fill(new Rect(w / 2 - 210, h / 2 + 70, 420, 50), Panel);
                GUI.Label(new Rect(w / 2 - 210, h / 2 + 70, 420, 50), "Click to look around", center);
            }
        }

        void DrawHelp(float h)
        {
            const float line = 27f;
            float height = 56f + Controls.Length * line;
            var rect = new Rect(24, h - 24 - height, 420, height);
            Fill(rect, Panel);
            GUI.Label(new Rect(rect.x + 18, rect.y + 12, 300, 30), "<b>CONTROLS</b>", body);
            for (int i = 0; i < Controls.Length; i++)
            {
                float y = rect.y + 46 + i * line;
                GUI.Label(new Rect(rect.x + 18, y, 170, line), $"<color=#8fdcff>{Controls[i].key}</color>", body);
                GUI.Label(new Rect(rect.x + 190, y, 220, line), Controls[i].action, body);
            }
        }

        void DrawCrosshair(float w, float h)
        {
            var hovered = interactor.Hovered;
            var held = interactor.Held;
            float size = hovered != null ? 16f : 6f;
            var c = hovered != null ? Accent : new Color(1f, 1f, 1f, 0.85f);

            if (hovered != null)
            {
                // Hollow square reticle when something can be used.
                float t = 2f;
                Fill(new Rect(w / 2 - size, h / 2 - size, size * 2, t), c);
                Fill(new Rect(w / 2 - size, h / 2 + size - t, size * 2, t), c);
                Fill(new Rect(w / 2 - size, h / 2 - size, t, size * 2), c);
                Fill(new Rect(w / 2 + size - t, h / 2 - size, t, size * 2), c);
            }
            else if (held == null)
            {
                Fill(new Rect(w / 2 - size / 2, h / 2 - size / 2, size, size), c);
            }

            string text = null;
            if (held != null)
                text = $"<b>{held.displayName}</b>\n<color=#8fdcff>[E]</color> Drop   <color=#8fdcff>[RMB]</color> Throw   <color=#8fdcff>[R]</color> Rotate   <color=#8fdcff>[Wheel]</color> Distance";
            else if (hovered != null)
                text = $"<b>{hovered.displayName}</b>\n<color=#8fdcff>[E]</color> {hovered.Prompt}";

            if (text != null)
            {
                var rect = new Rect(w / 2 - 330, h / 2 + 40, 660, 76);
                Fill(rect, Panel);
                GUI.Label(rect, text, prompt);
            }
        }

        void DrawToast(float w)
        {
            if (string.IsNullOrEmpty(toast) || Time.unscaledTime > toastUntil) return;
            float alpha = Mathf.Clamp01((toastUntil - Time.unscaledTime) / 0.4f);
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            var rect = new Rect(w / 2 - 300, 120, 600, 54);
            Fill(rect, Panel);
            GUI.Label(rect, toast, center);
            GUI.color = old;
        }

        void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = new Color(c.r, c.g, c.b, c.a * old.a);
            GUI.DrawTexture(r, pixel);
            GUI.color = old;
        }

        void EnsureStyles()
        {
            if (pixel != null) return;
            pixel = Texture2D.whiteTexture;
            title = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, richText = true, wordWrap = false };
            title.normal.textColor = Color.white;
            small = new GUIStyle(GUI.skin.label) { fontSize = 19, richText = true };
            small.normal.textColor = new Color(0.78f, 0.86f, 0.95f);
            body = new GUIStyle(GUI.skin.label) { fontSize = 20, richText = true, wordWrap = false };
            body.normal.textColor = new Color(0.9f, 0.94f, 1f);
            prompt = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, richText = true };
            prompt.normal.textColor = Color.white;
            center = new GUIStyle(prompt) { fontSize = 24 };
        }
    }
}
