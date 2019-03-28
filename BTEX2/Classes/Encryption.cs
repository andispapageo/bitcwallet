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
              
            }
            catch (Exception e) when (e is Exception || e is CryptographicUnexpectedOperationException)
            {
                success = false;
                exception = e.Message;
                return;
            }
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


        public class AEScustom //Advanced Encryption Standard 
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

        public static class nvo  //general crypto functions
        {
            public static byte[] GetRandomBytes(int size)
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                byte[] data = new byte[size];
                crypto.GetNonZeroBytes(data);
                return data;
            }
        }


        public string ArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }


        public class XTF
        {
            private uint[] xteakey = new uint[4];
            private const uint delta = 2654435769;

            public XTF() { }
            public XTF(uint k0, uint k1, uint k2, uint k3)
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
