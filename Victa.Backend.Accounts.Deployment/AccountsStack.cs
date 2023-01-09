using Pulumi;
using Pulumi.Gcp.Container;

using Victa.Backend.Accounts.Deployment.Configs;

using CloudRun = Pulumi.Gcp.CloudRun;
using Docker = Pulumi.Docker;
using Gcp = Pulumi.Gcp;
using PubSub = Pulumi.Gcp.PubSub;

namespace Victa.Backend.Accounts.Deployment;

public class AccountsStack : Stack
{
    public AccountsStack(DeploymentCfg cfg)
    {
        Registry registry = new("BackendRegistry");
        Output<string> registryUrl = registry.Id.Apply(async _ =>
        {
            return (await GetRegistryRepository.InvokeAsync()).RepositoryUrl;
        });

        StackSecrets secrets = new(cfg);

        Docker.Image dockerImage = new("DockerImage", args: new()
        {
            ImageName = Output.Format($"{registryUrl}/accounts-backend:{cfg.GithubRefName}"),
            Build = new Docker.DockerBuild
            {
                Context = cfg.BuildDirectory,
                Dockerfile = Path.Combine(cfg.BuildDirectory, "Victa.Backend.Accounts", "Dockerfile")
            }
        });

        CloudRun.Service service = new($"CloudRunService-{cfg.Deployment.ResourcePrefix}", new()
        {
            Name = $"accounts-{cfg.Deployment.ResourcePrefix}",
            Location = Gcp.Location.UsCentral1,
            AutogenerateRevisionName = true,
            Template = new CloudRun.Inputs.ServiceTemplateArgs
            {
                Metadata = new CloudRun.Inputs.ServiceTemplateMetadataArgs
                {
                    Annotations = new()
                    {
                        { "autoscaling.knative.dev/minScale", cfg.Container.MinScale },
                        { "autoscaling.knative.dev/maxScale", cfg.Container.MaxScale },
                        { "run.googleapis.com/execution-environment", cfg.Container.ExecutionEnvironment },
                        { "run.googleapis.com/startup-cpu-boost", cfg.Container.StartupCpuBust }
                    }
                },
                Spec = new CloudRun.Inputs.ServiceTemplateSpecArgs
                {
                    ContainerConcurrency = cfg.Container.Concurrency,
                    Containers = new CloudRun.Inputs.ServiceTemplateSpecContainerArgs
                    {
                        Image = dockerImage.ImageName,
                        Ports =
                        {
                            new CloudRun.Inputs.ServiceTemplateSpecContainerPortArgs
                            {
                                ContainerPort = cfg.Container.Port
                            }
                        },
                        Resources = new CloudRun.Inputs.ServiceTemplateSpecContainerResourcesArgs
                        {
                            Limits =
                            {
                                { "cpu", cfg.Container.Cpu },
                                { "memory", cfg.Container.Memory },
                            }
                        },
                        Envs =
                        {
                            // TODO: set as ENV: IS4_ISSUER_URI, APP_NAME
                            CreateEnv("DB_NAME", cfg.App.DbName),
                            CreateEnvAsSecretReference("DB_CONN", secrets.DbConn.SecretId),
                        }
                    }
                }
            }
        });

        CloudRun.IamMember cloudRunBackendServiceIamMember = new("PublicAccess", args: new()
        {
            Service = service.Name,
            Location = Gcp.Location.UsCentral1,
            Role = "roles/run.invoker",
            Member = "allUsers"
        });

        CloudRun.DomainMapping backendServiceTestDomainMapping = new("BackendCloudRunServiceDomainMapping", new()
        {
            Name = cfg.Container.Domain,
            Location = Gcp.Location.UsCentral1,
            Spec = new CloudRun.Inputs.DomainMappingSpecArgs { RouteName = service.Name, },
            Metadata = new CloudRun.Inputs.DomainMappingMetadataArgs
            {
                Namespace = service.Metadata.Apply(x => x.Namespace),
            },
        });

        var topics = new
        {
            user = new
            {
                created = CreateTopic($"accounts.user.created.{cfg.Deployment.ResourcePrefix}"),
                updated = CreateTopic($"accounts.user.updated.{cfg.Deployment.ResourcePrefix}"),
            },
        };

        Output<string> backendServiceUrl = service.Statuses.Apply(
            x => x.FirstOrDefault()?.Url?.TrimEnd('/') ?? throw new InvalidOperationException("Bad service url"));
    }



    private static Gcp.SecretManager.Secret CreateDefaultSecret(string name, Input<string> data)
    {

        var secret = new Gcp.SecretManager.Secret(name, new()
        {
            SecretId = name,
            Replication = new Gcp.SecretManager.Inputs.SecretReplicationArgs { Automatic = true, },
            Labels =
            {
                { "type", "service" },
                { "service", "backend" },
            }
        });

        _ = new Gcp.SecretManager.SecretVersion($"{name}-value", new()
        {
            Enabled = true,
            Secret = secret.Id,
            SecretData = data
        });

        return secret;
    }

    private static CloudRun.Inputs.ServiceTemplateSpecContainerEnvArgs CreateEnv(string envName, string value)
    {
        return new() { Name = envName, Value = value };
    }

    private static CloudRun.Inputs.ServiceTemplateSpecContainerEnvArgs CreateEnvAsSecretReference(string envName, Output<string> secretName)
    {
        return new CloudRun.Inputs.ServiceTemplateSpecContainerEnvArgs
        {
            Name = envName,
            ValueFrom = new CloudRun.Inputs.ServiceTemplateSpecContainerEnvValueFromArgs
            {
                SecretKeyRef = new CloudRun.Inputs.ServiceTemplateSpecContainerEnvValueFromSecretKeyRefArgs
                {
                    Key = "latest",
                    Name = secretName
                }
            }
        };
    }

    private static PubSub.Topic CreateTopic(string name)
    {
        return new PubSub.Topic(name, args: new() { Name = name }, options: new());
    }

    private static PubSub.Subscription CreateSubscription(string name, Output<string> topicName, Output<string> pushEndpoint)
    {
        return new PubSub.Subscription(name, new()
        {
            Name = name,
            Topic = topicName,
            PushConfig = new PubSub.Inputs.SubscriptionPushConfigArgs
            {
                PushEndpoint = pushEndpoint,
            },
            ExpirationPolicy = new PubSub.Inputs.SubscriptionExpirationPolicyArgs { Ttl = string.Empty },
            MessageRetentionDuration = TimeSpan.FromDays(3).TotalSeconds + "s",
            RetryPolicy = new PubSub.Inputs.SubscriptionRetryPolicyArgs
            {
                MaximumBackoff = "60s"
            },
        });
    }
}
