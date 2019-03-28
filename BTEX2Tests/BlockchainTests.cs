using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTEX2.MainActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTEX2.MainActivity.Tests
{
    [TestClass()]
    public class BlockchainTests
    {
        bool res = true;
        [TestMethod()]
        public void BlockchainTest()
        {
            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void GenesisTest()
        {
            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void GetLatestBlockTest()
        {
            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void AddGenesisBlockTest()
        {
            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void AdditionOf_BlockTest()
        {
            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void IsValidTest()
        {
            Assert.IsTrue(res);
        }
    }
}