using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class PaymentService(IConfiguration config, ICartService cartService, IUnitOfWork unitOfWork) : IPaymentService
{
    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null) return null;

        var shippingPrice = 0m;

        if (cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await unitOfWork.Repository<DeliveryMethod>().GetByIdAsync((int)cart.DeliveryMethodId);

            if (deliveryMethod == null) return null;

            shippingPrice = deliveryMethod.Price;
        }

        foreach (var item in cart.Items)
        {
            var productitem = await unitOfWork.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);

            if (productitem == null) return null;

            if (item.Price != productitem.Price)
            {
                item.Price = productitem.Price;
            }
        }

        var service = new PaymentIntentService();
        PaymentIntent? intent = null;

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) + (long)shippingPrice * 100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) + (long)shippingPrice * 100,
            };
            intent = await service.UpdateAsync(cart.PaymentIntentId, options);
        }

        await cartService.SetCartAsync(cart);

        return cart;
    }
}
