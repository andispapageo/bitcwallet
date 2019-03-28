using BitCWallet.Pages;
namespace BitCWallet.Models
{
    class WalletsSchema : Java.Lang.Object
    {
        public string WalletsMarket { get; set; }
        public string BallanceMarket { get; set; }
        public string BalanceUSD { get; set; }
        public int imageId { get; set; }
        public WalletsPage.Wallets id { get; internal set; }
    }
}