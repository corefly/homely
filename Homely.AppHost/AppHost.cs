var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var postgresUserName = builder.AddParameter("postgres-username", "postgres", publishValueAsDefault: true);
var postgresPassword = builder.AddParameter("postgres-password", "homely-postgres-dev-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUserName, postgresPassword)
    .WithDataVolume("homely-postgres-data");

var authDb = postgres.AddDatabase("authdb");

builder.AddProject<Projects.Homely_AuthService>("auth")
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
