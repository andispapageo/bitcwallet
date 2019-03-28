using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using BitCWallet.Adapters;
using Newtonsoft.Json;
using static BitCWallet.Adapters.ApiAdaptor;

namespace BitCWallet.Pages
{
    public class APIPage : Fragment
    {
        ApiAdaptor apiAdaptor;
        View thisVIew { get; set; }
        RecyclerView recycler { get { return thisVIew?.FindViewById(Resource.Id.recyclerAPI) as RecyclerView; } }
        RootObject root { get; set; }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            apiAdaptor = new ApiAdaptor(new List<Result>());
            RequestGet("https://bittrex.com/api/v1.1/public/getmarkets");
        }

        private void RequestGet(string url)
        {
                if (MainActivity.WIFI)
                {
                    if (url == string.Empty) return;
                    Uri uri = new Uri(url);
                    var json = new WebClient();
                    var res = json.DownloadString(uri);
                    root = JsonConvert.DeserializeObject<RootObject>(res);
                    if (root.Success && apiAdaptor != null)
                        apiAdaptor?.Update(root.Result
                            .OrderBy(x => x.BaseCurrencyLong)
                            .ThenBy(x => x.MarketCurrencyLong).ToList());
                }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            thisVIew  = inflater.Inflate(Resource.Layout.apiPage, container, false);
            var layoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.Vertical);
            recycler.SetLayoutManager(layoutManager);
            recycler.SetAdapter(apiAdaptor);
            recycler.Elevation = 21f;
            return thisVIew;
        }

        public Task<string> GetUSD(string MarketName)
        {
            if (root == null || root.Result == null) return Task.FromResult(string.Empty);
            return Task.FromResult((from n in root.Result 
            where n.MarketName 
            == MarketName select n.MinTradeSize)
            .FirstOrDefault());
        }
    }
}
