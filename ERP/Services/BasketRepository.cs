using ERP.Interface;
using ERP.Models;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ERP.Services
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _redis;

        public BasketRepository(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<bool> DeleteBasketAsync(string basketId)
        {
           return await _redis.KeyDeleteAsync(basketId);
        }


public async Task<UserBasketCheck?> GetBasketAsync(string basketId)
    {
        if (string.IsNullOrWhiteSpace(basketId))
        {
            // یا throw کنید یا null برگردونید – بسته به منطق برنامه‌تان
            return null;
        }

        try
        {
            var data = await _redis.StringGetAsync(basketId);

            if (data.IsNullOrEmpty)
            {
                return null; // سبد وجود ندارد یا منقضی شده
            }

            // دسیریالایز با گزینه‌های مناسب
            var basket = JsonSerializer.Deserialize<UserBasketCheck>(data!, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // در صورت تفاوت در نام‌گذاری پروپرتی‌ها
            });

            return basket;
        }
        catch (RedisConnectionException ex)
        {
            // لاگ کنید (مثلاً با ILogger)
            // _logger.LogError(ex, "Redis connection failed while getting basket {BasketId}", basketId);

            // رفتار fallback: مثلاً برگرداندن null یا سعی در بازیابی از دیتابیس
            return null;
        }
        catch (JsonException ex)
        {
           

            return null;
        }
    }



    public async Task<UserBasketCheck> UpdateBasketAsync(UserBasketCheck basket)
    {
        if (basket == null)
            throw new ArgumentNullException(nameof(basket));

        if (string.IsNullOrEmpty(basket.Id))
            throw new ArgumentException("Basket Id cannot be empty.", nameof(basket.Id));

        try
        {
            // سریالایز کردن آبجکت به JSON
            var serializedBasket = JsonSerializer.Serialize(basket);

            // ذخیره در Redis با انقضای 1 روز
            bool success = await _redis.StringSetAsync(
                key: basket.Id,
                value: serializedBasket,
                expiry: TimeSpan.FromDays(1));

            if (!success)
            {
                // خیلی نادر است که StringSet شکست بخورد مگر در شرایط خاص (مثل کلید قفل‌شده)
                // می‌توانید لاگ بزنید یا خطا برگردانید
                throw new InvalidOperationException("Failed to update basket in Redis.");
            }

            // بازگشت همان آبجکت به‌روزشده (یا می‌توانید از دیتابیس هم آپدیت کنید)
            return await GetBasketAsync(basket.Id);
        }
        catch (RedisConnectionException ex)
        {
            // مدیریت خطای اتصال به Redis
            // مثلاً لاگ کنید و رفتار fallback داشته باشید
            throw new InvalidOperationException("Redis connection failed while updating basket.", ex);
        }
    }
}
}
