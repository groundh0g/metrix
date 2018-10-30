using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using MetrixWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MatrixTests2
{
    [TestClass]
    public class UnitTest1
    {
        private MetrixHttpModule TheHttpModule { get; set; }
        private HttpApplication TheApplication { get; set; }
        private Mock<HttpRequestBase> MockRequest { get; set; }
        private Mock<HttpResponseBase> MockResponse { get; set; }
        private Mock<HttpContextBase> MockContext { get; set; }

        private MemoryStream MemoryStream { get; set; }

        public void SetupMocks()
        {
            TheApplication = new HttpApplication();
            MockRequest = new Mock<HttpRequestBase>();
            MockResponse = new Mock<HttpResponseBase>();
            MockContext = new Mock<HttpContextBase>();

            var memStream = MemoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memStream);
            MockResponse.SetupProperty(m => m.Filter, streamWriter.BaseStream);
            MockResponse.SetupGet(m => m.ContentType).Returns("text/html");

            var items = new Dictionary<string, object>();
            MockContext.SetupGet(m => m.Items).Returns(items);
            MockContext.Setup(m => m.Response).Returns(MockResponse.Object);
            MockContext.Setup(m => m.Request).Returns(MockRequest.Object);
            MockContext.Setup(m => m.ApplicationInstance).Returns(TheApplication);

            ////            //var mockApplication = Mock.Get(TheApplication);
            ////            var mockApplication = TheApplication;
            //////            mockApplication.SetupGet(m => m.Context).Returns(TheContext.Object);
            ////            mockApplication.Setup(m => m.Context).Returns(TheContext.Object.ApplicationInstance.Context);
            //            typeof(HttpApplication)
            //                .GetField("context", BindingFlags.Instance | BindingFlags.NonPublic)
            //                .SetValue(TheApplication, new HttpContextWrapper(TheContext.Object));

            TheHttpModule = new MetrixHttpModule();
            TheHttpModule.GetRequest = (object sender) => MockRequest.Object;
            TheHttpModule.GetResponse = (object sender) => MockResponse.Object;
            TheHttpModule.GetContext = (object sender) => MockContext.Object;

            TheHttpModule.Init(TheApplication);

            // TODO: not thrilled with this ... need to figure out which property hosts the Write calls
            MockResponse.SetupGet(m => m.Filter).Returns(TheHttpModule.CountingStream);
        }

        [TestMethod]
        public void BeginRequest_LogsStartTimes()
        {
            SetupMocks();

            TheHttpModule.ContextOnBeginRequest(TheApplication, EventArgs.Empty);

            Assert.IsFalse(MockContext.Object.Items.Count == 0);
            var metrixData = MockContext.Object.Items[MetrixHttpModule.METRIX_REQUEST_DATA] as MetrixRequestData;
            Assert.IsFalse(metrixData.StartRequest == 0);
            Assert.IsFalse(metrixData.StartRequestWork == 0);
            Assert.IsFalse(Guid.Empty.Equals(metrixData.RequestGuid));
        }

        [TestMethod]
        public void EndRequest_LogsEndTimes()
        {
            SetupMocks();

            TheHttpModule.ContextOnBeginRequest(TheApplication, EventArgs.Empty);
            TheHttpModule.ContextOnEndRequest(TheApplication, EventArgs.Empty);

            Assert.IsFalse(MockContext.Object.Items.Count == 0);
            var metrixData = MockContext.Object.Items[MetrixHttpModule.METRIX_REQUEST_DATA] as MetrixRequestData;
            Assert.IsFalse(metrixData.EndRequest == 0);
            Assert.IsFalse(metrixData.EndRequestWork == 0);
        }

        [TestMethod, Ignore("Unable to properly mock full request/response cycle. TODO: revisit")]
        public void EndRequest_LogsResponseLength()
        {
            SetupMocks();

            TheHttpModule.ContextOnBeginRequest(TheApplication, EventArgs.Empty);
            MockResponse.Object.Write("This string has 30 characters.");
            TheHttpModule.ContextOnEndRequest(TheApplication, EventArgs.Empty);

            Assert.IsFalse(MockContext.Object.Items.Count == 0);
            var metrixData = MockContext.Object.Items[MetrixHttpModule.METRIX_REQUEST_DATA] as MetrixRequestData;
            Assert.AreEqual(30L, metrixData.OutputLength);
        }
    }
}
