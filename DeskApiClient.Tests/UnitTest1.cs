using DeskApiClient;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeskApiClient.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void DeskApiClientTest()
        {
            var deskApiClient = new DeskApiClient();
            Assert.IsNotNull(deskApiClient);
        }

        [TestMethod()]
        public void DeskApiClientTest1()
        {
            var deskApiClient = new DeskApiClient("https://test.desk.com/api/v2", "username", "password");
            Assert.IsNotNull(deskApiClient);
        }

        [TestMethod()]
        public void DeskApiClientTest2()
        {

           var deskApiClient = new DeskApiClient("https://test.desk.com/api/v2", "ApiToken", "ApiSecret", "ApiToken", "ApiTokenSecret");
           Assert.IsNotNull(deskApiClient);
        }

        [TestMethod()]
        public void ExecuteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CallTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CallTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FormatDateForApiTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsExceedingApiLimitsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsNearingApiLimitsTest()
        {
            Assert.Fail();
        }
    }
}