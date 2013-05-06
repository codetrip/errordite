using System;
using System.Diagnostics;
using System.Threading;
using Errordite.Client;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.IoC;
using Errordite.Core.Messaging;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Services.Consumers;
using Castle.MicroKernel.Lifestyle;
using Newtonsoft.Json;
using Errordite.Core.Extensions;
using Raven.Abstractions.Exceptions;

namespace Errordite.Services.Processors
{
    public class SQSMessageProcessor : ComponentBase, IMessageProcessor
    {
        public void Process(ServiceConfiguration configuration, MessageEnvelope envelope)
        {
            var watch = Stopwatch.StartNew();

            Trace("Processing message of type '{0}' for oranisation with Id;={1}", 
                envelope.MessageType, 
                envelope.OrganisationId);

            try
            {
                for (int attempt = 1; attempt <= configuration.ConcurrencyRetryLimit; attempt++)
                {
                    try
                    {
                        Trace("Attempt {0} processing message", attempt);
                        TryProcessMessage(envelope);
                        Trace("Attempt {0} successfully processed message", attempt);
                        break;
                    }
                    catch (ConcurrencyException ex)
                    {
                        Trace("Concurrency exception proccessing attempt #{0}, message:{1}", attempt, ex.Message);

                        //failed on last attempt so rethrow the error
                        if (attempt == configuration.ConcurrencyRetryLimit)
                            throw;
                    }

                    if (configuration.ConcurrencyRetryDelayMilliseconds > 0)
                        Thread.Sleep(configuration.ConcurrencyRetryDelayMilliseconds);
                }
            }
            catch (Exception e)
            {
                Trace("Failed all attempts for message with Id:{0}", envelope.Id);

                using (ObjectFactory.Container.BeginScope())
                {
                    using (var session = ObjectFactory.GetObject<IAppSession>())
                    {
	                    envelope.ErrorMessage = e.Message;

                        Trace("Message for Organisation:={0} failed, logging to RavenDB", envelope.OrganisationId);

                        session.MasterRaven.Store(envelope);
                        session.Commit();
                    }

                    e.Data.Add("MessageEnvelopeId", envelope.FriendlyId);
                    Error(e);
                    ErrorditeClient.ReportException(e);
                }
            }

            Trace("Processing for message with Id:={0} completed in {1}ms", envelope.Id, watch.ElapsedMilliseconds);
        }

        private void TryProcessMessage(MessageEnvelope envelope)
        {
            //if there is no organisation Id this is a notification message, we dont need a session
            if (envelope.OrganisationId == Organisation.NullOrganisationId)
            {
                DoProcessMessage(envelope, null);
            }
            else
            {
                //start a scope on the container so the session is shared only within the context of this message processing routine
                using (ObjectFactory.Container.BeginScope())
                {
                    using (var session = ObjectFactory.GetObject<IAppSession>())
                    {
                        var organisation = ObjectFactory.GetObject<IGetOrganisationQuery>().Invoke(new GetOrganisationRequest
                        {
                            OrganisationId = envelope.OrganisationId
                        }).Organisation;

                        session.SetOrganisation(organisation);
                        DoProcessMessage(envelope, organisation);
                        session.Commit();
                    }
                }
            }
        }

        private void DoProcessMessage(MessageEnvelope envelope, Organisation organisation)
        {
            var messageType = Type.GetType(envelope.MessageType);

            if (messageType == null)
                throw new InvalidOperationException("Failed to resolve message type '{0}'".FormatWith(envelope.MessageType));

            Type consumerMessageType = messageType;
            Type consumerType = null;

            while (consumerMessageType != null)
            {
                consumerType = typeof(IErrorditeConsumer<>).MakeGenericType(consumerMessageType);

                if (ObjectFactory.Container.Kernel.HasComponent(consumerType))
                    break;

                consumerMessageType = consumerMessageType.BaseType;
            }

            dynamic consumer = ObjectFactory.Container.Resolve(consumerType);
            dynamic message = JsonConvert.DeserializeObject(envelope.Message, messageType);

            //base type is MessageBase, set Organisation property before invoking message consumer
            message.Organisation = organisation;

            Trace("Invoking consumer for organisation '{0}' of type '{1}'...", organisation == null ? "No Organisation Set" : organisation.Id, consumer.GetType());
            consumer.Consume(message);
            Trace("Completed invocation of consumer for organisation '{0}' of type '{1}'...", organisation == null ? "No Organisation Set" : organisation.Id, consumer.GetType());
        }
    }
}
