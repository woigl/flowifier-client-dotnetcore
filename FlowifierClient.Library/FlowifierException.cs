using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowifierClient
{
    public class FlowifierException : Exception
    {
        public FlowifierException() : base() { }

        public FlowifierException(string message) : base(message) { }

        public FlowifierException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
