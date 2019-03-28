using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using BitCWallet.Classes;
using NBitcoin;
using NBitcoin.Protocol;
using Newtonsoft.Json.Linq;
using QBitNinja.Client;
using QBitNinja.Client.Models;
namespace BitCWallet.Pages
{
    public class TransactionsPage : Fragment
    {
        View thisView;
        public TranscactionsRecycler transAdapter;
        TextInputLayout fromAddress { get { return thisView.FindViewById(Resource.Id.fromadd) as TextInputLayout; } }
        TextInputLayout toAddress { get { return thisView.FindViewById(Resource.Id.toadd) as TextInputLayout; } }
        TextInputLayout amounttoSend { get { return thisView.FindViewById(Resource.Id.sendButton) as TextInputLayout; } }
        Button send { get { return thisView.FindViewById(Resource.Id.sendButton) as Button; } }
        RecyclerView recycleList { get { return thisView.FindViewById(Resource.Id.recycleList) as RecyclerView; } }
        Button toggle { get { return thisView.FindViewById(Resource.Id.toggle) as Button; } }
        bool tgl { get; set; }        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            thisView = inflater.Inflate(Resource.Layout.transactionslayout, container, false);
            fromAddress.EditText.Text = MainActivity.BitcoinPubKeyAddress;
            transAdapter = new TranscactionsRecycler(new WalletsClass[0]);
            var layoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.Vertical);
            recycleList.SetLayoutManager(layoutManager);
            recycleList.SetAdapter(transAdapter);
            send.Click += Send_Click1;
            toggle.Click += RecycleList_Click;
            return thisView;
        }

        private void Send_Click1(object sender, EventArgs e)
        {
            if (amounttoSend.EditText.Text != string.Empty)
            {
                TransferFunds(MainActivity.BitcoinPubKeyAddress, 
                    (decimal)0.002, "mpjVqufQkDLYgrZwr8Ro82prrpKsJumNKd",
                    true ,
                    out WalletsClass rObjec, 
                    out bool success ,
                    out Transaction transcation);
                if (success && transcation != null)
                {
                    using (var node = Node.ConnectToLocal(MainActivity.net))
                    {
                        node.VersionHandshake();
                        node.SendMessage(new InvPayload(InventoryType.MSG_TX, transcation.GetHash()));
                        node.SendMessage(new TxPayload(transcation));
                        Thread.Sleep(500);
                    }
                }
            }
        }

        private async void GetMinerFee()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json"); string responseString;
                    JObject response = null;
                    var Uriurl = new Uri($"https://bitcoinfees.21.co/api/v1/fees/recommended");
                    var responseBytes = await client.DownloadDataTaskAsync(Uriurl);
                    responseString = Encoding.UTF8.GetString(responseBytes);
                    response = JObject.Parse(responseString);
            }

        }
        public class WalletsClass
        {
            public string wallet { get; set; }
            public string address { get; set; }
            public string privatekey { get; set; }
            public string source { get; set; }
            public string destination { get; set; }
            public decimal amount { get; set; }
            public string txID { get; set; }
            public string type { get; set; }
        }
        WalletsClass walletObject;
     }
}
