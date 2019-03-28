using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Java.Security;

namespace BitCWallet
{
    public class Encryption
    {

        public int CreateRandom_Key()
        {
            var reA = Encoding.ASCII.GetBytes("PMABitcoinWallet");
            var nA = BytesToInt(ref reA, 0, reA.Length);
            return Math.Abs(KeyEncryption(nA, "3227d1573b8c5f6d2a7e361218c31b4c"));//Any 32 byte 
        }

        const string key = "5252a2561c2f1f52a2b7e361218c31b4c";
        int KeyBc;
        public bool success { get; set; }
        private string exception { get; set; }
        public AEScustom AES;
        public Encryption() { }
        public Encryption(string mtext) : this(mtext, key) { }

        internal static byte[] CreateRandomSalt()
        {
            var random = SecureRandom.GetInstance("SHA256PRNG");
            var salt = new byte[122];
            random.NextBytes(salt);
            return salt;
        }
        internal Encryption(string msg, string enckey)
        {
            string s = string.Empty;
            try
            {
                AES = new AEScustom();
                byte[] plain, enc, iv;
                plain = StringToBytes(ref msg);
                iv = GetRandomBytes(16);
                if (AES == null) return;
                AES.Key = Hex2Bytes(enckey);
                AES.Mode = CipherMode.CBC;
                AES.Padding = PaddingMode.PKCS7;
                AES.IV = iv;
                enc = AES.Encrypt(plain);
                s = ToHex(ref iv, 0, 16, string.Empty) + ToHex(ref enc, 0, enc.Length, string.Empty); success = true;
                if (success) MainActivity.Preferences.Edit().PutString("token", s).Apply();
            }
            catch (Exception e) when (e is Exception || e is CryptographicUnexpectedOperationException)
            {
                success = false;
                exception = e.Message;
                return;
            }

        }
        public static string Decrypt(string cipherStr)
        {
            if (string.IsNullOrEmpty(cipherStr)) return "";
            AEScustom AES = new AEScustom
            {
                Mode =
                CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = Hex2Bytes(key),
                IV = new byte[16]
            };

            var bytes = Hex2Bytes(cipherStr);
            Array.Copy(bytes, 0, AES.IV, 0, 16);
            byte[] cipher = new byte[bytes.Length - 16];
            Array.Copy(bytes, 16, cipher, 0, bytes.Length - 16);
            var Decrypted = AES.Decrypt(cipher);
            return BytesToString(ref Decrypted).Replace("\0", string.Empty);
        }

        internal int KeyEncryption(int msg, string enckey)
        {
            try
            {
                byte[] plain, enc, iv;
                plain = IntToBytes(msg);
                iv = GetRandomBytes(16);

                if (AES == null)
                    AES = new AEScustom();

                AES.Key = Hex2Bytes(enckey);
                AES.Mode = CipherMode.CBC;
                AES.Padding = PaddingMode.PKCS7;
                AES.IV = iv;
                enc = AES.Encrypt(plain);
                KeyBc = ToHexInt(ref iv, 0, 16, string.Empty) + ToHexInt(ref enc, 0, enc.Length, string.Empty);
                return KeyBc;
            }
            catch (Exception e) when (e is Exception || e is CryptographicUnexpectedOperationException)
            {
                return 0;
            }
        }

        public byte[] GetRandomBytes(int size) //CRPRNG
        {
            var crypto = new RNGCryptoServiceProvider();
            byte[] data = new byte[size];
            crypto.GetNonZeroBytes(data);
            return data;
        }

        public byte[] IntToBytes(int i, bool bigendian = true)
        {
            byte[] ba = BitConverter.GetBytes(i);
            if (bigendian) Array.Reverse(ba);
            return ba;
        }

        internal int BytesToInt(ref byte[] array, int start, int count, bool bigendian = true)
        {
            if (start + count > array.Length) return 0;
            int res;
            byte[] ba = new byte[array.Length];
            Array.Copy(array, start, ba, 0, count);
            if (bigendian) Array.Reverse(ba);
            try { res = BitConverter.ToInt32(ba, 0); } catch (Exception e) { return 0; }
            return BitConverter.ToInt32(ba, 0);
        }

        public class AEScustom //Advanced Encryption Standard (AES)
        {

            RijndaelManaged AES = new RijndaelManaged();
            ICryptoTransform encryptor;
            ICryptoTransform decryptor;
            MemoryStream memStream;
            CryptoStream crStream;
            public byte[] Key { get; set; }
            public byte[] IV { get; set; }
            public CipherMode Mode { get; set; }
            public PaddingMode Padding { get; set; }
            public byte[] Encrypt(byte[] plain)
            {
                return Encrypt(plain, 0, plain.Length);
            }

