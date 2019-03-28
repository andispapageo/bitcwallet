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

         //   GetMinerFee();
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

       
        private void RecycleList_Click(object sender, EventArgs e)
        {
            tgl = !tgl;
            if (tgl)
            {
                QBitNinjaClient client = new QBitNinjaClient(MainActivity.net);
             
                foreach (var txID in BitCoinWallet.TransactionsID)
                {
                    
                    var transactionId = uint256.Parse(txID);
                  
                    GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;
                    var s = transactionResponse.Transaction.ToString();
                    List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
                    Transaction transaction = transactionResponse.Transaction;

                    foreach (var coin in receivedCoins)
                    {
                        Money amount = (Money)coin.Amount;
                        Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                        var paymentScript = coin.TxOut.ScriptPubKey;

                        Console.WriteLine(paymentScript);
                        var address = paymentScript.GetDestinationAddress(MainActivity.net);
                        Console.WriteLine(address);
                    }
                    var outputs = transaction.Outputs;
                    foreach (TxOut output in outputs)
                    {
                        Money amount = output.Value;

                        Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                        var paymentScript = output.ScriptPubKey;
                        Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                        var address = paymentScript.GetDestinationAddress(Network.Main);
                        Console.WriteLine(address);
                        Console.WriteLine();
                    }
                }
            }
            else
            {

            }
        }

        private async void GetMinerFee()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                string responseString;

                try
                {
                    JObject response = null;
                    var Uriurl = new Uri($"https://bitcoinfees.21.co/api/v1/fees/recommended");
                    var responseBytes = await client.DownloadDataTaskAsync(Uriurl);
                    responseString = Encoding.UTF8.GetString(responseBytes);
                    response = JObject.Parse(responseString);

                }
                catch (WebException e)
                {

                }
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

        public void TransferFunds(string ssPrivateKey, 
            decimal ValuetoTransfer, 
            string ssToAddress_ClientB,
            bool found_orRefund,
            out WalletsClass walletObject,
            out bool success,
            out Transaction transcation)
        {

            success = false;
            transcation = null;
            decimal addBlnc = 0;
            decimal addBlnceConf = 0;
            string tid = string.Empty;
            walletObject = new WalletsClass();

            BitcoinSecret bitcoinPrivateKey = new BitcoinSecret(ssPrivateKey);
            var network = bitcoinPrivateKey.Network;
            var fromAddress = bitcoinPrivateKey.GetAddress().ToString();
            var client = new QBitNinjaClient(network);

            var transactionId = uint256.Parse(tid);
            var transactionResponse = client.GetTransaction(transactionId).Result;
          
            OutPoint outPointToSpend = null;
            foreach (var coin in transactionResponse.ReceivedCoins)
            {
                if (Equals(coin.TxOut.ScriptPubKey, bitcoinPrivateKey.ScriptPubKey))
                {
                    outPointToSpend = coin.Outpoint;
                }
            }

            var transfer = Transaction.Create(MainActivity.net);
            transfer.Inputs.Add(new TxIn() { PrevOut = outPointToSpend });

            var toAddress = BitcoinAddress.Create(ssToAddress_ClientB, MainActivity.net);

            var ValuetoTransferConv = new Money(ValuetoTransfer, MoneyUnit.BTC);
            var minerFee = new Money(0.0002m, MoneyUnit.BTC);
            //tximanout == 0.001  - 0.0002 -0.0002
            var txInAmount = (Money)transactionResponse.ReceivedCoins[(int)outPointToSpend.N].Amount;
            var changeAmount = txInAmount.ToDecimal(MoneyUnit.BTC) - ValuetoTransferConv.ToDecimal(MoneyUnit.BTC) - minerFee.ToDecimal(MoneyUnit.BTC);
            if (changeAmount > 0)
            {
                TxOut valuetoAddress_B = new TxOut() { Value = new Money(ValuetoTransfer, MoneyUnit.BTC), ScriptPubKey = toAddress.ScriptPubKey };
                TxOut changeTxOut = new TxOut() { Value = new Money(changeAmount, MoneyUnit.BTC), ScriptPubKey = bitcoinPrivateKey.ScriptPubKey };
                transfer.Outputs.Add(valuetoAddress_B);
                transfer.Outputs.Add(changeTxOut);
            }
            else
            {
                MainActivity.act.RunOnUiThread(() =>
                {
                    Toast.MakeText(MainActivity.act, "The total amount cannot be negative" +
                    " Please add a smaller amount to total or miner fee", ToastLength.Long).Show();
                });
                return;
            }
            transfer.Sign(bitcoinPrivateKey, transactionResponse.ReceivedCoins.ToArray());
            BroadcastResponse brResp = client.Broadcast(transfer).Result;

            if (!brResp.Success)
            {
                success = false;
                MainActivity.act.RunOnUiThread(() =>
                {
                    Toast.MakeText(MainActivity.act, "BroadCastResponse Error" + brResp.Error.ErrorCode + " : " + brResp.Error.Reason, ToastLength.Long).Show();
                });
                transcation = null;
                return;
            }
            else
            {
                walletObject.source = found_orRefund ? "A" : "B";
                walletObject.destination = found_orRefund ? "B" : "A";
                walletObject.type = found_orRefund ? "Fund" : "Refund";
                walletObject.amount = ValuetoTransfer;

                success = brResp.Success;
                transcation = transfer;
            }
        }
    }
}
