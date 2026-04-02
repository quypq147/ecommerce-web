using System.Text.Json;

namespace EcommerceApp.Services;

public class SessionCartService : ICartService
{
    private const string CartKey = "cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionCartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IReadOnlyDictionary<int, int> GetItems()
    {
        return GetCart();
    }

    public void Add(int productId, int quantity = 1)
    {
        if (quantity < 1)
        {
            return;
        }

        var cart = GetCart();
        cart[productId] = cart.TryGetValue(productId, out var existing) ? existing + quantity : quantity;
        Save(cart);
    }

    public void Update(int productId, int quantity)
    {
        var cart = GetCart();

        if (quantity <= 0)
        {
            cart.Remove(productId);
        }
        else
        {
            cart[productId] = quantity;
        }

        Save(cart);
    }

    public void Remove(int productId)
    {
        var cart = GetCart();
        if (cart.Remove(productId))
        {
            Save(cart);
        }
    }

    public void Clear()
    {
        Save([]);
    }

    private Dictionary<int, int> GetCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return [];
        }

        var data = session.GetString(CartKey);
        if (string.IsNullOrWhiteSpace(data))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<int, int>>(data) ?? [];
    }

    private void Save(Dictionary<int, int> cart)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return;
        }

        session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }
}
