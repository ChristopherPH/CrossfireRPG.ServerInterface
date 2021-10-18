using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public abstract class Manager
    {
        public Manager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            this.Connection = Connection;
            this.Builder = Builder;
            this.Parser = Parser;
        }

        protected SocketConnection Connection { get; private set; }
        protected MessageBuilder Builder { get; private set; }
        protected MessageParser Parser { get; private set; }
    }
}
