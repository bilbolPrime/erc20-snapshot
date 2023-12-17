using BilbolStack.Erc20Snapshot.Repository;
using BilbolStack.ERC20Snapshot.Media;
using Microsoft.Extensions.Options;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;

namespace BilbolStack.Erc20Snapshot.Chain
{
    public class ERC20ContractScraper : IERC20ContractScraper
    {
        protected Web3 _web3;
        protected string _contractAddress;
        protected Account _account;

        private IERC20Repository _erc20Repository;
        private IMediaManager _mediaManager;
        private List<string> _ignore;
        private List<string> _sale;
        private long _minBlock;
        private string _ticker;
        private BigInteger _threshold;

        public ERC20ContractScraper(IOptions<ChainSettings> chainSettings, IERC20Repository erc20Repository, IMediaManager mediaManager)
        {
            _account = new Account(chainSettings.Value.AccountPrivateKey, chainSettings.Value.ChainId);
            _web3 = new Web3(_account, chainSettings.Value.RpcUrl);
            _web3.TransactionManager.UseLegacyAsDefault = true;
            _contractAddress = chainSettings.Value.NFTContractAddress;
            _erc20Repository = erc20Repository;
            _ignore = chainSettings.Value.AddressesToIgnore.Select(i => i.ToLower()).ToList();
            _sale = chainSettings.Value.AddressesOfSale.Select(i => i.ToLower()).ToList();
            _minBlock = chainSettings.Value.NFTContractAddressDeployedOnBlock;
            _mediaManager = mediaManager;
            _ticker = chainSettings.Value.Ticker;
            _threshold = BigInteger.Parse(chainSettings.Value.Threshold);
        }

        public async Task CheckChange()
        {
            var data = _erc20Repository.GetData();
            var startBlock = (int) data.BlockNumber;
            if(data.BlockNumber == 0)
            {
                startBlock = (int) _minBlock;
            }
            
            var latestBlockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var endBlock = Math.Min(startBlock + 50000, latestBlockNumber.ToLong());

            if (startBlock > latestBlockNumber.Value.ToHexBigInteger().ToLong())
            {
                return;
            }

            var transfers = new List<ERC20Transfer>();

            {
                var contractTransferEvent = _web3.Eth.GetEvent<TransferDTO>(_contractAddress);
                var filterInput = contractTransferEvent.CreateFilterInput(new BlockParameter(startBlock.ToHexBigInteger()), new BlockParameter(((int)endBlock).ToHexBigInteger()));
                var transferEvents = await contractTransferEvent.GetAllChangesAsync(filterInput);
                foreach (var transferEvent in transferEvents)
                {
                    transfers.Add(new ERC20Transfer() { BlockNumber = transferEvent.Log.BlockNumber.ToLong(), From = transferEvent.Event.By, To = transferEvent.Event.To, Amount = transferEvent.Event.Value.ToString(), TX = transferEvent.Log.TransactionHash });
                }
            }

            _erc20Repository.SaveData(data, endBlock + 1, transfers);

            try
            {
                foreach(var transfer in transfers.Where(i => i.BlockNumber > latestBlockNumber.ToLong() - 100 && _threshold <= BigInteger.Parse(i.Amount)).OrderBy(i => i.BlockNumber))
                {
                    if (_ignore.Any(i => i == transfer.From.ToLower() || i == transfer.To.ToLower()))
                    {
                        continue;
                    }

                    var message = string.Empty;
                    var amountInEth = BigInteger.Divide(BigInteger.Parse(transfer.Amount), new BigInteger(1000_000_000_000_000_000)).ToString("N0");
                    if (_sale.Any(i => i == transfer.From.ToLower() || i == transfer.To.ToLower()))
                    {
                        message = _sale.Any(i => i == transfer.From.ToLower()) ? $"{transfer.To} bought {amountInEth} {_ticker}" : $"{transfer.From} sold {amountInEth} {_ticker}";

                    }
                    else
                    {
                        message = $"{transfer.From} sent {amountInEth} {_ticker} to {transfer.To}";
                    }

                    _mediaManager.Send($"Block {transfer.BlockNumber}\n{message}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
