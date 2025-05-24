namespace Artisans.Core.Enums
{
    public enum OrderStatus
    {
        Pending,        // Order placed, awaiting payment/processing
        Processing,     // Payment received, being prepared
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }
}