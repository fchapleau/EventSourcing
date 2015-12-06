﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.Processors
{
    public class ReadSideServer<T> where T : class, IEntity, new()
    {
        private int _snapshotFrequency;
        private int _snapShotSkewSeconds;
        private List<Thread> _asyncReceiveThreads;
        private ILogging _logger;
        private IDataAccessLayer<T> _dal;
        private Type _aggregateType;
        private IMessagingClient _readSideClient;

        internal ReadSideServer(Type aggregateType, ILogging logger, IDataAccessLayer<T> dal, IMessagingClient readSideClient, int numberOfThreads, int snapshotFrequency, int snapShotSkewSeconds)
        {
            _aggregateType = aggregateType;
            _logger = logger;
            _dal = dal;
            _readSideClient = readSideClient;
            _snapshotFrequency = snapshotFrequency;
            _snapShotSkewSeconds = snapShotSkewSeconds;

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
            while (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested)
            {
                try
                {
                    var eventReceived = _readSideClient.Receive();
                    if (eventReceived == null) continue;

                    try
                    {
                        ConstructorInfo ctor = _aggregateType.GetConstructor(Type.EmptyTypes);
                        var aggregate = ctor.Invoke(null) as IEntityAggregate<T>;
                        var aggregateWithoutSnapshot = ctor.Invoke(null) as IEntityAggregate<T>;

                        // Get the latest snapshot
                        EntityEvent snapshotEvent = null;
                        T snapshotEntity = default(T);
                        _dal.GetLatestSnapShot(eventReceived.Event.EntityUId, out snapshotEvent, out snapshotEntity);
                            
                        // Get all events after the snapshot
                        var eventSourcedEvents = _dal.GetEventsSince(eventReceived.Event.EntityUId, snapshotEvent);

                        if (eventSourcedEvents.Count() == 0)
                        {
                            _logger.WriteLine(LoggingLevel.Warning, string.Format("Latest Update was containing all events... bypassing processing for Entity {0}.", eventReceived.Event.EntityUId.ToString()));
                            eventReceived.Complete();
                            continue;
                        }
                            
                        // Update the Projection according to the latest snapshot and all remaining events
                        aggregate.Initialize((snapshotEntity == null) ? null : snapshotEntity.Clone() as T, eventSourcedEvents);
                        aggregateWithoutSnapshot.Initialize(null, _dal.GetEventsSince(eventReceived.Event.EntityUId, null));

                        if(snapshotEntity !=null && !aggregate.Entity.Equals(aggregateWithoutSnapshot.Entity) && Debugger.IsAttached){
                            Debugger.Break();
                        }
                        
                        _dal.InsertEntity(aggregate.Entity);

                        // If there is enough events, create a new snapshot
                        var eventsBeforeSkew = aggregate.GetEventsOlderthan(DateTime.Now.AddSeconds(-_snapShotSkewSeconds));
                                                
                        if (eventsBeforeSkew.Count() > _snapshotFrequency)
                        {
                            var snapshotInstance = ctor.Invoke(null) as IEntityAggregate<T>;
                            T clone = (snapshotEntity==null)?null: snapshotEntity.Clone() as T;
                            snapshotInstance.Initialize(clone, eventsBeforeSkew);
                            _dal.InsertSnapshot(snapshotInstance.Entity, snapshotInstance.LatestEvent);
                            _logger.WriteLine(LoggingLevel.Warning, 
                                string.Format("Snapshot created for Entity {0} and {1} events out of {2} total events.", 
                                aggregate.Entity.UId.ToString(), 
                                eventsBeforeSkew.Count(), 
                                eventSourcedEvents.Count()));
                        }

                        // Complete the transaction
                        eventReceived.Complete();
                        _logger.WriteLine(LoggingLevel.Verbose, string.Format("Event {0} Processed for Entity {1}, with {2} events.", eventReceived.Event.EventUId.ToString(), aggregate.Entity.UId.ToString(), eventSourcedEvents.Count()));
                    }
                    catch (SerializationException serEx)
                    {
                        eventReceived.DeadLetter();
                        _logger.WriteLine(LoggingLevel.Error, "Read Side [" + i.ToString() + "], Serialization Exception, DeadLettering Event [" + eventReceived.Event.ToString() + "]... " + serEx.ToString());
                    }
                    catch (Exception ex)
                    {
                        eventReceived.Abandon();
                        _logger.WriteLine(LoggingLevel.Error, "Read Side [" + i.ToString() + "], Abandon Event [" + eventReceived.Event.ToString() + "]... " + ex.ToString());
                    }
                }
                catch (Exception e)
                {
                    _logger.WriteLine(LoggingLevel.Verbose, "Error when trying to receive a message" + e.ToString());
                }
            }
        }
    }
}
