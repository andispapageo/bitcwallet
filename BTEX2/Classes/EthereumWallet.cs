using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BitCWallet.Pages
{
    class EthereumWallet
    {
        public void CreateAccount()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes();
        }

        private async void AdjustEthereumAddress()
        {
            var url = "https://ropsten.infura.io/v3/4611f97dd5e14d8a96bc6633b12890fc";
            var privateKey = "0x730CF958C26497F518d19B6233ce9d21c67ED6C1";
            var account = new Account(privateKey);
            var web3 = new Web3(account, url);
            try
            {
                var deploymentMessage = new StandardTokenDeployment{ TotalSupply = 6500000000 };
                var deploymentHandler = web3.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
                var transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);
                var contractAddress = transactionReceipt.ContractAddress;
            }
            catch (Exception e)
            {

            }

            finally
            {
                var balanceOfFunctionMessage = new BalanceOfFunction() { Owner = account.Address, };
                var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                var balance = await balanceHandler.QueryAsync<BigInteger>(privateKey, balanceOfFunctionMessage);
                var balances = await web3.Eth.GetBalance.SendRequestAsync("0x730CF958C26497F518d19B6233ce9d21c67ED6C1");

                Console.WriteLine($"Balance in Wei: {balances.Value}");
                var etherAmount = Web3.Convert.FromWei(balances.Value);
            }
        }

        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            [Parameter("address", "_from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "_to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 3, false)]
            public BigInteger Value { get; set; }
        }

        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger TokenAmount { get; set; }
        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {
            [Parameter("address", "_owner", 1)]
            public string Owner { get; set; }
        }

        public class StandardTokenDeployment : ContractDeploymentMessage
        {
            public static string BYTECODE = "0x608060405234801561001057600080fd5b5061013f806100206000396000f3fe608060405260043610610041576000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff168063ef5fb05b14610046575b600080fd5b34801561005257600080fd5b5061005b6100d6565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561009b578082015181840152602081019050610080565b50505050905090810190601f1680156100c85780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b60606040805190810160405280600b81526020017f68656c6c6f20776f726c6400000000000000000000000000000000000000000081525090509056fea165627a7a72305820317ef57f6b6c28602aa7b97562ddddbf858ccb2452746cde57c639e00ce98dce0029";
            public StandardTokenDeployment() : base(BYTECODE) { }
            [Parameter("uint256", "totalSupply")]
            public BigInteger TotalSupply { get; set; }
        }

        public class MakerTokenConvertor
        {

            private const long MakerMeiUnitValue = 1000000000000000000;
            private int CalculateNumberOfDecimalPlaces(decimal value, int currentNumberOfDecimals = 0)
            {
                decimal multiplied = (decimal)((double)value * Math.Pow(10, currentNumberOfDecimals));
                if (Math.Round(multiplied) == multiplied) return currentNumberOfDecimals;
                return CalculateNumberOfDecimalPlaces(value, currentNumberOfDecimals + 1);
            }

            public BigInteger ConvertToMei(decimal makerAmount)
            {
                var decimalPlaces = CalculateNumberOfDecimalPlaces(makerAmount);
                if (decimalPlaces == 0) return BigInteger.Multiply(new BigInteger(makerAmount), MakerMeiUnitValue);

                var decimalConversionUnit = (decimal)Math.Pow(10, decimalPlaces);

                var makerAmountFromDec = new BigInteger(makerAmount * decimalConversionUnit);
                var meiUnitFromDec = new BigInteger(MakerMeiUnitValue / decimalConversionUnit);
                return makerAmountFromDec * meiUnitFromDec;
            }

            public decimal ConvertFromMei(BigInteger meiAmount)
            {
                return (decimal)meiAmount / MakerMeiUnitValue;
            }
        }

        public class EthereumContractInfo
        {
            public string Abi { get; set; }
            public string Bytecode { get; set; }
            public string TransactionHash { get; set; }
            public string ContractAddress { get; set; }

            public EthereumContractInfo(string name, string abi, string bytecode, string transactionHash)
            {

                Abi = abi;
                Bytecode = bytecode;
                TransactionHash = transactionHash;
            }
        }
    }
}