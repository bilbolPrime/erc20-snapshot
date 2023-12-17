using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace BilbolStack.Erc20Snapshot.Chain
{
    [Event("Transfer")]
    public class TransferDTO : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public string By { get; set; }

        [Parameter("address", "to", 2, true)]
        public string To { get; set; }
        [Parameter("uint256", "value", 3, false)]
        public BigInteger Value { get; set; }
    }
}
