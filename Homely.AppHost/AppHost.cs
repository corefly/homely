var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var authDb = postgres.AddDatabase("authdb");

builder.AddProject<Projects.Homely_AuthService>("auth")
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
