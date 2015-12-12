using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Core;
using log4net.Appender;

namespace EMToolBox
{
    public delegate void LogTextAppend(string text);
    
    public class DelegateAppender : AppenderSkeleton
    {
        private LogTextAppend logTextAppend;

        public LogTextAppend LogTextMethod
        {
            get { return logTextAppend; }
            set { logTextAppend = value; }
        }

        public DelegateAppender()
        {
            logTextAppend = EmptyAppend;
        }

        private void EmptyAppend(string text)
        {
            // Do nothing
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (logTextAppend != null)
                logTextAppend(RenderLoggingEvent(loggingEvent));
        }
    }
}
