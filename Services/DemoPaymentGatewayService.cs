namespace EcommerceApp.Services;

public class DemoPaymentGatewayService : IPaymentGatewayService
{
    public Task<PaymentInitResult> InitializeAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            return Task.FromResult(PaymentInitResult.Failed("Thiếu cổng thanh toán."));
        }

        if (request.Amount <= 0)
        {
            return Task.FromResult(PaymentInitResult.Failed("Số tiền không hợp lệ."));
        }

        var provider = request.Provider.Trim().ToUpperInvariant();
        var transactionId = $"{provider}-{Guid.NewGuid():N}";

        if (provider is "VNPAY" or "STRIPE")
        {
            var separator = request.ReturnUrl.Contains('?') ? '&' : '?';
            var redirectUrl = $"{request.ReturnUrl}{separator}provider={Uri.EscapeDataString(provider)}&txnRef={Uri.EscapeDataString(request.OrderCode)}&vnp_TransactionNo={Uri.EscapeDataString(transactionId)}&status=success";

            return Task.FromResult(new PaymentInitResult
            {
                Success = true,
                RequiresRedirect = true,
                RedirectUrl = redirectUrl,
                TransactionId = transactionId
            });
        }

        return Task.FromResult(PaymentInitResult.Failed("Provider chưa được hỗ trợ. Dùng VNPAY hoặc STRIPE."));
    }
}
