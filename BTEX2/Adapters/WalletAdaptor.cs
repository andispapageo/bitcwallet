using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using BitCWallet.Models;
using static BitCWallet.Pages.WalletsPage;

namespace BitCWallet.Adapters
{

    class WalletAdaptor : RecyclerView.Adapter
    {
        public event EventHandler<WalletAdaptorClickEventArgs> ItemClick;
        public event EventHandler<WalletAdaptorClickEventArgs> ItemLongClick;
        List<WalletsSchema> items;
        public WalletAdaptor(List<WalletsSchema> data)
        {
            items = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = null;
            var id = Resource.Layout.walletsadapLayout;
            itemView = LayoutInflater.From(parent.Context).Inflate(id, parent, false);
            var vh = new WalletAdaptorViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            var holder = viewHolder as WalletAdaptorViewHolder;
            WalletsSchema obj = items[position];
            holder.walletBalance_Market.Text = obj.BallanceMarket;
            holder.walletBalance_USD.Text = obj.BalanceUSD;
            holder.marketImage.SetImageResource(obj.imageId);
        }
        public override int ItemCount => items.Count;
        void OnClick(WalletAdaptorClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(WalletAdaptorClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        internal void Update(Wallets res, decimal market, decimal convUSD)
        {
            if (MainActivity.act == null) return;
            MainActivity.act.RunOnUiThread(() =>
            {
                var a = items.Find(x => x.id == res);
                a.BallanceMarket = "BTC" + market.ToString();
                a.BalanceUSD = "$" + convUSD.ToString("0.00");
                NotifyItemChanged(items.FindIndex(x => x.id == res));
            });
        }
    }

    public class WalletAdaptorViewHolder : RecyclerView.ViewHolder
    {
        public TextView walletBalance_Market { get; set; }
        public TextView walletBalance_USD { get; set; }
        public ImageView marketImage { get; set; }

        public WalletAdaptorViewHolder(View itemView, Action<WalletAdaptorClickEventArgs> clickListener,
                            Action<WalletAdaptorClickEventArgs> longClickListener) : base(itemView)
        {
            walletBalance_Market = itemView.FindViewById(Resource.Id.balMarket) as TextView;
            walletBalance_USD = itemView.FindViewById(Resource.Id.balanceUSD) as TextView;
            marketImage = itemView.FindViewById(Resource.Id.marketImage) as ImageView;

            itemView.Click += (sender, e) => clickListener(new WalletAdaptorClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new WalletAdaptorClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class WalletAdaptorClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}