using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace StockTimeFormatter
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class DTReplacer : MonoBehaviour
    {
        public static string GetUniqueStringFromUniqueNode(string name, string node)
        {
            var configs = GameDatabase.Instance.GetConfigs(node);
            if (configs.Length > 1)
            {
                Debug.LogError($"[StockDT] Multiple `{node}` configurations, falling back to default");
            }
            else if (configs.Length == 1)
            {
                ConfigNode config = configs[0].config;
                string[] formats = config.GetValues(name);
                if (formats.Length > 1)
                {
                    Debug.LogError($"[StockDT] `{node}` configuration has multiple `{name}` entries, falling back to default");
                }
                else if (formats.Length == 1)
                {
                    // Unescaping similar to that performed in KSP localization
                    // strings (because we need curly braces), though contrary
                    // to KSP we reject unpaired surrogates and instead accept
                    // codepoints outside the BMP with the \U syntax.
                    return new Regex(
                        @"\\(?:u([0-9A-F]{4})|U([0-9A-F]{5}))").Replace(
                        formats[0].Replace("｢", "{").Replace("｣", "}"),
                        match => char.ConvertFromUtf32(
                            int.Parse(
                                match.Value.Substring(2),
                                System.Globalization.NumberStyles.HexNumber)));
                }
            }
            return null;
        }

        public void Start()
        {
            string dateFormat = KSP.Localization.Localizer.GetStringByTag("#StockTimeFormatter_CLDR_mediumDate");
            string timeFormat = KSP.Localization.Localizer.GetStringByTag("#StockTimeFormatter_CLDR_mediumTime");
            string dateTimeFormat = KSP.Localization.Localizer.GetStringByTag("#StockTimeFormatter_CLDR_mediumDateTime");
            string customDateFormat = GetUniqueStringFromUniqueNode("dateFormat", "StockTimeFormatter");
            string customTimeFormat = GetUniqueStringFromUniqueNode("timeFormat", "StockTimeFormatter");
            string customDateTimeFormat = GetUniqueStringFromUniqueNode("dateTimeFormat", "StockTimeFormatter");
            // Validate any custom patterns.
            if (customDateTimeFormat != null)
            {
                try
                {
                    string.Format(customDateTimeFormat, "time", "date");
                    dateTimeFormat = customDateTimeFormat;
                }
                catch (FormatException)
                {
                    Debug.LogError("[StockDT] Invalid date and time combiner format " + customDateTimeFormat);
                }
            }

            double epoch = 0;
            string customEpoch = GetUniqueStringFromUniqueNode("epoch", "StockTimeFormatter");
            if (customEpoch != null)
            {
                Debug.LogError("[StockDT] Invalid epoch " + customEpoch);
            }

            KSPUtil.dateTimeFormatter = new StockDateTimeFormatter(epoch);
        }
    }
}
