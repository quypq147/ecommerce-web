namespace EcommerceApp.Services;

public class PaymentRequest
{
    public required string Provider { get; init; }
    public required string OrderCode { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required string ReturnUrl { get; init; }
}

public class PaymentInitResult
{
    public bool Success { get; init; }
    public bool RequiresRedirect { get; init; }
    public string? RedirectUrl { get; init; }
    public string? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static PaymentInitResult Failed(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}

public interface IPaymentGatewayService
{
    Task<PaymentInitResult> InitializeAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}
