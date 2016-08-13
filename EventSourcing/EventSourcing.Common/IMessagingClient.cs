﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IMessagingClient
    {
        void SendMessage(EntityEvent e);
        IMessagingEvent Receive();
        long MessageWaitingCount();

        long MessageInErrorCount();
    }
}
