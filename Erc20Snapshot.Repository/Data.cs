namespace BilbolStack.Erc20Snapshot.Repository
{
    public class Data
    {
        public long BlockNumber { get; set; }
        public List<ERC20Transfer> ERC20Transfers { get; set; }
        public List<ERC20Balance> Balance { get; set; }
    }


    public class ERC20Transfer
    {
        public long BlockNumber { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Amount { get; set; }
        public string TX { get; set; }
    }

    public class ERC20Balance
    {
        public string Owner { get; set; }
        public string Amount { get; set; }
    }
}
