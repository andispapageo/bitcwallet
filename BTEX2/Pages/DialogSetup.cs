using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using PMADialog = Android.Support.V4.App.DialogFragment;
using Nmnemonic = NBitcoin.Mnemonic;
using NWorldList = NBitcoin.Wordlist;
using NBitcoin;
using static BitCWallet.MnemonicEx1;
namespace BitCWallet.Pages
{
    public class DialogSetup : PMADialog
    {
        View mainMnemonice;
        LinearLayout mainLay { get { return mainMnemonice.FindViewById<LinearLayout>(Resource.Id.mainlay); } }
        TextInputLayout userName { get { return mainMnemonice.FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutUsername); } }
        TextInputLayout password { get { return mainMnemonice.FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutPassword); } }
        Button login { get { return mainMnemonice.FindViewById<Button>(Resource.Id.btnLogin); } }
        Button restore { get { return mainMnemonice.FindViewById<Button>(Resource.Id.btnRestore); } }
        public string walletWords { get; private set; }
        public int keyA { get; private set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            keyA = MainActivity.act.enc.CreateRandom_Key();
            MainActivity.Preferences.Edit().PutInt("keynum", keyA).Apply();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            mainMnemonice = inflater.Inflate(Resource.Layout.dialogMnemonic, container, false);
            login.Click += Login_Click;
            return mainMnemonice;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (areFieldsAdded())
                CreateHashForPassword(password.EditText.Text);
        }

        private bool areFieldsAdded()
        {
            foreach (TextInputLayout s in GetViewsByType<TextInputLayout>(mainLay))
            {
                if (string.IsNullOrWhiteSpace(s.EditText.Text)) return false;
                return true;
            }
            return false;
        }

        private void CreateHashForPassword(string text)
        {
            var encrypt = new Encryption(text);
            if (encrypt.success)
                CreateWalletNBitcoit();
        }
        public static string createPassPharaseCOde( string password)
        {
            BitcoinPassphraseCode passphraseCode = new BitcoinPassphraseCode(password,MainActivity.net, null);
            EncryptedKeyResult encryptedKey1 = passphraseCode.GenerateEncryptedSecret();
            BitcoinSecret privateKey = encryptedKey1.EncryptedKey.GetSecret(password);
            return encryptedKey1.EncryptedKey.ToString();
        }
        private async Task<string> CreateWallet()
        {
            string ssMnemo = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    var mnemonic = new Nmnemonic(NWorldList.English, WordCount.Twelve);
                    ssMnemo = mnemonic.ToString();
                }
                catch (Exception e){Console.WriteLine(e.Message); ssMnemo = string.Empty;}
            });
            return await Task.FromResult(ssMnemo);
        }

        private void CreateMnemonicCustom()
        {
            Wordlist mWordlist = new Wordlist();
            var mnemonic = new MnemonicEx1(Wordlist.English, NWordCount.Twelve);
            if (mnemonic == null)return;
            var res = mWordlist.GetSentence(mnemonic._Indices);
            if (string.IsNullOrWhiteSpace(res))return;
            var clearres = res.Replace("\0", " ").Split(' ');
            var resPrivatekey = mnemonic.PrivateKeyAddress;
            var showDialog = new DialogShowWords();
            res = res.Replace("\0", " ");
            showDialog.mSeed = res;
            showDialog.Show(MainActivity.fm, "seed");
        }

        public IEnumerable<T> GetViewsByType<T>(ViewGroup root) where T : View
        {
            var views = new List<T>();
            try
            {
                var children = root.ChildCount;
                for (var i = 0; i < children; i++)
                {
                    var child = root.GetChildAt(i);
                    if (child is T myChild) views.Add(myChild);
                    else if (child is ViewGroup viewGroup) views.AddRange(GetViewsByType<T>(viewGroup));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return views;
        }
    }
}
