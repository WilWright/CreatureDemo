using System.Text;

namespace Utils
{
    public static class StringUtils
    {
        public static string ToFirstCharacterUpper(this string s, bool lowerRemaining = false)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var builder = new StringBuilder();

            builder.Append(char.ToUpper(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                builder.Append(lowerRemaining ? char.ToLower(s[i]) : s[i]);
            }

            return builder.ToString();
        }

        public static string ToFirstCharacterLower(this string s, bool upperRemaining = false)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var builder = new StringBuilder();

            builder.Append(char.ToLower(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                builder.Append(upperRemaining ? char.ToUpper(s[i]) : s[i]);
            }

            return builder.ToString();
        }
    }
}
