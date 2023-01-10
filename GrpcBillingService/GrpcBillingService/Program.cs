using GrpcBillingService.BillingBase;
using GrpcBillingService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IBillingBase, BillingBaseMemory>();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<BillingSevice>();
app.MapGet("/", () => "Billing GRPC Service");

app.Run();
