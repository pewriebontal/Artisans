
using System.ComponentModel.DataAnnotations;
using Artisans.Models; 
using System.Collections.Generic;

namespace Artisans.Models
{
    public class CartItemViewModel 
    {
        public int ItemId { get; set; } // ProductId or MaterialId
        public string? ItemType { get; set; } 
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string? ImageUrl {get; set;}
    }

    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal CartTotal { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Full Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "City")]
        public string? ShippingCity { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? ShippingPostalCode { get; set; }

        
        [Required]
        [Display(Name = "Credit Card Number (use test numbers)")]
        [CreditCard] 
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Expiry Date must be in MM/YY format.")]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string CardExpiry { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVV.")]
        [Display(Name = "CVV")]
        public string CardCvv { get; set; } = string.Empty;
    }
}