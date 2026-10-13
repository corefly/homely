using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var postgresServer = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var postgresDb = postgresServer.AddDatabase("homely");

var api = builder.AddProject<Projects.Homely_Api>("api")
    .WithReference(postgresDb)
    .WaitFor(postgresDb);

var frontend = builder
    .AddViteApp("frontend", "../frontend");

builder
    .AddYarp("app")
    .WithHostHttpsPort(8081)
    .WithConfiguration(yarp =>
    {
        var apiCluster = yarp.AddCluster(api);
        yarp.AddRoute("/api/{**catch-all}", apiCluster)
            .WithTransformPathRemovePrefix("/api");

        if (builder.ExecutionContext.IsRunMode)
        {
            var frontendCluster = yarp.AddCluster(frontend);
            yarp.AddRoute("{**catch-all}", frontendCluster);
        }
    })
    .WithExternalHttpEndpoints()
    .PublishWithStaticFiles(frontend);

builder.Build().Run();
