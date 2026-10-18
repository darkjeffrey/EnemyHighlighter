using System;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace EnemyHighlighter
{
    internal static class Log
    {
        /// <summary>Message-log line, only when "Debug messages" is on. Debug text isn't translated.</summary>
        public static void Debug(string text)
        {
            if (!EhSettings.DebugMessages) return;
            InformationManager.DisplayMessage(new InformationMessage("[EnemyHighlighter] " + text, Colors.Gray));
        }

        public static void Error(string where, Exception e)
        {
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=ehl_err_generic}[EnemyHighlighter] error ({WHERE}): {ERROR}")
                    .SetTextVariable("WHERE", where + TopFrame(e))
                    .SetTextVariable("ERROR", e.Message)
                    .ToString(),
                Colors.Red));
        }

        /// <summary>The first line of the stack trace, e.g. "at EnemyHighlighter.X.Y()", so an error report says where it broke.</summary>
        private static string TopFrame(Exception e)
        {
            string trace = e?.StackTrace;
            if (string.IsNullOrEmpty(trace)) return "";
            int end = trace.IndexOf('\n');
            return " " + (end >= 0 ? trace.Substring(0, end) : trace).Trim();
        }

        public static void Info(TextObject text, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(text.ToString(), color));
        }
    }
}
