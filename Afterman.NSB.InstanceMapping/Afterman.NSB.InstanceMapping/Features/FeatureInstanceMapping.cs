﻿using System.Linq;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Routing;

namespace Afterman.NSB.InstanceMapping.Features
{
    using Constants;
    using Repository;
    using Repository.Concepts;

    public class FeatureInstanceMapping
        : Feature
    {
        public FeatureInstanceMapping()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            SqlHelper.CreateTableIfNotExists();
            var endpointInstances = context.Settings.Get<EndpointInstances>();
            var refresher = new AutoRefresher(endpointInstances);
            context.RegisterStartupTask(refresher);
            RegisterCurrentEndpoint(context);
        }

        private void RegisterCurrentEndpoint(FeatureConfigurationContext context)
        {
            var instanceMappings = SqlHelper.GetAll();
            var endpointLogicalAddress = (LogicalAddress)context.Settings.Get(NServiceBusSettings.LogicalAddress);
            var endpointName = endpointLogicalAddress.EndpointInstance.Endpoint;
            var machineName = endpointLogicalAddress.EndpointInstance.Properties[NServiceBusSettings.Machine];
            if (instanceMappings.Any(x => x.EndpointName == endpointName && x.TargetMachine == machineName)) return;

            SqlHelper.Add(new InstanceMapping
            {
                EndpointName = endpointName,
                TargetMachine = machineName,
                IsEnabled = true
            });
        }
    }
}
