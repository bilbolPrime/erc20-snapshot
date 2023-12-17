namespace BilbolStack.Erc20Snapshot.Chain
{
    public class ChainSettings
    {
        public const string ConfigKey = "ChainInfo";
        public string AccountPrivateKey { get; set; }
        public long ChainId { get; set; }
        public string RpcUrl { get; set; }
        public string NFTContractAddress { get; set; }
        public string Ticker { get; set; }
        public string Threshold {  get; set; }
        public long NFTContractAddressDeployedOnBlock { get; set; }
        public List<string> AddressesToIgnore { get; set; }
        public List<string>  AddressesOfSale { get; set; }
    }
}
