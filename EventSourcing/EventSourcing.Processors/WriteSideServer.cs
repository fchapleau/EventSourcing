using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.Processors
{
    public class WriteSideServer<T> where T :IEntity, new()
    {
        private IMessagingClient _writeSideClient;
        private IMessagingClient _readSideClient;
        private List<Thread> _asyncReceiveThreads;
        private ILogging _logger;
        private IDataAccessLayer<T> _dal;

        internal WriteSideServer(ILogging logger, IDataAccessLayer<T> dal, IMessagingClient writeSideClient, IMessagingClient readSideClient, int numberOfThreads)
        {
            _logger = logger;
            _dal = dal;

            _writeSideClient = writeSideClient;
            _readSideClient = readSideClient;

            _asyncReceiveThreads = new List<Thread>();
            for (var i = 0; i < numberOfThreads; i++)
            {
                var asyncReceive = new Thread(new ParameterizedThreadStart(ProcessAsync));
                asyncReceive.IsBackground = true;
                asyncReceive.Start(i);

                _asyncReceiveThreads.Add(asyncReceive);
            }
        }

        private void ProcessAsync(object i)
        {
            while (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested)
            {
                try
                {
                    using (var retrievedMessage = _writeSideClient.Receive())
                    {
                        if (retrievedMessage == null) continue;

                        EntityEvent eventReceived = null;
                        try
                        {
                            eventReceived = retrievedMessage.Event;
                            eventReceived.OrderKey = string.Format("{0}-{1}", DateTimeOffset.MaxValue.Ticks - eventReceived.EventTimeStamp.Ticks, long.MaxValue - retrievedMessage.SequenceNumber);

                            _dal.InsertEvent(eventReceived);
                            _readSideClient.SendMessage(eventReceived);

                            retrievedMessage.Complete();
                            _logger.WriteLine(LoggingLevel.Verbose, string.Format("Event {0} Processed.", eventReceived.ToString()));
                        }
                        catch (SerializationException serEx)
                        {
                            retrievedMessage.DeadLetter();
                            _logger.WriteLine(LoggingLevel.Error, "Read Side [" + i.ToString() + "], Serialization Exception, DeadLettering Event [" + eventReceived.ToString() + "]... " + serEx.ToString());
                        }
                        catch (Exception ex)
                        {
                            retrievedMessage.Abandon();
                            _logger.WriteLine(LoggingLevel.Verbose, "Write Side [" + i.ToString() + "], Abandon Event [" + eventReceived.ToString() + "]... " + ex.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.WriteLine(LoggingLevel.Error, "Error when trying to receive a message" + e.ToString());
                }
            }
        }
    }
}