            public byte[] Encrypt(byte[] plain, int start, int length)
            {

                AES.Mode = Mode;
                AES.Padding = Padding;
                encryptor = AES.CreateEncryptor(Key, IV);
                memStream = new MemoryStream();
                using (crStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                {
                    crStream.Write(plain, start, length);
                    crStream.FlushFinalBlock();
                }
                byte[] enc = memStream.ToArray();
                memStream.Close();
                crStream.Close();
                return enc;
            }

            public byte[] Decrypt(byte[] ciphertext)
            {

                AES.Mode = Mode;
                AES.Padding = Padding;
                decryptor = AES.CreateDecryptor(Key, IV);
                memStream = new MemoryStream(ciphertext);
                crStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
                byte[] dec = new byte[ciphertext.Length - 1 + 1];
                crStream.Read(dec, 0, dec.Length);
                memStream.Close();
                crStream.Close();
                return dec;
            }

            public byte[] Decrypt(byte[] ciphertext, byte[] Keym, byte[] IVm)
            {

                AES.Mode = Mode;
                AES.Padding = Padding;
                decryptor = AES.CreateDecryptor(Keym, IVm);
                memStream = new MemoryStream(ciphertext);
                crStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
                byte[] dec = new byte[ciphertext.Length - 1 + 1];
                crStream.Read(dec, 0, dec.Length);
                memStream.Close();
                crStream.Close();
                return dec;
            }
        }

        internal static string BytesToString(ref byte[] ba, int start, int count)
        {
            int i;

            if (start < 0) return "";
            if (start >= ba.Length) return "";
            if (start + count > ba.Length) count = ba.Length - start;

            string s = "";
            for (i = start; i <= start + count - 1; i++) { s += (char)(ba[i]); }
            return s;
        }

        internal static string BytesToString(ref byte[] ba)
        {
            return BytesToString(ref ba, 0, ba.Length);
        }

        internal static byte[] StringToBytes(ref string s)
        {
            return new UTF8Encoding().GetBytes(s);
        }

        public static class crypto  //general crypto functions
        {
            public static string GetCryptoString(int size)
            {
                char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
                var crypto = new RNGCryptoServiceProvider();
                byte[] data = new byte[size + 1];
                string s = string.Empty;

                crypto.GetNonZeroBytes(data);
                foreach (byte b in data)
                    s = s + chars[b % chars.Length];

                return s;

            }

            public static byte[] GetRandomBytes(int size)
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                byte[] data = new byte[size];
                crypto.GetNonZeroBytes(data);
                return data;
            }
        }

        internal static bool HexToByte(string hex, out byte result)
        {
            return byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out result);
        }

        internal static byte[] Hex2Bytes(string hex)
        {
            Hex2Bytes(hex, out byte[] res);
            return res;
        }

        internal static bool Hex2Bytes(string hex, out byte[] res)
        {
            hex = hex.Replace(" ", "").Replace("-", "").Trim();
            res = new byte[hex.Length / 2];

            for (int i = 0; i < res.Length; i++)
            {
                if (!HexToByte(hex.Substring(i * 2, 2), out byte b)) return false;
                res[i] = b;
            }
            return true;
        }

        internal string ToHex(ref byte[] array, int start, int len, string sep)
        {
            return BitConverter.ToString(array, start, len).Replace("-", sep);
        }

        internal int ToHexInt(ref byte[] array, int start, int len, string sep)
        {
            return BitConverter.ToInt16(array, start);
        }

