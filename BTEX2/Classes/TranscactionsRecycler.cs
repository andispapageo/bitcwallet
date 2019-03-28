using System;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using static BitCWallet.Pages.TransactionsPage;
namespace BitCWallet.Classes
{
    public class TranscactionsRecycler : RecyclerView.Adapter
    {
        public event EventHandler<TranscactionsRecyclerClickEventArgs> ItemClick;
        public event EventHandler<TranscactionsRecyclerClickEventArgs> ItemLongClick;
        WalletsClass[] items;

        public TranscactionsRecycler(WalletsClass[] data)
        {
            items = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = null;
            var id = Resource.Layout.traobject;
            itemView = LayoutInflater.From(parent.Context).
              Inflate(id, parent, false);

            var vh = new TranscactionsRecyclerViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            var holder = viewHolder as TranscactionsRecyclerViewHolder;
            var objects = items[position];

            if (objects != null)
            {
                holder.address.Text = string.Format("{0} {1}", $"Wallet Address", !string.IsNullOrWhiteSpace(objects.address) ? objects.address : "");
                holder.inputs.Text = $"Total UTXO BTC:{objects.amount.ToString()}";
                holder.txID.Text = string.Format("{0} {1}", "TxID Hash", !string.IsNullOrEmpty(objects.txID) ? objects.txID.ToString() : string.Empty);
            }
        }

        public override int ItemCount => items.Length;

        void OnClick(TranscactionsRecyclerClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(TranscactionsRecyclerClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        internal void Update(WalletsClass[] array)
        {
            if (MainActivity.act == null) return;
            MainActivity.act.RunOnUiThread(() =>
            {
                items = array;
                NotifyDataSetChanged();
            });
        }
    }

    public class TranscactionsRecyclerViewHolder : RecyclerView.ViewHolder
    {
        public TextView address { get; set; }
        public TextView inputs { get; set; }
        public TextView txID { get; set; }

        public TranscactionsRecyclerViewHolder(View itemView, Action<TranscactionsRecyclerClickEventArgs> clickListener,
                            Action<TranscactionsRecyclerClickEventArgs> longClickListener) : base(itemView)
        {
            address = itemView.FindViewById(Resource.Id.Address) as TextView;
            inputs = itemView.FindViewById(Resource.Id.TxInputs) as TextView;
            txID = itemView.FindViewById(Resource.Id.txID) as TextView;
            
            itemView.Click += (sender, e) => clickListener(new TranscactionsRecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new TranscactionsRecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class TranscactionsRecyclerClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}