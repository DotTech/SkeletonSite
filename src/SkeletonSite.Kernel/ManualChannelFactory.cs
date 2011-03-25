using System;
using System.ServiceModel.Configuration;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Configuration;

namespace SkeletonSite.Kernel
{
    /// <summary>
    /// Factory class for manual initialization of service channels
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ManualChannelFactory<T> : ChannelFactory<T>
    {
        private readonly string _contractName;
        readonly Uri _overrideEndPoint;

        /// <summary>
        /// custom client channel constructor which 
        /// specifies an external configuration file
        /// </summary>
        /// <param name="contractName">Contract name of desired endpoint</param>
        public ManualChannelFactory(string contractName)
            : base(typeof(T))
        {
            _contractName = contractName;
            InitializeEndpoint((string)null, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualChannelFactory&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="contractName">Contract name of desired endpoint</param>
        /// <param name="overrideEndPoint">The override end point.</param>
        public ManualChannelFactory(string contractName, Uri overrideEndPoint)
            : base(typeof(T))
        {
            _contractName = contractName;
            _overrideEndPoint = overrideEndPoint;
            InitializeEndpoint((string)null, null);
        }

        /// <summary>
        /// Overrides the CreateDescription() method of the channel factory to force configuration to be loaded from Configuration class
        /// </summary>
        /// <returns></returns>
        protected override ServiceEndpoint CreateDescription()
        {
            ServiceEndpoint serviceEndpoint = base.CreateDescription();
            ServiceModelSectionGroup serviceModeGroup = ServiceModelSectionGroup.GetSectionGroup(Configuration.GetServicesConfiguration());
            ChannelEndpointElement selectedEndpoint = null;

            foreach (ChannelEndpointElement endpoint in serviceModeGroup.Client.Endpoints)
            {
                if (endpoint.Contract == _contractName)
                {
                    selectedEndpoint = endpoint;
                    break;
                }
            }

            if (selectedEndpoint != null)
            {
                if (serviceEndpoint.Binding == null)
                {
                    serviceEndpoint.Binding = CreateBinding(selectedEndpoint.Binding, serviceModeGroup);
                }

                if (serviceEndpoint.Address == null)
                {
                    if (_overrideEndPoint == null)
                    {
                        serviceEndpoint.Address = new EndpointAddress(selectedEndpoint.Address, GetIdentity(selectedEndpoint.Identity), selectedEndpoint.Headers.Headers);
                    }
                    else
                    {
                        serviceEndpoint.Address = new EndpointAddress(_overrideEndPoint, GetIdentity(selectedEndpoint.Identity), selectedEndpoint.Headers.Headers);
                    }
                }

                if (serviceEndpoint.Behaviors.Count == 0 && !String.IsNullOrEmpty(selectedEndpoint.BehaviorConfiguration))
                {
                    AddBehaviors(selectedEndpoint.BehaviorConfiguration, serviceEndpoint, serviceModeGroup);
                }

                serviceEndpoint.Name = selectedEndpoint.Contract;
            }

            return serviceEndpoint;
        }

        /// <summary>
        /// Configures the binding for the selected endpoint
        /// </summary>
        /// <param name="bindingName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private Binding CreateBinding(string bindingName, ServiceModelSectionGroup group)
        {
            BindingCollectionElement bindingElementCollection = group.Bindings[bindingName];
            if (bindingElementCollection.ConfiguredBindings.Count > 0)
            {
                IBindingConfigurationElement be = bindingElementCollection.ConfiguredBindings[0];

                Binding binding = GetBinding(be);
                if (be != null)
                {
                    be.ApplyConfiguration(binding);
                }

                return binding;
            }

            return null;
        }

        /// <summary>
        /// Adds the configured behavior to the selected endpoint
        /// </summary>
        /// <param name="behaviorConfiguration"></param>
        /// <param name="serviceEndpoint"></param>
        /// <param name="group"></param>
        private void AddBehaviors(string behaviorConfiguration, ServiceEndpoint serviceEndpoint, ServiceModelSectionGroup group)
        {
            EndpointBehaviorElement behaviorElement = group.Behaviors.EndpointBehaviors[behaviorConfiguration];
            for (int i = 0; i < behaviorElement.Count; i++)
            {
                BehaviorExtensionElement behaviorExtension = behaviorElement[i];
                object extension = behaviorExtension.GetType().InvokeMember("CreateBehavior",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                null, behaviorExtension, null);
                if (extension != null)
                {
                    serviceEndpoint.Behaviors.Add((IEndpointBehavior)extension);
                }
            }
        }

        /// <summary>
        /// Gets the endpoint identity from the configuration file
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private EndpointIdentity GetIdentity(IdentityElement element)
        {
            EndpointIdentity identity = null;
            PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            if (properties["dns"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            if (properties["rsa"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509Certificate2Collection supportingCertificates = new X509Certificate2Collection();
                supportingCertificates.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
                if (supportingCertificates.Count == 0)
                {
                    throw new InvalidOperationException("UnableToLoadCertificateIdentity");
                }
                X509Certificate2 primaryCertificate = supportingCertificates[0];
                supportingCertificates.RemoveAt(0);
                return EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, supportingCertificates);
            }

            return identity;
        }

        /// <summary>
        /// Helper method to create the right binding depending on the configuration element
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <returns></returns>
        private Binding GetBinding(IBindingConfigurationElement configurationElement)
        {
            if (configurationElement is CustomBindingElement)
                return new CustomBinding();
            if (configurationElement is BasicHttpBindingElement)
                return new BasicHttpBinding();
            if (configurationElement is NetMsmqBindingElement)
                return new NetMsmqBinding();
            if (configurationElement is NetNamedPipeBindingElement)
                return new NetNamedPipeBinding();
            if (configurationElement is NetPeerTcpBindingElement)
                return new NetPeerTcpBinding();
            if (configurationElement is NetTcpBindingElement)
                return new NetTcpBinding();
            if (configurationElement is WSDualHttpBindingElement)
                return new WSDualHttpBinding();
            if (configurationElement is WSHttpBindingElement)
                return new WSHttpBinding();
            if (configurationElement is WSFederationHttpBindingElement)
                return new WSFederationHttpBinding();

            return null;
        }
    }
}
