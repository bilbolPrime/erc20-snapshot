namespace BilbolStack.Erc20Snapshot.Repository
{
    public interface IERC20Repository
    {
        Data GetData();
        void SaveData(Data data, long blockNumber, List<ERC20Transfer> transfers);
        Stream GenerateReport(long? untilBlock, out long blockNumber);
    }
}
