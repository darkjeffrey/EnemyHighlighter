using System;
using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnemyHighlighter
{
    /// <summary>The keys offered in the "Highlight key" list: every keyboard key and the extra mouse buttons.</summary>
    public static class KeyChoices
    {
        public const InputKey DefaultKey = InputKey.LeftAlt;

        public static readonly List<string> Names = Build();

        public static int DefaultIndex => Math.Max(0, Names.IndexOf(DefaultKey.ToString()));

        public static InputKey Parse(string name)
        {
            if (!string.IsNullOrEmpty(name) && Enum.TryParse(name, out InputKey key) && key != InputKey.Invalid)
                return key;
            return DefaultKey;
        }

        private static List<string> Build()
        {
            var names = new List<string>();
            foreach (InputKey key in Enum.GetValues(typeof(InputKey)))
            {
                string name = key.ToString();
                // Left/right click would fire all the time, Escape opens the menu, the wheel and controller
                // inputs don't work as a held key.
                if (key == InputKey.Invalid || key == InputKey.Escape || key == InputKey.Extended
                    || key == InputKey.LeftMouseButton || key == InputKey.RightMouseButton
                    || key == InputKey.MouseScrollUp || key == InputKey.MouseScrollDown
                    || name.StartsWith("Controller", StringComparison.Ordinal))
                    continue;
                if (!names.Contains(name)) names.Add(name);
            }
            return names;
        }
    }

    /// <summary>An outline colour in the "Outline colour" list. The list shows ToString(), so it is translated.</summary>
    public sealed class ColorChoice
    {
        private readonly string _text;
        public uint Value { get; }

        private ColorChoice(string text, float r, float g, float b)
        {
            _text = text;
            Value = new Color(r, g, b, 1f).ToUnsignedInteger();
        }

        public override string ToString() => LocalizedText.Resolve(_text);

        public static readonly List<ColorChoice> All = new List<ColorChoice>
        {
            new ColorChoice("{=ehl_color_red}Red", 1f, 0.12f, 0.12f),
            new ColorChoice("{=ehl_color_orange}Orange", 1f, 0.5f, 0f),
            new ColorChoice("{=ehl_color_yellow}Yellow", 1f, 0.9f, 0.1f),
            new ColorChoice("{=ehl_color_magenta}Magenta", 1f, 0.2f, 0.9f),
            new ColorChoice("{=ehl_color_cyan}Cyan", 0.1f, 0.9f, 1f),
            new ColorChoice("{=ehl_color_white}White", 1f, 1f, 1f),
            // Okabe-Ito colours, made to stay distinct for colour-blind players.
            new ColorChoice("{=ehl_color_cb_orange}Colour-blind orange", 0.9f, 0.62f, 0f),
            new ColorChoice("{=ehl_color_cb_blue}Colour-blind sky blue", 0.34f, 0.71f, 0.91f),
        };
    }

    internal static class LocalizedText
    {
        /// <summary>
        /// Resolves "{=id}Text". If the game's text system isn't ready yet, TextObject hands the raw "{=id}Text"
        /// back; drop the id then so the English still shows.
        /// </summary>
        public static string Resolve(string text)
        {
            string resolved;
            try { resolved = new TextObject(text).ToString(); }
            catch (Exception) { resolved = text; }
            if (resolved != null && resolved.StartsWith("{=", StringComparison.Ordinal))
            {
                int close = resolved.IndexOf('}');
                if (close >= 0) resolved = resolved.Substring(close + 1);
            }
            return resolved;
        }
    }
}
