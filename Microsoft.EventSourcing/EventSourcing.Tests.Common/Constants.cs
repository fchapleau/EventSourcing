using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests.Common
{
    public static class Constants
    {
        public const string StorageAccountName = "eventsource";
        public const string StorageAccountKey = "";

        public const string ServiceBusConnectionString = "";
        public const string ServiceBusReadSideQueue = "symbolreadside";
        public const string ServiceBusWriteSideQueue = "symbolwriteside";
    }
}
