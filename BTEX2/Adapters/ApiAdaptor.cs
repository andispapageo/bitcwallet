using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BitCWallet.Adapters
{
    class ApiAdaptor : RecyclerView.Adapter
    {
        public event EventHandler<ApiAdaptorClickEventArgs> ItemClick;
        public event EventHandler<ApiAdaptorClickEventArgs> ItemLongClick;
        List<Result> items;
        public class Result
        {
            public string MarketName { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public DateTime TimeStamp { get; set; }
            public double Bid { get; set; }
            public double Ask { get; set; }
            public int OpenBuyOrders { get; set; }
            public int OpenSellOrders { get; set; }
            public double PrevDay { get; set; }
            public DateTime Created { get; set; }
            public string MarketCurrencyLong { get; set; }
            public string BaseCurrencyLong { get; set; }
            public string MinTradeSize { get; set; }
            public string IsActive { get; set; }

        }

        public class RootObject
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ObservableCollection<Result> Result { get; set; }
        }

        public ApiAdaptor(List<Result> data)
        {
            items = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = null;
            var id = Resource.Layout.recyclerLayout;
            itemView = LayoutInflater.From(parent.Context). Inflate(id, parent, false);
            var vh = new ApiAdaptorViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }
  
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            var holder = viewHolder as ApiAdaptorViewHolder;
            Result apiObj = items[position];
            holder.TextView1.Text = apiObj.MarketCurrencyLong.ToString();
            holder.TextView2.Text = apiObj.BaseCurrencyLong.ToString();
            holder.TextView3.Text = apiObj.MinTradeSize.ToString();
            holder.TextView4.Text = apiObj.MarketName.ToString();
        }
       
        public override int ItemCount => items.Count;
        void OnClick(ApiAdaptorClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ApiAdaptorClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        internal void Update(List<Result> root)
        {
            MainActivity.act.RunOnUiThread(() =>
            {
                items = root;
                this.NotifyDataSetChanged();
            });
        }

    }

    public class ApiAdaptorViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextView1 { get; set; }
        public TextView TextView2 { get; set; }
        public TextView TextView3 { get; set; }
        public TextView TextView4 { get; set; }
        public ApiAdaptorViewHolder(View itemView, Action<ApiAdaptorClickEventArgs> clickListener,Action<ApiAdaptorClickEventArgs> longClickListener) : base(itemView)
        {
            TextView1 = itemView.FindViewById(Resource.Id.textView1) as TextView;
            TextView2 = itemView.FindViewById(Resource.Id.textView2) as TextView;
            TextView3 = itemView.FindViewById(Resource.Id.textView3) as TextView;
            TextView4 = itemView.FindViewById(Resource.Id.textView4) as TextView;
            itemView.Click += (sender, e) => clickListener(new ApiAdaptorClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ApiAdaptorClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class ApiAdaptorClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}