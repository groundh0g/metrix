using System;
using System.Text;

namespace MetrixWeb
{
    public class MetrixRequestData
    {
        public long StartRequest { get; set; }
        public long StartRequestWork { get; set; }
        public long EndRequestWork { get; set; }
        public long EndRequest { get; set; }
        
        public Guid RequestGuid { get; set; }
        public string RawUrl { get; set; }
        public string ResponseStatus { get; set; }
        public long OutputLength { get; set; }

        public static long MinimumMillis { get; set; }
        public static long MaximumMillis { get; set; }

        public static long TotalMillis { get; set; }
        public static long TotalFrames { get; set; }
        private static bool skipFrame = true;
        
        public static void AddFrame(MetrixRequestData data)
        {
            if (skipFrame)
            {
                // skip the "first run" frame times, they're artificially high
                skipFrame = false;
                MinimumMillis = long.MaxValue;
                MaximumMillis = long.MinValue;
            }
            else
            {
                var elapsed = data.EndRequest - data.StartRequest;
                MetrixRequestData.TotalMillis += elapsed;
                MetrixRequestData.TotalFrames++;
                MetrixRequestData.MinimumMillis = Math.Min(MetrixRequestData.MinimumMillis, elapsed);
                MetrixRequestData.MaximumMillis = Math.Max(MetrixRequestData.MaximumMillis, elapsed);
            }
        }

        public double RequestDuration
        {
            get { return new TimeSpan(EndRequest - StartRequest).TotalSeconds; }
        }

        public double OverheadDuration
        {
            get { return new TimeSpan((EndRequest - StartRequest) - (EndRequestWork - StartRequestWork)).TotalSeconds; }
        }

        // unused. tricky to measure, requires second (unmeasured) call to log the time-to-log
        public long StartSerialize { get; set; }
        public long EndSerialize { get; set; }
        
        public string Display
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("MetrixWeb.MetrixRequestData")
                    .Append("  RequestGuid: ").AppendLine(RequestGuid.ToString())
                    .Append("  StartRequest: ").AppendLine(StartRequest.ToString())
                    .Append("  StartRequestWork: ").AppendLine(StartRequestWork.ToString())
                    .Append("  EndRequestWork: ").AppendLine(EndRequestWork.ToString())
                    .Append("  EndRequest: ").AppendLine(EndRequest.ToString())
                    .Append("  RequestDuration: ").AppendLine(RequestDuration.ToString() + " seconds")
                    .Append("  OverheadDuration: ").AppendLine(OverheadDuration.ToString() + " seconds")
                    .Append("  AverageDuration: ").AppendLine((new TimeSpan(TotalMillis).TotalSeconds / Math.Max(1.0, TotalFrames)).ToString() + " seconds")
                    .Append("  MinimumDuration: ").AppendLine((new TimeSpan(MinimumMillis).TotalSeconds.ToString() + " seconds"))
                    .Append("  MaximumDuration: ").AppendLine((new TimeSpan(MaximumMillis).TotalSeconds.ToString() + " seconds"))
                    .Append("  RawUrl: ").AppendLine(RawUrl)
                    .Append("  ResponseStatus: ").AppendLine(ResponseStatus)
                    .Append("  OutputLength: ").AppendLine(OutputLength.ToString())
                    .ToString();
            }
        }
    }
    
}