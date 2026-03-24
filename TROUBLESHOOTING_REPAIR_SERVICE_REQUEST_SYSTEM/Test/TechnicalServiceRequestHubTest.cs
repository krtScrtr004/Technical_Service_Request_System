using System;
using System.Dynamic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Tests.Hubs
{
    [TestClass]
    public class TechnicalServiceRequestHubTests
    {
        private IDependencyResolver _originalResolver;

        [TestInitialize]
        public void Setup()
        {
            _originalResolver = GlobalHost.DependencyResolver;
        }

        [TestCleanup]
        public void Cleanup()
        {
            GlobalHost.DependencyResolver = _originalResolver;
        }

        [TestMethod]
        public void RefreshTechnicalServiceRequestList_CallsClientMethod()
        {
            var called = false;

            dynamic allClient = new ExpandoObject();
            allClient.refreshTechnicalServiceRequestList = (Action)(() => called = true);

            ConfigureHubContext(allClient);

            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RefreshTechnicalServiceRequestSeverity_PassesSeverityName()
        {
            var actualSeverity = string.Empty;
            var expectedSeverity = "High";

            dynamic allClient = new ExpandoObject();
            allClient.refreshTechnicalServiceRequestSeverity = (Action<string>)(s => actualSeverity = s);

            ConfigureHubContext(allClient);

            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestSeverity(expectedSeverity);

            Assert.AreEqual(expectedSeverity, actualSeverity);
        }

        [TestMethod]
        public void RefreshTechnicalServiceRequestStatus_PassesStatusName()
        {
            var actualStatus = string.Empty;
            var expectedStatus = "Completed";

            dynamic allClient = new ExpandoObject();
            allClient.refreshTechnicalServiceRequestStatus = (Action<string>)(s => actualStatus = s);

            ConfigureHubContext(allClient);

            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestStatus(expectedStatus);

            Assert.AreEqual(expectedStatus, actualStatus);
        }

        [TestMethod]
        public void RefreshTechnicalServiceRequestActionHistory_PassesHistoryObject()
        {
            TechnicalServiceRequestHistory received = null;
            var expected = new TechnicalServiceRequestHistory
            {
                Id = 99,
                ActionTaken = "Dummy action",
                DateAction = DateTime.Now
            };

            dynamic allClient = new ExpandoObject();
            allClient.refreshTechnicalServiceRequestActionHistory =
                (Action<TechnicalServiceRequestHistory>)(h => received = h);

            ConfigureHubContext(allClient);

            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestActionHistory(expected);

            Assert.IsNotNull(received);
            Assert.AreEqual(expected.Id, received.Id);
            Assert.AreEqual(expected.ActionTaken, received.ActionTaken);
        }

        private static void ConfigureHubContext(dynamic allClient)
        {
            var mockClients = new Mock<IHubConnectionContext<dynamic>>();
            mockClients.Setup(c => c.All).Returns(allClient);

            var mockHubContext = new Mock<IHubContext>();
            mockHubContext.SetupGet(c => c.Clients).Returns(mockClients.Object);

            var mockConnectionManager = new Mock<IConnectionManager>();
            mockConnectionManager
                .Setup(m => m.GetHubContext(It.IsAny<string>()))
                .Returns(mockHubContext.Object);

            var resolver = new DefaultDependencyResolver();
            resolver.Register(typeof(IConnectionManager), () => mockConnectionManager.Object);
            GlobalHost.DependencyResolver = resolver;
        }
    }
}