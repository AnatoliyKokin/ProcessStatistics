using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public struct OperationResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }

        public static OperationResult OK { get { return new OperationResult(true); } }

        public static OperationResult Error(string msg) { return new OperationResult(false, msg); }

        public static bool operator == (OperationResult r1, OperationResult r2)
        {
            return r1.Result == r2.Result;
        }

        public static bool operator !=(OperationResult r1, OperationResult r2)
        {
            return r1.Result != r2.Result;
        }

        public static bool operator ==(OperationResult r1, bool r2)
        {
            return r1.Result == r2;
        }

        public static bool operator !=(OperationResult r1, bool r2)
        {
            return r1.Result != r2;
        }

        public static implicit operator bool(OperationResult r) => r.Result;

        private OperationResult(bool result, string msg = "")
        {
            Result = result;
            Message = msg;
        }


    }
}
