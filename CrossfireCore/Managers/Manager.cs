using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.Managers
{
    public abstract class Manager
    {
        public Manager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
        {
            this.Connection = Connection;
            this.Builder = Builder;
            this.Handler = Handler;
        }

        protected SocketConnection Connection { get; private set; }
        protected MessageBuilder Builder { get; private set; }
        protected MessageHandler Handler { get; private set; }
    }
}
