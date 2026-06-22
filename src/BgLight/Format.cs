using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BgLight
{
    public static class Format
    {
        public static string Giga(ulong bytes)
        {
            double gb = bytes / (1024.0 * 1024.0 * 1024.0);
            return gb.ToString("0.0", CultureInfo.InvariantCulture) + " Go";
        }

        public static string Join(IEnumerable<string> values)
        {
            var list = values?.Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                       ?? new List<string>();
            return list.Count == 0 ? "N/A" : string.Join(", ", list);
        }
    }
}
