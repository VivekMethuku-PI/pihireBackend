using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.Common.Logging
{
    public class LoggingEvents
    {
        public const int GenerateItems = 1000;
        public const int ListItems = 1001;
        public const int GetItem = 1002;
        public const int InsertItem = 1003;
        public const int UpdateItem = 1004;
        public const int DeleteItem = 1005;
        public const int Authenticate = 1007;

        public const int Other = 3999;

        public const int MandatoryDataMissing = 4000;
        public const int GetItemNotFound = 4001;
        public const int UpdateItemNotFound = 4002;
    }
}
