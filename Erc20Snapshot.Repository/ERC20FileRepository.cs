using Newtonsoft.Json;
using System.IO.Compression;
using System.Numerics;
using System.Text;

namespace BilbolStack.Erc20Snapshot.Repository
{
    public class ERC20FileRepository : IERC20Repository
    {
        private const string _dataFile = "data.txt";

        private static object _lockObj = new object();

        public Stream GenerateReport(long? untilBlock, out long block)
        {
            if (untilBlock.HasValue)
            {
                block = untilBlock.Value;
                return GenerateReport(untilBlock.Value);
            }

            var data = GetData();
            block = data.BlockNumber;
            lock (_lockObj)
            {
                var balances = data.Balance;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Wallet,Wei");
                foreach(var balance in balances.Where(i => i.Amount != "0"))
                {
                    stringBuilder.AppendLine($"{balance.Owner},{balance.Amount}");
                }

                File.WriteAllText("balances.csv", stringBuilder.ToString());

                stringBuilder.Clear();

                var transfers = data.ERC20Transfers;
                stringBuilder.AppendLine("Block,From,To,Wei,TX");
                foreach (var transfer in transfers.OrderBy(i => i.BlockNumber))
                {
                    stringBuilder.AppendLine($"{transfer.BlockNumber},{transfer.From},{transfer.To},{transfer.Amount},{transfer.TX}");
                }

                File.WriteAllText("transfers.csv", stringBuilder.ToString());

                stringBuilder.Clear();

                var memoryStream = new MemoryStream();
                var files = new List<DownloadFile>();
                files.Add(new DownloadFile() { Name = "balances.csv", FileContent = File.ReadAllBytes("balances.csv") });
                files.Add(new DownloadFile() { Name = "transfers.csv", FileContent = File.ReadAllBytes("transfers.csv") });

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var entry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        entryStream.WriteAsync(file.FileContent, 0, file.FileContent.Length);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }

        public Data GetData()
        {
            lock (_lockObj)
            {
                return File.Exists(_dataFile) ? JsonConvert.DeserializeObject<Data>(File.ReadAllText(_dataFile)) : new Data() { Balance = new List<ERC20Balance>(), ERC20Transfers = new List<ERC20Transfer>() };
            }
        }

        public void SaveData(Data data, long blockNumber, List<ERC20Transfer> transfers)
        {
            lock (_lockObj)
            {
                data.BlockNumber = blockNumber;
                AddTransfers(data, transfers);
                File.WriteAllText(_dataFile, JsonConvert.SerializeObject(data));
            }
        }

        private void AddTransfers(Data data, List<ERC20Transfer> transfers)
        {
            data.ERC20Transfers.AddRange(transfers);
            foreach(var transfer in transfers.OrderBy(i => i.BlockNumber))
            {
                if (transfer.From != "0x0000000000000000000000000000000000000000")
                {
                    var fromBalance = data.Balance.FirstOrDefault(i => i.Owner == transfer.From);
                    if (fromBalance != null)
                    {
                        fromBalance.Amount = BigInteger.Subtract(BigInteger.Parse(fromBalance.Amount), BigInteger.Parse(transfer.Amount)).ToString();
                    }
                    else
                    {
                        throw new Exception($"Could not find {transfer.From}");
                    }
                }

                var toBalance = data.Balance.FirstOrDefault(i => i.Owner == transfer.To);
                if (toBalance != null)
                {
                    toBalance.Amount = BigInteger.Add(BigInteger.Parse(toBalance.Amount), BigInteger.Parse(transfer.Amount)).ToString();
                }
                else
                {
                    data.Balance.Add(new ERC20Balance() { Amount = transfer.Amount, Owner = transfer.To });
                }
            }
        }

        private Stream GenerateReport(long untilBlock)
        {
            var data = new Data() { BlockNumber = untilBlock, Balance = new List<ERC20Balance>(), ERC20Transfers = new List<ERC20Transfer>() };
            AddTransfers(data, GetData().ERC20Transfers.Where(i => i.BlockNumber <= untilBlock).ToList());
            data.BlockNumber = untilBlock;
            
            var files = new List<DownloadFile>();
            var balances = data.Balance;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Wallet,Wei");
            foreach (var balance in balances.Where(i => i.Amount != "0"))
            {
                stringBuilder.AppendLine($"{balance.Owner},{balance.Amount}");
            }



            files.Add(new DownloadFile() { Name = "balances.csv", FileContent = Encoding.ASCII.GetBytes(stringBuilder.ToString()) });

            stringBuilder.Clear();

            var transfers = data.ERC20Transfers;
            stringBuilder.AppendLine("Block,From,To,Wei,TX");
            foreach (var transfer in transfers.OrderBy(i => i.BlockNumber))
            {
                stringBuilder.AppendLine($"{transfer.BlockNumber},{transfer.From},{transfer.To},{transfer.Amount},{transfer.TX}");
            }

            files.Add(new DownloadFile() { Name = "transfers.csv", FileContent = Encoding.ASCII.GetBytes(stringBuilder.ToString()) });

            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    entryStream.WriteAsync(file.FileContent, 0, file.FileContent.Length);
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }


        class DownloadFile
        {
            public string Name { get; set; }
            public byte[] FileContent { get; set; }
        }
    }
}
