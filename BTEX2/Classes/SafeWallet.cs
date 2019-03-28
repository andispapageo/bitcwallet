
using NBitcoin;
using Nmnemonic = NBitcoin.Mnemonic;
using NWorldList = NBitcoin.Wordlist;
namespace BitCWallet.Classes
{
    class SafeWallet
    {
        private const string StealthPath = "0'";
        private const string NormalHdPath = "1'";
        private Network _network;
        private ExtKey _seedPrivateKey;
        public string WalletFilePath { get; }
        public string Seed => _seedPrivateKey.GetWif(_network).ToWif();
        public string SeedPublicKey => _seedPrivateKey.Neuter().GetWif(_network).ToWif();
        public SafeWallet(SafeWallet safe)
        {
            _network = safe._network;
            _seedPrivateKey = safe._seedPrivateKey;
            WalletFilePath = safe.WalletFilePath;
        }
        public SafeWallet(string password, Network network, string mnemonicString = null) 
        {
            _network = network;
            if (mnemonicString != null)
                SetSeed(password, mnemonicString);

        }
        public virtual string GetAddress(int index)
        {
            var startPath = NormalHdPath;
            var keyPath = new KeyPath(startPath + "/" + index);
            return _seedPrivateKey.Derive(keyPath).ScriptPubKey.GetDestinationAddress(_network).ToString();
        }

        private Nmnemonic SetSeed(string password, string mnemonicString = null)
        {
            var mnemonic = mnemonicString == null ? new Nmnemonic(NWorldList.English, WordCount.Twelve) : new Nmnemonic(mnemonicString);
            _seedPrivateKey = mnemonic.DeriveExtKey(password);
            return mnemonic;
        }

        public static SafeWallet Recover(string mnemonic, string password, string walletFilePath, Network network)
        {
            var safe = new SafeWallet(password, network, mnemonic);
            return safe;
        }

    
        public static SafeWallet Create(out string mnemonic, string password, Network network)
        {
            var safe = new SafeWallet(password, network);
            mnemonic = safe.SetSeed(password).ToString();
            return safe;
        }
    }
}