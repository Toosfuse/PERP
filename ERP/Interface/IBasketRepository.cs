using ERP.Models;
namespace ERP.Interface
{
    public interface IBasketRepository
    {
        Task<UserBasketCheck> GetBasketAsync(string BasketId);
        Task<UserBasketCheck> UpdateBasketAsync(UserBasketCheck UserBasketCheck);

        Task<bool> DeleteBasketAsync(string BasketId);

    }
}