        public string ArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }


        public class XTEA
        {

            private uint[] xteakey = new uint[4];
            /// <summary>
            /// The delta is derived from the golden ratio where delta = (sqrt(2) - 1) * 2^31
            /// A different multiple of delta is used in each round so that no bit of
            /// the multiple will not change frequently
            /// </summary>
            private const uint delta = 2654435769;

            public XTEA() { }
            public XTEA(uint k0, uint k1, uint k2, uint k3)
            {
                SetKey(k0, k1, k2, k3);
                byte[] array = new byte[4] { (byte)k0, (byte)k1, (byte)k2, (byte)k3 };
                Encode(ref array, 0, array.Length);
            }

            internal void SetKey(uint k0, uint k1, uint k2, uint k3)
            {
                xteakey[0] = k0;
                xteakey[1] = k1;
                xteakey[2] = k2;
                xteakey[3] = k3;

            }
            internal void SetKey(ref byte[] key, Int32 startindex)
            {
                xteakey[0] = BA2Uint(ref key, startindex);
                xteakey[1] = BA2Uint(ref key, startindex + 4);
                xteakey[2] = BA2Uint(ref key, startindex + 8);
                xteakey[3] = BA2Uint(ref key, startindex + 12);

            }
            private uint BA2Uint(ref byte[] ba, int i)
            {
                int t, r;

                r = 0x100 * ba[i + 1];
                r = ba[i] + (0x100 * ba[i + 1]) + (0x10000 * ba[i + 2]);
                t = ba[i + 3];
                t = (t * 0x1000000);
                r += t;
                return (uint)r;

            }
            internal void Encode(ref byte[] ba, int start, int count)
            {
                uint v0, v1;
                uint[] result;
                int i;

                for (i = start; i <= (start + count - 1); i += 8)
                {
                    v0 = BA2Uint(ref ba, i);
                    v1 = BA2Uint(ref ba, i + 4);
                    result = EncodeLong(v0, v1);
                    ba[i] = (byte)(result[0] & 0xff);
                    ba[i + 1] = (byte)((result[0] & 0xff00) / 0x100);
                    ba[i + 2] = (byte)((result[0] & 0xff0000) / 0x10000);
                    ba[i + 3] = (byte)((result[0] & 0xff000000) / 0x1000000);
                    ba[i + 4] = (byte)(result[1] & 0xff);
                    ba[i + 5] = (byte)((result[1] & 0xff00) / 0x100);
                    ba[i + 6] = (byte)((result[1] & 0xff0000) / 0x10000);
                    ba[i + 7] = (byte)((result[1] & 0xff000000) / 0x1000000);
                }

            }

            internal void Decode(ref byte[] ba)
            {
                uint v0, v1;
                uint[] result;
                int i;

                for (i = 0; i <= ba.Length - 1; i += 8)
                {
                    v0 = BA2Uint(ref ba, i);
                    v1 = BA2Uint(ref ba, i + 4);
                    result = DecodeLong(v0, v1);
                    ba[i] = (byte)(result[0] & 0xff);
                    ba[i + 1] = (byte)((result[0] & 0xff00) / 0x100);
                    ba[i + 2] = (byte)((result[0] & 0xff0000) / 0x10000);
                    ba[i + 3] = (byte)((result[0] & 0xff000000) / 0x1000000);
                    ba[i + 4] = (byte)(result[1] & 0xff);
                    ba[i + 5] = (byte)((result[1] & 0xff00) / 0x100);
                    ba[i + 6] = (byte)((result[1] & 0xff0000) / 0x10000);
                    ba[i + 7] = (byte)((result[1] & 0xff000000) / 0x1000000);
                }

            }
            internal uint[] EncodeLong(UInt32 v0, UInt32 v1)
            {

                uint r1, r2, sum;
                byte i;

                sum = 0;
                for (i = 1; i <= 32; i++)
                {
                    r1 = Add32(((v1 << 4) ^ (v1 >> 5)), v1);
                    r2 = Add32(sum, xteakey[sum & 3]);
                    v0 = Add32(v0, r1 ^ r2);

                    sum = Add32(sum, delta);

                    r1 = Add32(((v0 << 4) ^ (v0 >> 5)), v0);
                    r2 = Add32(sum, xteakey[(sum >> 11) & 3]);
                    v1 = Add32(v1, r1 ^ r2);
                }

                return new uint[] { v0, v1 };

            }
            internal uint[] DecodeLong(uint i0, uint i1)
            {

                uint sum = Mul32(delta, 32);
                byte i = 0;

                for (i = 1; i <= 32; i++)
                {
                    i1 = Sub32(i1, Add32(((i0 << 4) ^ (i0 >> 5)), i0) ^ Add32(sum, xteakey[(sum >> 11) & 3]));
                    sum = Sub32(sum, delta);
                    i0 = Sub32(i0, Add32(((i1 << 4) ^ (i1 >> 5)), i1) ^ Add32(sum, xteakey[sum & 3]));
                }

                return new uint[] { i0, i1 };

            }


            private uint Mul32(uint x, uint y)
            {

                ulong x64 = x;
                ulong y64 = y;

                return (uint)((x64 * y64) & uint.MaxValue);

            }
            private uint Sub32(uint x, uint y)
            {

                ulong x64 = (ulong)x + 0x100000000L;
                ulong y64 = y;

                return (uint)((x64 - y64) & uint.MaxValue);

            }
            private uint Add32(uint x, uint y)
            {

                ulong x64 = x;
                ulong y64 = y;

                return Convert.ToUInt32((x64 + y64) & UInt32.MaxValue);

            }

        }
    }
}