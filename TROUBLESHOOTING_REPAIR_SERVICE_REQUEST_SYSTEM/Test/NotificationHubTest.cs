using System;
using System.Dynamic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Tests.Hubs
{
    [TestClass]
    public class NotificationHubTests
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
        public void NotifyRecipient_TargetsCorrectGroup_AndCallsNewNotification()
        {
            var called = false;
            var capturedGroup = string.Empty;

            dynamic groupClient = new ExpandoObject();
            groupClient.newNotification = (Action)(() => called = true);

            ConfigureHubContextWithGroupClient(
                groupClient,
                (Action<string>)(g => capturedGroup = g)
            );

            NotificationHub.RefreshNotificationList(123);

            Assert.AreEqual("recipient:123", capturedGroup);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RefreshNotificationBadge_TargetsCorrectGroup_AndCallsRefreshNotificationBadge()
        {
            var called = false;
            var capturedGroup = string.Empty;

            dynamic groupClient = new ExpandoObject();
            groupClient.refreshNotificationBadge = (Action)(() => called = true);

            ConfigureHubContextWithGroupClient(
                groupClient,
                (Action<string>)(g => capturedGroup = g)
            );

            NotificationHub.RefreshNotificationBadge(456);

            Assert.AreEqual("recipient:456", capturedGroup);
            Assert.IsTrue(called);
        }

        private static void ConfigureHubContextWithGroupClient(dynamic groupClient, Action<string> onGroupCaptured)
        {
            var mockClients = new Mock<IHubConnectionContext<dynamic>>();
            mockClients
                .Setup(c => c.Group(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns((string groupName, string[] _) =>
                {
                    onGroupCaptured(groupName);
                    return groupClient;
                });

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