using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Client;
using Errordite.Core;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.IoC;
using Errordite.Core.Messaging;
using Errordite.Core.Misc;
using Errordite.Core.Organisations.Queries;
using System.Linq;
using Errordite.Core.Session;
using Errordite.Services.Consumers;
using Castle.MicroKernel.Lifestyle;
using Newtonsoft.Json;
using Errordite.Core.Extensions;

namespace Errordite.Services.Processors
{
    public class MessageProcessor : ComponentBase
    {
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<string> _organisations = new List<string>();
        private readonly OwnThreadQueueProcessor<MessageEnvelope> _queueProcessor;
        private readonly AmazonSQS _amazonSQS;

        public MessageProcessor(ServiceConfiguration serviceConfiguration, AmazonSQS amazonSQS, IComponentAuditor auditor)
        {
            _serviceConfiguration = serviceConfiguration;
            _amazonSQS = amazonSQS;
            _queueProcessor = new OwnThreadQueueProcessor<MessageEnvelope>(ProcessMessage, auditor);
            Auditor = auditor;
        }

        private void ProcessMessage(MessageEnvelope envelope)
        {
            var watch = Stopwatch.StartNew();

            Trace("Processing message of type '{0}' for oranisation with Id;={1}", 
                envelope.MessageType, 
                envelope.OrganisationId);

            bool processed = false;
            Exception error = null;

            for (int attempt = 0; attempt < _serviceConfiguration.RetryLimit; attempt++)
            {
                try
                {
                    Trace("Attempt {0} processing message", attempt);
                    TryProcessMessage(envelope);
                    Trace("Attempt {0} successfully processed message", attempt);
                    processed = true;
                    break;
                }
                catch (Exception ex)
                {
                    Trace("Failed to proccess attempt {0}, error:{1}", attempt, ex.Message);
                    error = ex;
                }

                if(_serviceConfiguration.RetryDelayMilliseconds > 0)
                    Thread.Sleep(_serviceConfiguration.RetryDelayMilliseconds); 
            }

            if (!processed)
            {
                Trace("Failed all attempts for message with Id:{0}", envelope.Id);

                using (ObjectFactory.Container.BeginScope())
                {
                    using (var session = ObjectFactory.GetObject<IAppSession>())
                    {
                        Trace("Message for Organisation:={0} failed, logging to RavenDB", envelope.OrganisationId);

                        envelope.Service = _serviceConfiguration.Service;
                        session.MasterRaven.Store(envelope);
                        session.Commit();
                    }

                    if (error != null)
                    {
                        error.Data.Add("MessageEnvelopeId", envelope.Id);
                        Error(error);
                        ErrorditeClient.ReportException(error);
                    }
                }
            }

            Trace("Processing for message with Id:={0} completed in {1}ms", envelope.Id, watch.ElapsedMilliseconds);

            _amazonSQS.DeleteMessage(new DeleteMessageRequest
            {
                QueueUrl = _serviceConfiguration.QueueAddress,
                ReceiptHandle = envelope.ReceiptHandle
            });
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
                        Trace("Received Message of type {0}", envelope.Message.GetType().FullName);
                        TraceObject(envelope.Message);

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

            var consumerType = typeof(IErrorditeConsumer<>).MakeGenericType(messageType);

            if (!ObjectFactory.Container.Kernel.HasComponent(consumerType))
                consumerType = typeof(IErrorditeConsumer<>).MakeGenericType(messageType.BaseType);

            dynamic consumer = ObjectFactory.Container.Resolve(consumerType);
            dynamic message = JsonConvert.DeserializeObject(envelope.Message, messageType);

            //base type is MessageBase, set Organisation property before invoking message consumer
            message.Organisation = organisation;
            consumer.Consume(message);
        }

        public bool ContainsOrganisation(string organisationId)
        {
            return _organisations.Any(o => o == organisationId);
        }

        public bool CanAddOrganisation()
        {
            return _organisations.Count < _serviceConfiguration.MaxOrganisationsPerMessageProcesor;
        }

        public void AddOrganisation(string organisationId)
        {
            _organisations.Add(organisationId);
        }

        public void Enquque(MessageEnvelope message)
        {
            _queueProcessor.Enqueue(message);
        }
    }
}
