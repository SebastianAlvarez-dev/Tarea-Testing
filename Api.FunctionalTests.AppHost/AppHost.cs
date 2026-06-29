var builder = DistributedApplication.CreateBuilder(args);

builder.AddSqlServer("bdserver").AddDatabase("bd");

builder.Build().Run();
