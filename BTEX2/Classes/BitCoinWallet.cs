using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Android.Widget;
using NBitcoin;
using NBitcoin.Crypto;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using Nmnemonic = NBitcoin.Mnemonic;
namespace BitCWallet.Pages
{
    class BitCoinWallet
    {
        static string mWords { get; set; }
        public bool proceed { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string words { get { return MainActivity.Preferences.GetString("mnemonic", string.Empty); } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int getKeyNumber
        {
            get { return MainActivity.Preferences.GetInt("keynum", 0); }
        }

        int KeyRes { get; set; }
        public BitCoinWallet(string words)
        {
            KeyRes = getKeyNumber;
            if (KeyRes == 0 || string.IsNullOrWhiteSpace(words))
            {
                proceed = false;
                return;
            }
            else proceed = true;
        }

        public BitCoinWallet(bool adjustAddress, string Address) : this(Address)
        {
            if (proceed)
                StartAsync();
        }
        string res = MainActivity.Preferences.GetString("BitcoinPassphraseCode", "");

        private async void StartAsync()
        {
        
            var res = await GenerateAdd_HDWalletBIP44(words, "andis", KeyRes);
            if (res != null
                && res.Count() > 1
                && res.ToList()[0] != null
                && res.ToList()[1] != null)
            {
                var sf = res.ToList()[0];
                if (sf == string.Empty) return;

                var bitcoinadrress = GenerateBitCoinAddress(sf);

                var dec = await GetBalance(sf);
                if (dec == null) return;
                MainActivity.walletInfo.BalanceBitCoin = dec.ToList()[0];
                InitialisePages.mWalletsPage.UpdateUI(MainActivity.walletInfo.BalanceBitCoin, WalletsPage.Wallets.Bitcoin);
            }
        }

        private string GenerateBitCoinAddress(string v)
        {
            Base58Encoding bas58 = new Base58Encoding();
            if (string.IsNullOrEmpty(v)) return string.Empty; ;
            return bas58.Encode(OneWay_Hash(Encoding.ASCII.GetBytes(v)));
        }
        //RIPEMD160 
        //Public key K  => RIPEMD160(SHA256(K)) => HASH160 
        //Elliptic curve multiplication proccess
        static byte[] OneWay_Hash(byte[] entropy)
        {
            var checksum = Hashes.SHA256(entropy);
            var r160 = RIPEMD160.Create();
            return r160.ComputeHash(checksum);
        }

        private static byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }

        void Get_ScriptPubKey(string BitCAddress)
        {
            KeyId publicKeyHash = new KeyId(BitCAddress);
            BitcoinAddress mainNetAddress = publicKeyHash.GetAddress(MainActivity.net);
            Console.WriteLine(mainNetAddress.ScriptPubKey); // OP_DUP OP_HASH160 14836dbe7f38c5ac3d49e8d790af808a4ee9edcf OP_EQUALVERIFY OP_CHECKSIG
        }

        void Address_From_ScriptPubKey(KeyId publicKeyHash)
        {
            Script paymentScript = publicKeyHash.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(MainActivity.net);
        }

        void GenerateBitCoinAddress_FromSPKHash(Script paymentScript)
        {
            KeyId samePublicKeyHash = (KeyId)paymentScript.GetDestination();
            BitcoinPubKeyAddress sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, MainActivity.net);
        }

        public void GenerateAddress(string ssMnemo, int ssKeynumber, out string ssPubKey, out string ssPrivateKey)
        {
            var net = Network.TestNet;
            var restoreNnemo = new Mnemonic(ssMnemo); //Retrieve private key from mnemonic
            ExtKey masterKey = restoreNnemo.DeriveExtKey();
            KeyPath keypth = new KeyPath("m/44'/0'/0'/0/" + ssKeynumber);
            ExtKey key = masterKey.Derive(keypth);
            ssPubKey = key.PrivateKey.PubKey.GetAddress(net).ToString();
            ssPrivateKey = key.PrivateKey.GetBitcoinSecret(net).ToString();
        }

        //Master keys
        //m/purpose'/coin_type'/account'/change/address_index
        // m /44'/ uses the BIP 44 which means a multiaccount structure
        // m/44'/0'/ uses the Bitcoin Core , 1 is using the Test Net , 2 the Litecoin
        // m/44'/0'/0' are the accounts each wallet might contain several bitcoin accounts
        public static async Task<IEnumerable<string>> GenerateAdd_HDWalletBIP44(string ssMnemo, string password = "", int randomkey = 0)
        {
            BitcoinPubKeyAddress ssPubKey = null;
            KeyId pubkeyhash = null;
            await Task.Run(() =>
            {
                try
                {
                    var net = MainActivity.net;
                    var restoreNnemo = new Nmnemonic(ssMnemo);
                    ExtKey masterKey = restoreNnemo.DeriveExtKey(password);
                    KeyPath keypth = new KeyPath($"m/44'/0'/0'/0/{randomkey}");
                    //  ExtPubKey masterPubKey = masterKey.Neuter(); //GetPubkeys only
                    ExtKey key = masterKey.Derive(keypth);
                    //  ExtKey masterkey = restoreNnemo.DeriveExtKey();
                    //  ExtKey keys = masterkey.Derive(keypth);
                    var chaincode = key.ChainCode;
                    pubkeyhash = key.PrivateKey.PubKey.Hash;
                    ssPubKey = key.PrivateKey.PubKey.GetAddress(net);
                    //  ssPrivateKey = key.PrivateKey.GetBitcoinSecret(net).ToString();
                    MainActivity.Preferences.Edit().PutString("ssPubKey", pubkeyhash.ToString()).Apply();
                    MainActivity.Preferences.Edit().PutString("BitcoinPubKeyAddress", ssPubKey.ToString()).Apply();
                    //    MainActivity.Preferences.Edit().PutString("ssPrivateKey", ssPrivateKey).Apply();

                }
                catch (Exception e)
                {
                    ssPubKey = null;
                }
            });

            return new string[] { ssPubKey.ToString() };
        }
        public static List<string> TransactionsID = new List<string>();
        public async static Task<IEnumerable<decimal>> GetBalance(string BTAdrs)
        {
            try
            {
                var resBalanceIN = 0.0M;
                var resConfirmedBalanceIN = 0.0M;
                await Task.Run(() =>
                {
                    QBitNinjaClient client = new QBitNinjaClient(MainActivity.net);
                    BalanceModel balance = client.GetBalance(new BitcoinPubKeyAddress(BTAdrs)).Result;

                    var s = 0;
                    if (balance.Operations.Count > 0)
                    {
                        var unspentCoins = new List<Coin>();
                        var unspentCoinsConfirmed = new List<Coin>();
                        foreach (var operation in balance.Operations.Where(x => x.ReceivedCoins != null && x.ReceivedCoins.Count > 0)) //txid
                        {
                            TransactionsID.Add(operation.TransactionId.ToString());
                            unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                            if (operation.Confirmations > 0)
                                unspentCoinsConfirmed.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                        }
                        resBalanceIN = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
                        resConfirmedBalanceIN = unspentCoinsConfirmed.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));


                    }
                });
                return (new decimal[] { resBalanceIN, resConfirmedBalanceIN });
            }
            catch (Exception e) when (e is Exception || e is ArgumentException || e is ArgumentNullException)
            {
                MainActivity.act.RunOnUiThread(() =>
                {
                    Toast.MakeText(MainActivity.act, e.Message, ToastLength.Long).Show();
                });
                return null;
            }
        }
    }

}