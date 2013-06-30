using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetGuidConverter
{
    public class MessageException : Exception
    {
        public MessageException(string message, params object[] args):base(String.Format(message,args))
        {
            
        }
    }
}
