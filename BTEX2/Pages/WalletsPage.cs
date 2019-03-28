
using System;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using NBitcoin;

using static BitCWallet.MainActivity;
using BitCWallet.Models;
using BitCWallet.Adapters;
using Android.Gms.Common;
using System.Linq;
using ZXing;

using Android.Widget;
using Android.Graphics.Drawables;
using ZXing.Mobile;
using static BitCWallet.Pages.TransactionsPage;
using BitCWallet.Classes;

namespace BitCWallet.Pages
{
    public class WalletsPage : Fragment
    {

        public static bool once { get; set; }
        public bool mSignInClicked { get; private set; }
        SignInButton mGoogleSignIn;
        View thisView;
        WalletAdaptor wAdaptor;
        RecyclerView recycler
        {
            get { return thisView?.FindViewById(Resource.Id.walletsrecycler) as RecyclerView; }
        }

        public enum Wallets
        {
            Bitcoin,
            Ethereum
        }

        private void MGoogleSignIn_Click(object sender, EventArgs e)
        {
            if (!mGoogleApiClient.IsConnecting)
            {
                mSignInClicked = true;
                act.ResolveSignInError();

            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var mainWalletsList = new List<WalletsSchema>();
            mainWalletsList.Add(new WalletsSchema() { id = Wallets.Bitcoin, BallanceMarket = "BTC 0.00", BalanceUSD = "$ 0.00", imageId = Resource.Drawable.BC_Logo_ });
            mainWalletsList.Add(new WalletsSchema() { id = Wallets.Ethereum, BallanceMarket = "ETH 0.00", BalanceUSD = "$ 0.00", imageId = Resource.Drawable.ether });
            wAdaptor = new WalletAdaptor(mainWalletsList);

            SafeWallet.Create(out string mnemonic,"andis", net);
        
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            thisView = inflater.Inflate(Resource.Layout.wLayout, container, false);
            var layoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.Vertical);
            recycler.SetLayoutManager(layoutManager);
            recycler.SetAdapter(wAdaptor);

            mGoogleSignIn = thisView.FindViewById(Resource.Id.sign_in_button) as SignInButton;
            mGoogleSignIn.Click += MGoogleSignIn_Click;

            CreateQR(thisView.FindViewById(Resource.Id.QR) as ImageView);
            SetAddress(thisView.FindViewById(Resource.Id.bitcoinaddress) as TextView);
            InitiliseWallet_Balance();

            P2PK();

            return thisView;
        }
        WalletsClass resBalance, resBalanceB;
          private async void InitiliseWallet_Balance()
        {
            if (WIFI)
            {
                if (!once && CheckStartuP())
                {
                    AdjustBitCoinAddress();
                    // AdjustEthereumAddress();
                    once = true;
                }
                else
                {
                    var dec = await BitCoinWallet.GetBalance( Get_ScriptPubKey(pubkey));
                    if (dec != null)
                    {
                        walletInfo.BalanceBitCoin = dec.ToList()[0];
                        InitialisePages.mWalletsPage.UpdateUI(walletInfo.BalanceBitCoin, Wallets.Bitcoin);
                        resBalance = new WalletsClass() { address = MainActivity.BitcoinPubKeyAddress, amount = dec.ToList()[1] };
                    }
                    var decA = await BitCoinWallet.GetBalance(Get_ScriptPubKey("651875a59a027266d443ece45910013b8f48654c"));
                    if (decA != null)
                    {
                        resBalanceB = new WalletsClass() { address = "mpjVqufQkDLYgrZwr8Ro82prrpKsJumNKd", amount = decA.ToList()[1] };
                    }
                    WalletsClass[] arrayB = new WalletsClass[2];
                    if (resBalance != null) arrayB[0] = resBalance;
                    if (resBalanceB != null) arrayB[1] = resBalanceB;
                    InitialisePages.mTransactionsPage.transAdapter.Update(arrayB);
                }
            }
        }


        private void SetAddress(TextView textView)
        {
            if (textView != null && MainActivity.BitcoinPubKeyAddress != string.Empty)
                textView.Text = MainActivity.BitcoinPubKeyAddress;
        }

        private void CreateQR(ImageView qR)
        {
            if (string.IsNullOrEmpty(MainActivity.BitcoinPubKeyAddress) 
                || string.IsNullOrWhiteSpace(MainActivity.BitcoinPubKeyAddress))
                return;

            IBarcodeWriter writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            var result = writer.Encode(MainActivity.BitcoinPubKeyAddress);
            var barcodeBitmap = new BitmapRenderer();
            var image = barcodeBitmap.Render(result, BarcodeFormat.QR_CODE, MainActivity.BitcoinPubKeyAddress);
            var res = new BitmapDrawable(Android.App.Application.Context.Resources, image);
            qR.SetImageBitmap(image);
        }

