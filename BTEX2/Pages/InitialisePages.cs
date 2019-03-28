
using System;

namespace BitCWallet.Pages
{
    class InitialisePages
    {
        public static APIPage mAPIPage { get; set; }
        public static WalletsPage mWalletsPage { get; set; }
        public static TransactionsPage mTransactionsPage { get; set; }
        public bool set { get; set; }
        public InitialisePages()
        {
            try
            {
                mAPIPage = new APIPage();
                mWalletsPage = new WalletsPage();
                mTransactionsPage = new TransactionsPage();
            }
            catch (Exception e) { set = false; return; }
            finally { set = true; }
        }
    }
}