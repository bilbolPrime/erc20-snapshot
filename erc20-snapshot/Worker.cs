using BilbolStack.Erc20Snapshot.Chain;

namespace erc20_snapshot
{
    public class Worker : BackgroundService
    {
        private IERC20ContractScraper _erc20ContractScraper;
        public Worker(IERC20ContractScraper nftContractScraper)
        {
            _erc20ContractScraper = nftContractScraper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(15000, stoppingToken);
                try
                {
                    await _erc20ContractScraper.CheckChange();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(60000);
                }
            }
        }
    }
}