        public void P2PK()
        {
            NBitcoin.Block genesisBlock = Network.Main.GetGenesis();
            Transaction firstTransactionEver = genesisBlock.Transactions.First();
            Console.WriteLine(firstTransactionEver);

            var firstOutputEver = firstTransactionEver.Outputs.First();
            var firstScriptPubKeyEver = firstOutputEver.ScriptPubKey;
            var firstBitcoinAddressEver = firstScriptPubKeyEver.GetDestinationAddress(net);

            Console.WriteLine(firstBitcoinAddressEver == null);
            var firstPublicKeyEver = firstScriptPubKeyEver.GetDestinationPublicKeys().First();
            Console.WriteLine(firstPublicKeyEver);
            var privateKeyForComparing = new Key();
            Console.WriteLine("Pay To Public Key : " + privateKeyForComparing.PubKey.ScriptPubKey);
            Console.WriteLine("Pay To Public Key Hash : " + privateKeyForComparing.PubKey.Hash.ScriptPubKey);
        }
        public void MultiSig()
        {
            Key privateKeyGenerator = new Key();
            BitcoinSecret bSecret = privateKeyGenerator.GetBitcoinSecret(net);
            Key privateKeyFromBitcoinSecret = bSecret.PrivateKey;
            Console.WriteLine($"privateKeyFromBitcoinSecret.ToString(Network.Main): {privateKeyFromBitcoinSecret.ToString(net)}");
        }

        public void BitCoindAdd()
        {
            //We learned that a Bitcoin address is generated by a public key hash and a network identifier.
            Key privateKeyForPublicKeyHash = new Key();
            var publicKeyHash = privateKeyForPublicKeyHash.PubKey.Hash;
            var bitcoinAddress = publicKeyHash.GetAddress(net);
            Console.WriteLine(publicKeyHash);
            Console.WriteLine(bitcoinAddress);
        }
        public void BitcoinPassPhrase()
        {
            var passphraseCode = new BitcoinPassphraseCode("my secret", net, null);

            EncryptedKeyResult encryptedKeyResult = passphraseCode.GenerateEncryptedSecret();

            var generatedAddress = encryptedKeyResult.GeneratedAddress;
            var encryptedKey = encryptedKeyResult.EncryptedKey;
            var confirmationCode = encryptedKeyResult.ConfirmationCode;

            Console.WriteLine(generatedAddress); // 14KZsAVLwafhttaykXxCZt95HqadPXuz73
            Console.WriteLine(encryptedKey); // 6PnWtBokjVKMjuSQit1h1Ph6rLMSFz2n4u3bjPJH1JMcp1WHqVSfr5ebNS
            Console.WriteLine(confirmationCode); // cfrm38VUcrdt2zf1dCgf4e8gPNJJxnhJSdxYg6STRAEs7QuAuLJmT5W7uNqj88hzh9bBnU9GFkN

            Console.WriteLine(confirmationCode.Check("my secret", generatedAddress)); // True
            var bitcoinPrivateKey = encryptedKey.GetSecret("my secret");
            Console.WriteLine(bitcoinPrivateKey.GetAddress() == generatedAddress); // True
            Console.WriteLine(bitcoinPrivateKey); // KzzHhrkr39a7upeqHzYNNeJuaf1SVDBpxdFDuMvFKbFhcBytDF1R
        }

      
        public void AdjustBitCoinAddress(bool execute = false)
        {
            if (mainCheck(!true))
                new BitCoinWallet(true, pubkey);
        }

        string Get_ScriptPubKey(string BitCAddress)
        {
            var publicKey = new KeyId(BitCAddress);
            var bitcoinAddress = publicKey.GetAddress(net);

            Console.WriteLine(publicKey);
            Console.WriteLine(bitcoinAddress);

            var scriptPubKey = bitcoinAddress.ScriptPubKey;
            Console.WriteLine(scriptPubKey);
            var sameBitcoinAddress = scriptPubKey.GetDestinationAddress(net);
            Console.WriteLine(sameBitcoinAddress);
            return bitcoinAddress.ToString();
        }

        void Address_From_ScriptPubKey(KeyId publicKey)
        {
            Script paymentScript = publicKey.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(net);
        }

        void GenerateBitCoinAddress_FromSPKHash(Script paymentScript)
        {

            KeyId samePublicKeyHash = (KeyId)paymentScript.GetDestination();
            var sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, net);
        }

        internal async void UpdateUI(decimal v, Wallets res)
        {
            if (act == null || wAdaptor == null) return;
            decimal usdAPI = 0;
            if (InitialisePages.mAPIPage != null)
            {
                var dividerUSD = await InitialisePages.mAPIPage.GetUSD("USD-BTC");
                if (decimal.TryParse(dividerUSD, out decimal usd))
                    usdAPI = usd;
            }
            wAdaptor.Update(res,
                v, usdAPI != 0
                ? v / usdAPI
                : 0);
        }

        bool mainCheck(bool cases)
        {
            if (string.IsNullOrWhiteSpace(userpsw) && string.IsNullOrWhiteSpace(pubkey)) return cases;
            else return !cases;
        }

        private bool CheckStartuP()
        {
            var res = mainCheck(true);
            if (res)
            {
                var dialogSetMnemonic = new DialogSetup();
                dialogSetMnemonic.Show(fm, "mnemonic");
            }
            return res;
        }

    }
}