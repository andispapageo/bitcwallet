using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using BitCWallet.Models;
using BitCWallet.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static BitCWallet.SchemasModel;
using NFragment = Android.Support.V4.App.Fragment;
using NFragmentManager = Android.Support.V4.App.FragmentManager;
namespace BitCWallet
{
    [Activity(Label = "BitCWallet", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        public static WalletsInfo walletInfo;
        public static MainActivity act { get; set; }
        public bool mSignInClicked { get; private set; }


        public static NBitcoin.Network net = NBitcoin.Network.TestNet;


        static InitialisePages pg;
        public Encryption enc;
        public static NFragmentManager fm;
        static ViewPager viewpager;
        static List<NFragment> listofpages;
        public static GoogleApiClient mGoogleApiClient;
        static CustomFragmentAdapter adapter;
        public static ISharedPreferences Preferences = Application.Context.GetSharedPreferences("m", FileCreationMode.Private);

        BarcodeDetector barcodeDetector;
        CameraSource cameraSource;
        NetState netconn;
        public static bool WIFI { get; set; }
        ConnectionResult mConnectionResult;
        private bool mIntentinProgress;

        const int RequestCameraPermisionID = 1001;
        private bool mInfoPopulated;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static string words
        {
            get { return Preferences.GetString("mnemonic", string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static string pubkey
        {
            get { return Preferences.GetString("ssPubKey", string.Empty); }
            set { Preferences.GetString("ssPubKey", value); }
        }

        int getKeyNumber
        {
            get { return Preferences.GetInt("keynum", 0); }
        }

        public static string userpsw
        {
            get { return Preferences.GetString("token", string.Empty); }
            set { Preferences.GetString("token", value); }
        }

        public static string BitcoinPubKeyAddress
        {
            get { return Preferences.GetString("BitcoinPubKeyAddress", string.Empty); }
            set { Preferences.GetString("BitcoinPubKeyAddress", value); }
        }

        string this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0x00: return "application/x-www-form-urlencoded;";
                    case 0x01: return "https://insight.bitpay.com/api/addr/";
                    case 0x02: return "https://api.coinmarketcap.com/v1/ticker/bitcoin/";
                    case 0x03: return "https://bittrex.com/api/v1.1/public/getmarketsummary?market=usdt-btc";
                    case 0x04: return "https://www.blockchain.com/btc/tx/7957a35fe64f80d234d76d83a2a8f1a0d8149a41d81de548f0a65a8a999f6f18";
                    default: return "";
                }
            }
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:

                    return true;
                case Resource.Id.navigation_dashboard:

                    return true;
                case Resource.Id.navigation_notifications:

                    return true;
            }
            return false;
        }

        public void OnConnected(Bundle connectionHint)
        {
            mSignInClicked = false;
            if (mInfoPopulated)
            {
                return;
            }
            if (PlusClass.PeopleApi.GetCurrentPerson(mGoogleApiClient) != null)
            {
                IPerson plusUser = PlusClass.PeopleApi.GetCurrentPerson(mGoogleApiClient);

                if (plusUser.HasDisplayName)
                {
                    //mName.Text += plusUser.DisplayName;
                }

                if (plusUser.HasTagline)
                {
                    // mTagline.Text += plusUser.Tagline;
                }

                if (plusUser.HasBraggingRights)
                {
                    // mBraggingRights.Text += plusUser.HasBraggingRights;
                }

                if (plusUser.HasRelationshipStatus)
                {
                    switch (plusUser.RelationshipStatus)
                    {
                        case 0:
                            //  mRelationship.Text += "Single";
                            break;

                        case 1:
                            //  mRelationship.Text += "In a relationship";
                            break;

                        case 2:
                            //   mRelationship.Text += "Engaged";
                            break;

                        case 3:
                            //   mRelationship.Text += "Married";
                            break;

                        case 4:
                            //mRelationship.Text += "It's complicated";
                            break;

                        case 5:
                            //  mRelationship.Text += "In an open relationship";
                            break;

                        case 6:
                            //  mRelationship.Text += "Widowed";
                            break;

                        case 7:
                            //  mRelationship.Text += "In a domestic partnership";
                            break;

                        case 8:
                            //  mRelationship.Text += "In a civil union";
                            break;

                        default:
                            //   mRelationship.Text += "Unknown";
                            break;
                    }
                }

                if (plusUser.HasGender)
                {
                    switch (plusUser.Gender)
                    {
                        case 0:
                            // mGender.Text += "Male";
                            break;

                        case 1:
                            //  mGender.Text += "Female";
                            break;

                        case 2:
                            //  mGender.Text += "Other";
                            break;

                        default:
                            //  mGender.Text += "Unknown";
                            break;
                    }
                }

                mInfoPopulated = true;
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            if (!mIntentinProgress)
                mConnectionResult = result;
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (mGoogleApiClient.IsConnected)
            {
                mGoogleApiClient.Disconnect();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            enc = new Encryption();
            walletInfo = new WalletsInfo();

            if (WIFI)
            {
                mGoogleApiClient = new GoogleApiClient.Builder(this, this, this).AddApi(PlusClass.API)
                   .AddScope(PlusClass.ScopePlusLogin)
                   .AddScope(PlusClass.ScopePlusProfile)
                   .Build();
                mGoogleApiClient.Connect();
            }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            act = this;

            netconn = new Abstract_Net();
            WIFI = netconn.WifiState(this);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            pg = new InitialisePages();
            fm = SupportFragmentManager;
            viewpager = FindViewById(Resource.Id.pager1) as ViewPager;
            AdjustPages();

        }

        private void AdjustPages()
        {
            if (pg == null) return;
            if (pg.set)
            {
                listofpages = new List<NFragment>();
                    listofpages.Add(InitialisePages.mWalletsPage);
                    listofpages.Add(InitialisePages.mTransactionsPage);
                    listofpages.Add(InitialisePages.mAPIPage);

                    adapter = new CustomFragmentAdapter(listofpages, fm, this);
                    viewpager.Adapter = adapter;
                    viewpager.OffscreenPageLimit = adapter.Count;
               
            }
        }

        void Functions()
        {
            Blockchain phillyCoin = new Blockchain();
            phillyCoin.AdditionOf_Block(new Block() { TimeStamp = DateTime.Now, PreviousHash = null, Data = "{sender:A,receiver:B,UTXO:10}" });
            phillyCoin.AdditionOf_Block(new Block() { TimeStamp = DateTime.Now, PreviousHash = null, Data = "{sender:B,receiver:A,UTXO:5}" });
            phillyCoin.AdditionOf_Block(new Block() { TimeStamp = DateTime.Now, PreviousHash = null, Data = "{sender:B,receiver:A,UTXO:5}" });

            Console.WriteLine(phillyCoin.IsValid());
            getSchema_InsightBitPayCom<SchemaInsightBitPayCom>("1Cdid9KFAaatwczBwBttQcwXYCpvK8h7FK");

        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 0)
            {
                if (resultCode != Result.Ok)
                {
                    //  mSignInClicked = false;
                }

                //  mIntentInProgress = false;

                if (!mGoogleApiClient.IsConnecting)
                {
                    mGoogleApiClient.Connect();
                }
            }
        }

        public void ResolveSignInError()
        {
            if (mGoogleApiClient.IsConnected)
            {
                return;
            }
            try
            {
                if (mConnectionResult.HasResolution)
                {
                    mIntentinProgress = true;
                    StartIntentSenderForResult(mConnectionResult.Resolution.IntentSender, 0, null, 0, 0, 0, null);
                }
            }
            catch (IntentSender.SendIntentException e)
            {
                mIntentinProgress = false;
                mGoogleApiClient.Connect();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case RequestCameraPermisionID:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Permission.Granted)
                            {
                                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, RequestCameraPermisionID);
                                return;
                            }
                            try
                            {
                                // cameraSource.Start(surfaceView.Holder);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }

                    break;
            }
        }

        async void getSchema_InsightBitPayCom<T>(string urladress) where T : class
        {
            using (WebClient client = new WebClient())
            {
                var dataString = $"{this[1]}{urladress}";
                var dataBytes = Encoding.UTF8.GetBytes(dataString);
                try
                {
                    var responseBytes = await client.DownloadDataTaskAsync(dataString);
                    var responseString = Encoding.UTF8.GetString(responseBytes);

                    if (string.IsNullOrEmpty(responseString)) return;
                    if (IsValidJson(responseString))
                    {
                        var resultObject = new ObjectEnumerable<T> { Objadjust = Deserialize<T>(responseString) };
                    }
                    else return;
                }
                catch (WebException e) { Console.WriteLine(e.Message); return; }
            }
        }
        private bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try { var obj = JToken.Parse(strInput); return true; }
                catch (JsonReaderException jex) { Console.WriteLine(jex.Message); return false; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); return false; }
            }
            else { return false; }
        }
        public T Deserialize<T>(string UTF8string)
        {
            return JsonConvert.DeserializeObject<T>(UTF8string);
        }

        public class ObjectEnumerable<T> : object
        {
            public T Objadjust { get; set; }
        }

        public class Blockchain
        {
            public IList<Block> Chain = new List<Block>();
            public Blockchain() { AddGenesisBlock(); }
            public Block @Genesis() { return new Block { TimeStamp = DateTime.Now, PreviousHash = null, Data = "{}" }; }
            public Block @GetLatestBlock() { return Chain[Chain.Count - 1]; }
            public void @AddGenesisBlock() { Chain.Add(Genesis()); }
            public void @AdditionOf_Block(Block block)
            {
                Block latestBlock = GetLatestBlock();
                block.Index = latestBlock.Index + 1;
                block.PreviousHash = latestBlock.Hash;
                block.Hash = block.ComputingHash();
                block.Mine(this.Difficulty);
                Chain.Add(block);
            }
            public int Difficulty { set; get; } = 2;
            public bool IsValid()
            {
                if (Chain == null) return false;
                for (int i = 1; i < Chain.Count; i++) // Avoid Genesis as the has is null
                {
                    Block currentBlock = Chain[i];
                    Block previousBlock = Chain[i - 1];
                    if (currentBlock.Hash != currentBlock.ComputingHash()) return false;
                    if (currentBlock.PreviousHash != previousBlock.Hash) return false;
                }
                return true;
            }
        }

        public class Block
        {
            public enum SHASets { SHA1, SHA256, SHA384, SHA51 }
            public int Index { get; set; }
            public DateTime TimeStamp { get; set; }
            public string PreviousHash { get; set; }
            public string Hash { get; set; }
            public string Data { get; set; }
            public int Nonce { get; set; } = 0;
            public void Mine(int difficulty)
            {
                var leadingZeros = new string('0', difficulty);
                while (Hash == null || Hash.Substring(0, difficulty) != leadingZeros)
                {
                    Nonce++;
                    Hash = ComputingHash();
                }
            }
            public string ComputingHash(string hashAlgorithm = "")
            {

                var plainTextBytes = Encoding.UTF8.GetBytes($"{TimeStamp}-{PreviousHash ?? ""}-{Data} - {Nonce}");
                byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length];
                for (int i = 0; i < plainTextBytes.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainTextBytes[i];
                }

                HashAlgorithm hash;
                switch (hashAlgorithm.ToUpper())
                {
                    case var s when s == Enum.ToObject(typeof(SHASets), 0) as string: hash = new SHA1Managed(); break;
                    case var s when s == Enum.ToObject(typeof(SHASets), 1) as string: hash = new SHA256Managed(); break;
                    case var s when s == Enum.ToObject(typeof(SHASets), 2) as string: hash = new SHA384Managed(); break;
                    case var s when s == Enum.ToObject(typeof(SHASets), 3) as string: hash = new SHA512Managed(); break;
                    default: hash = new MD5CryptoServiceProvider(); break;
                }
                var hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
                var hashWithSaltBytes = new byte[hashBytes.Length];
                for (int i = 0; i < hashBytes.Length; i++)
                    hashWithSaltBytes[i] = hashBytes[i];

                string hashValue = Convert.ToBase64String(hashWithSaltBytes);
                return hashValue;
            }
        }


        public class WalletTransaction
        {
            public string From { get; set; }
            public string To { get; set; }
            public string Token { get; set; }
            public decimal Amount { get; set; }
        }

        public class EthAccountToken : AccountToken
        {
            public EthAccountToken()
            {
                Symbol = "ETH";
            }
        }
        public class Token
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public string ImgUrl { get; set; }
            public int NumberOfDecimalPlaces { get; set; }
        }
        public class AccountToken
        {
            public string Symbol { get; set; }
            public decimal Balance { get; set; }
        }

        public class AccountInfo
        {
            public AccountInfo()
            {
                Eth = new EthAccountToken();
                AccountTokens = new List<AccountToken>();
            }
            public string Address { get; set; }
            public AccountToken Eth { get; set; }
            public List<AccountToken> AccountTokens { get; set; }

        }
    }
}

