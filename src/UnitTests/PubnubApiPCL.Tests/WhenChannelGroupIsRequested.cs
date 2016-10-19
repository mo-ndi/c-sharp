﻿using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenChannelGroupIsRequested : TestHarness
    {
        private static ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedChannelGroupMessage = false;
        private static bool receivedGrantMessage = false;

        private static string currentUnitTestCase = "";
        private static string channelGroupName = "hello_my_group";
        private static string channelName = "hello_my_channel";
        private static string authKey = "myAuth";

        private static Pubnub pubnub = null;
        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "ytgdyeV_yhD_a8KRyNsUaumaW4h70SWIsiHuuKE39Fw=")
                    .WithParameter("channel-group", channelGroupName)
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().ChannelGroups(new string[] { channelGroupName }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new GrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenAddChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "GxvqczR2FCHioz2nUPDUnv2qpnKUB9O_hFOW798_BFQ=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName }).ChannelGroup(channelGroupName).Async(new ChannelGroupAddChannelResult());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenRemoveChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";
            string channelName = "hello_my_channel";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("remove", channelName)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "GxvqczR2FCHioz2nUPDUnv2qpnKUB9O_hFOW798_BFQ=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.RemoveChannelsFromChannelGroup().Channels(new string[] { channelName }).ChannelGroup(channelGroupName).Async(new ChannelGroupRemoveChannel());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();

            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetChannelListShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"channels\": [\"" + channelName + "\"], \"group\": \"" + channelGroupName + "\"}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroupName).Async(new ChannelGroupAllChannels());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetAllChannelGroupShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetAllChannelGroupShouldReturnSuccess";

            if (PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM is enabled; WhenChannelGroupIsRequested -> ThenGetAllChannelGroupShouldReturnSuccess.");
                return;
            }

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"namespace\": \"\", \"groups\": [\"" + channelGroupName + "\", \"hello_my_group1\"]}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group", PubnubCommon.SubscribeKey))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.ListChannelGroups().Async(new ChannelGroupAll());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        private class GrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    if (status.Error == false)
                    {
                        receivedGrantMessage = true;
                    }

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                    }
                }
                catch
                { }
                finally
                {
                    grantManualEvent.Set();
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAddChannelResult : PNCallback<PNChannelGroupsAddChannelResult>
        {
            public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        //if (status.StatusCode == 200 && result.Message.ToLower() == "ok" && result.Service == "channel-registry"&& status.Error == false && result.ChannelGroup.Substring(1) == channelGroupName)
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupRemoveChannel : PNCallback<PNChannelGroupsRemoveChannelResult>
        {
            public override void OnResponse(PNChannelGroupsRemoveChannelResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && result.Message.ToLower() == "ok" && result.Service == "channel-registry" && status.Error == false && result.ChannelGroup.Substring(1) == channelGroupName)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAllChannels : PNCallback<PNChannelGroupsAllChannelsResult>
        {
            public override void OnResponse(PNChannelGroupsAllChannelsResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false && result.ChannelGroup==channelGroupName && result.Channels.Count>0)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAll : PNCallback<PNChannelGroupsListAllResult>
        {
            public override void OnResponse(PNChannelGroupsListAllResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }
    }
}
