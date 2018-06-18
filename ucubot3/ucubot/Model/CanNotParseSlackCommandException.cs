using System;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Runtime;

namespace ucubot.Model
{
    public class CanNotParseSlackCommandException : Exception
    {
        public CanNotParseSlackCommandException(string command) : base($"Can not parse command {command}") { }
    }
}