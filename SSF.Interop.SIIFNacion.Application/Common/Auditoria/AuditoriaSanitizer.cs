using System;
using System.Text.RegularExpressions;

namespace SSF.Interop.SIIFNacion.Application.Common.Auditoria
{
    internal static class AuditoriaSanitizer
    {
        private static readonly Regex AuthorizationBearer =
            new("(\"Authorization\"\\s*:\\s*\")Bearer\\s+([^\"]+)(\")", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PasswordField =
            new("(\"Password\"\\s*:\\s*\")([^\"]+)(\")", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string? Sanitize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var t = text;
            t = AuthorizationBearer.Replace(t, "$1Bearer ***$3");
            t = PasswordField.Replace(t, "$1***$3");
            return t;
        }

        public static string? Truncate(string? text, int maxChars)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (maxChars <= 0 || text.Length <= maxChars)
                return text;

            return text.Substring(0, maxChars) + $"...(truncado, len={text.Length})";
        }
    }
}

