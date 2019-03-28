using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;

namespace BitCWallet
{
    public class Comms
    {
        internal string NetWorkType { get; set; }
        internal string ConnectionType { get; set; }
        public static implicit operator string(Comms p) => p.NetWorkType;
    }

    public abstract class NetState
    {
        public abstract bool WifiState(Activity _);
    }
    class Abstract_Net : NetState
    {
        Comms cms = new Comms();
        ConnectivityType ActiveInternetConnectionType;
        public static bool wifistatesarray;
        public override bool WifiState(Activity _)
        {
            try
            {
                var cm = (ConnectivityManager)_.GetSystemService(Context.ConnectivityService);
                if (null == _ || cm == null) return false;
                if (cm.ActiveNetworkInfo != null)
                {
                    if (cm.ActiveNetworkInfo.IsAvailable)
                    {
                        if (ActiveInternetConnectionType != cm.ActiveNetworkInfo.Type)
                        {
                            ActiveInternetConnectionType = cm.ActiveNetworkInfo.Type;
                            cms.NetWorkType = ActiveInternetConnectionType.ToString();
                        }
                        wifistatesarray = cm.ActiveNetworkInfo.IsConnected;
                    }
                    else wifistatesarray = false;
                }
            }
            catch (Exception ex)
            {
             
            }
            return wifistatesarray;
        }
    }
}