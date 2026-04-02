namespace EcommerceApp.Services;

public interface ICartService
{
    IReadOnlyDictionary<int, int> GetItems();
    void Add(int productId, int quantity = 1);
    void Update(int productId, int quantity);
    void Remove(int productId);
    void Clear();
}
