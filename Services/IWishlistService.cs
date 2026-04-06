namespace EcommerceApp.Services;

public interface IWishlistService
{
    IReadOnlyCollection<int> GetItems();
    void Add(int productId);
    void Remove(int productId);
    void Clear();
}
