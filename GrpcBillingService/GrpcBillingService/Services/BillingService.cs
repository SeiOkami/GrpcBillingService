using Grpc.Core;
using Billing;
using GrpcBillingService.BillingBase;

namespace GrpcBillingService.Services;

public class BillingSevice : Billing.Billing.BillingBase
{
    private IBillingBase _base;

    public BillingSevice(IBillingBase @base)
    {
        _base = @base;
    }

    public override async Task ListUsers(None request,
        IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
    {
        foreach (var user in _base.ListUsers())
            await responseStream.WriteAsync(user);
    }

    public override Task<Response> CoinsEmission(EmissionAmount amount, ServerCallContext context)
    {
        return Task.FromResult(_base.CoinsEmission(amount));
    }

    public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
    {
        return Task.FromResult(_base.MoveCoins(request));
    }
    
    public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
    {
        return Task.FromResult(_base.LongestHistoryCoin());
    }
}
