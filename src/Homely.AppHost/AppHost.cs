var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);
var postgresDb = postgres.AddDatabase("homely");

builder.Build().Run();
