using System;

namespace SSMSTools.Exceptions
{
    internal class OnlyOneObjectExplorerNodeAllowedException: Exception
    {
        public OnlyOneObjectExplorerNodeAllowedException(string message) : base(message)
        {
        }
    }
}
