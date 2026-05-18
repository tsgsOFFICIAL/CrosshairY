using System.Text;
using System.IO;

namespace CrosshairY.Models
{
    public static class ShareCode
    {
        private const string Prefix = "TSGS-";
        private const byte Version = 4;

        private const string Alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"; // Custom Base57
        private static readonly int BaseN = Alphabet.Length;

        public static string Encode(CrosshairSettings crosshair)
        {
            using MemoryStream ms = new();
            using BinaryWriter bw = new(ms);

            bw.Write(Version);

            bw.Write(crosshair.Gap);
            bw.Write(crosshair.Length);
            bw.Write(crosshair.Thickness);
            bw.Write(crosshair.OutlineThickness);

            bw.Write(crosshair.Dot);
            bw.Write(crosshair.TStyle);
            bw.Write(crosshair.Outline);
            bw.Write(crosshair.SquareStyle);

            bw.Write(crosshair.ColorR);
            bw.Write(crosshair.ColorG);
            bw.Write(crosshair.ColorB);
            bw.Write(crosshair.Alpha);

            bw.Write(crosshair.OutlineColorR);
            bw.Write(crosshair.OutlineColorG);
            bw.Write(crosshair.OutlineColorB);
            bw.Write(crosshair.OutlineAlpha);

            WriteString(bw, crosshair.CrosshairName ?? "");
            WriteString(bw, crosshair.Description ?? "");

            byte[] data = ms.ToArray();

            string encoded = EncodeBaseN(data);

            List<string> chunks = new List<string>();
            for (int i = 0; i < encoded.Length; i += 5)
            {
                int len = Math.Min(5, encoded.Length - i);
                chunks.Add(encoded.Substring(i, len));
            }

            return Prefix + string.Join("-", chunks);
        }

        public static CrosshairSettings? Decode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !code.StartsWith(Prefix))
                return null;

            try
            {
                string raw = code[Prefix.Length..].Replace("-", "");

                byte[] data = DecodeBaseN(raw);

                using MemoryStream ms = new(data);
                using BinaryReader br = new(ms);

                byte version = br.ReadByte();

                return version switch
                {
                    1 => DecodeV1(br),
                    2 => DecodeV2(br),
                    3 => DecodeV3(br),
                    4 => DecodeV4(br),
                    _ => null,
                };
            }
            catch
            {
                return null;
            }
        }

        private static CrosshairSettings DecodeV1(BinaryReader br)
        {
            CrosshairSettings c = new CrosshairSettings
            {
                Gap = br.ReadSingle(),
                Length = br.ReadSingle(),
                Thickness = br.ReadSingle(),
                OutlineThickness = br.ReadSingle(),

                Dot = br.ReadBoolean(),
                TStyle = br.ReadBoolean(),
                Outline = br.ReadBoolean(),

                ColorR = br.ReadByte(),
                ColorG = br.ReadByte(),
                ColorB = br.ReadByte(),
                Alpha = br.ReadByte()
            };

            return c;
        }

        private static CrosshairSettings DecodeV2(BinaryReader br)
        {
            CrosshairSettings c = new CrosshairSettings
            {
                Gap = br.ReadSingle(),
                Length = br.ReadSingle(),
                Thickness = br.ReadSingle(),
                OutlineThickness = br.ReadSingle(),

                Dot = br.ReadBoolean(),
                TStyle = br.ReadBoolean(),
                Outline = br.ReadBoolean(),

                ColorR = br.ReadByte(),
                ColorG = br.ReadByte(),
                ColorB = br.ReadByte(),
                Alpha = br.ReadByte(),
                
                CrosshairName = ReadString(br),
                Description = ReadString(br)
            };

            return c;
        }

        private static CrosshairSettings DecodeV3(BinaryReader br)
        {
            CrosshairSettings c = new CrosshairSettings
            {
                Gap = br.ReadSingle(),
                Length = br.ReadSingle(),
                Thickness = br.ReadSingle(),
                OutlineThickness = br.ReadSingle(),

                Dot = br.ReadBoolean(),
                TStyle = br.ReadBoolean(),
                Outline = br.ReadBoolean(),
                SquareStyle = br.ReadBoolean(),

                ColorR = br.ReadByte(),
                ColorG = br.ReadByte(),
                ColorB = br.ReadByte(),
                Alpha = br.ReadByte(),

                CrosshairName = ReadString(br),
                Description = ReadString(br)
            };

            return c;
        }

        private static CrosshairSettings DecodeV4(BinaryReader br)
        {
            CrosshairSettings c = new CrosshairSettings
            {
                Gap = br.ReadSingle(),
                Length = br.ReadSingle(),
                Thickness = br.ReadSingle(),
                OutlineThickness = br.ReadSingle(),

                Dot = br.ReadBoolean(),
                TStyle = br.ReadBoolean(),
                Outline = br.ReadBoolean(),
                SquareStyle = br.ReadBoolean(),

                ColorR = br.ReadByte(),
                ColorG = br.ReadByte(),
                ColorB = br.ReadByte(),
                Alpha = br.ReadByte(),

                OutlineColorR = br.ReadByte(),
                OutlineColorG = br.ReadByte(),
                OutlineColorB = br.ReadByte(),
                OutlineAlpha = br.ReadByte(),

                CrosshairName = ReadString(br),
                Description = ReadString(br)
            };

            return c;
        }

        private static void WriteString(BinaryWriter bw, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            bw.Write((ushort)bytes.Length);   // max 65,535 chars
            bw.Write(bytes);
        }

        private static string ReadString(BinaryReader br)
        {
            ushort length = br.ReadUInt16();
            byte[] bytes = br.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        // ---------- Base-N Encoding ----------
        private static string EncodeBaseN(byte[] data)
        {
            List<int> digits = [0];

            for (int i = 0; i < data.Length; i++)
            {
                int carry = data[i];

                for (int j = 0; j < digits.Count; j++)
                {
                    int val = digits[j] * 256 + carry;
                    digits[j] = val % BaseN;
                    carry = val / BaseN;
                }

                while (carry > 0)
                {
                    digits.Add(carry % BaseN);
                    carry /= BaseN;
                }
            }

            StringBuilder sb = new StringBuilder();

            for (int i = digits.Count - 1; i >= 0; i--)
                sb.Append(Alphabet[digits[i]]);

            return sb.ToString();
        }

        private static byte[] DecodeBaseN(string input)
        {
            List<int> bytes = [0];

            for (int i = 0; i < input.Length; i++)
            {
                int carry = Alphabet.IndexOf(input[i]);
                if (carry < 0)
                    throw new Exception("Invalid character");

                for (int j = 0; j < bytes.Count; j++)
                {
                    int val = bytes[j] * BaseN + carry;
                    bytes[j] = val & 0xFF;
                    carry = val >> 8;
                }

                while (carry > 0)
                {
                    bytes.Add(carry & 0xFF);
                    carry >>= 8;
                }
            }

            bytes.Reverse();
            return [.. bytes.ConvertAll(b => (byte)b)];
        }
    }
}