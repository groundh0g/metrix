using System;
using System.Diagnostics;
using System.Web;

namespace MetrixWeb
{
    public class MetrixHttpModule : IHttpModule
    {
        public const string METRIX_REQUEST_DATA = "MetrixRequestData";

        public MetrixHttpModule()
        {
        }

        public string ModuleName
        {
            get { return "MetrixHttpModule"; }
        }

        public void Dispose() { }

        public CountingStream CountingStream { get; set; }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += ContextOnBeginRequest;
            context.EndRequest += ContextOnEndRequest;
        }

        public void ContextOnBeginRequest(object sender, EventArgs e)
        {
            var startRequest = Stopwatch.GetTimestamp();
            var application = sender as HttpApplication;
            if (application != null)
            {
                var data = new MetrixRequestData();
                var request = GetRequest(sender);
                var context = GetContext(sender);

                var response = GetResponse(sender);
                response.Filter = CountingStream = new CountingStream(response.Filter);

                data.RequestGuid = Guid.NewGuid();
                data.StartRequest = startRequest;
                data.RawUrl = request.RawUrl;
                context.Items[METRIX_REQUEST_DATA] = data;
                
                data.StartRequestWork = Stopwatch.GetTimestamp();
            }
        }
        
        public void ContextOnEndRequest(object sender, EventArgs e)
        {
            var endRequestWork = Stopwatch.GetTimestamp();
            var application = sender as HttpApplication;
            if (application != null)
            {
                var context = GetContext(sender);
                var response = GetResponse(sender);

                if ("text/html".Equals(response.ContentType))
                {
                    var data = context.Items[METRIX_REQUEST_DATA] as MetrixRequestData;
                    data.EndRequestWork = endRequestWork;
                    data.ResponseStatus = response.Status;

                    response.Flush();
                    data.OutputLength = CountingStream.Count;

                    data.EndRequest = Stopwatch.GetTimestamp();
                    MetrixRequestData.AddFrame(data);

                    SerializeData(application, data);
                }
            }
        }

        private void SerializeData(HttpApplication application, MetrixRequestData data)
        {
            // Write to HTML footer (TODO: make configurable)
            GetResponse(application).Write(string.Format("<hr/><pre>{0}</pre><hr/>", data.Display.Replace("\n", "<br/>")));
            
            // Write to console (TODO: make configurable)
            System.Diagnostics.Debug.WriteLine("-----------------");
            System.Diagnostics.Debug.WriteLine(data.Display);
            System.Diagnostics.Debug.WriteLine("-----------------");
            
            // TODO: Write to NLog (make configurable)
            // TODO: Write to EventLog (make configurable)
            // TODO: Write to Database (make configurable)
            // TODO: Write to WebAPI (make configurable)
            // ...
        }
        
        #region " Dependency Injection (well, overrides) for Tests "
        
        // replaced in tests with mock request
        public Func<object, HttpRequestBase> GetRequest = (object sender) => {
            return new HttpRequestWrapper((sender as HttpApplication).Context.Request);
        };

        // replaced in tests with mock request
        public Func<object, HttpResponseBase> GetResponse = (object sender) => {
            return new HttpResponseWrapper((sender as HttpApplication).Context.Response);
        };

        // replaced in tests with mock context
        public Func<object, HttpContextBase> GetContext = (object sender) => {
            return new HttpContextWrapper((sender as HttpApplication).Context);
        };
        
        #endregion

    }
}