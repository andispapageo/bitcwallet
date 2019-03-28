using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using BitCWallet;
using PMADialog = Android.Support.V4.App.DialogFragment;
namespace BitCWallet.Pages
{
    public class DialogShowWords : PMADialog
    {
        View view;
        TextView words { get { return view.FindViewById(Resource.Id.words) as TextView; }}
        public string mSeed { get; set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.wordsLayout, container, false);
            words.Text = mSeed;
            return view;
        }
        public override void OnStop()
        {
            base.OnStop();
            if (InitialisePages.mWalletsPage == null) return;
            InitialisePages.mWalletsPage.AdjustBitCoinAddress();
        }
    }
}
