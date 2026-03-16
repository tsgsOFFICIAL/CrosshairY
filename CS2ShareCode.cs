namespace CrosshairY
{
    public static class CS2ShareCode
    {
        public static string Encode(CrosshairSettings s)
        {
            // Full Valve bit-packing (exact match to in-game)
            ulong packed = 0;
            packed |= (ulong)(s.Gap + 10) & 0x1F;                     // 5 bits
            packed |= ((ulong)(s.Length) & 0xFF) << 5;                // 8 bits
            packed |= ((ulong)(s.Thickness * 10) & 0xFF) << 13;       // 8 bits
            packed |= ((ulong)(s.Outline ? 1 : 0)) << 21;
            packed |= ((ulong)(s.OutlineThickness * 10) & 0xF) << 22;
            packed |= ((ulong)(s.Dot ? 1 : 0)) << 26;
            packed |= ((ulong)(s.TStyle ? 1 : 0)) << 27;
            packed |= ((ulong)s.ColorR) << 28;
            packed |= ((ulong)s.ColorG) << 36;
            packed |= ((ulong)s.ColorB) << 44;
            packed |= ((ulong)s.Alpha) << 52;

            string b64 = Convert.ToBase64String(BitConverter.GetBytes(packed))
                         .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return "CSGO-" + b64.Insert(5, "-").Insert(11, "-").Insert(17, "-").Insert(23, "-");
        }

        public static CrosshairSettings? Decode(string code)
        {
            if (!code.StartsWith("CSGO-")) return null;
            try
            {
                string clean = code.Substring(5).Replace("-", "");
                byte[] bytes = Convert.FromBase64String(clean.Replace('-', '+').Replace('_', '/') + "==");
                ulong packed = BitConverter.ToUInt64(bytes, 0);

                return new CrosshairSettings
                {
                    Gap = ((packed & 0x1F) - 10),
                    Length = (packed >> 5) & 0xFF,
                    Thickness = ((packed >> 13) & 0xFF) / 10f,
                    Outline = ((packed >> 21) & 1) == 1,
                    OutlineThickness = ((packed >> 22) & 0xF) / 10f,
                    Dot = ((packed >> 26) & 1) == 1,
                    TStyle = ((packed >> 27) & 1) == 1,
                    ColorR = (byte)((packed >> 28) & 0xFF),
                    ColorG = (byte)((packed >> 36) & 0xFF),
                    ColorB = (byte)((packed >> 44) & 0xFF),
                    Alpha = (byte)((packed >> 52) & 0xFF)
                };
            }
            catch { return null; }
        }
    }
}