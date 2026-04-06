using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EcommerceApp.Services;

public class SessionWishlistService : IWishlistService
{
    private const string WishlistKey = "wishlist";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionWishlistService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IReadOnlyCollection<int> GetItems()
    {
        return GetWishlist();
    }

    public void Add(int productId)
    {
        var wishlist = GetWishlist();
        wishlist.Add(productId);
        Save(wishlist);
    }

    public void Remove(int productId)
    {
        var wishlist = GetWishlist();
        if (wishlist.Remove(productId))
        {
            Save(wishlist);
        }
    }

    public void Clear()
    {
        Save([]);
    }

    private HashSet<int> GetWishlist()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return [];
        }

        var data = session.GetString(WishlistKey);
        if (string.IsNullOrWhiteSpace(data))
        {
            return [];
        }

        return JsonSerializer.Deserialize<HashSet<int>>(data) ?? [];
    }

    private void Save(HashSet<int> wishlist)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return;
        }

        session.SetString(WishlistKey, JsonSerializer.Serialize(wishlist));
    }
}
