
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Artisans.Core.Entities;
using Artisans.Core.Enums;
using Artisans.Features.Orders.ViewModels;
using Artisans.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; 
using System.Threading.Tasks;

namespace Artisans.Features.Orders
{
    [Authorize] 
    public class OrdersController : Controller
    {
        private readonly ArtisansDBContext _context;
        private readonly UserManager<User> _userManager;
        private const string CartSessionKey = "ShoppingCart";

        public OrdersController(ArtisansDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItemViewModel> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItemViewModel>() : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int itemId, string itemType, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(ci => ci.ItemId == itemId && ci.ItemType == itemType);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                if (itemType == "Product")
                {
                    var product = await _context.Products.FindAsync(itemId);
                    if (product != null && product.IsActive && product.StockQuantity >= quantity)
                    {
                        cart.Add(new CartItemViewModel { ItemId = product.Id, ItemType = "Product", Name = product.Name, UnitPrice = product.Price, Quantity = quantity, ImageUrl = product.MainImageUrl });
                    }
                    else { TempData["ErrorMessage"] = "Product not available or insufficient stock."; return Redirect(Request.Headers["Referer"].ToString() ?? "/"); }
                }
                else if (itemType == "Material")
                {
                    var material = await _context.Materials.FindAsync(itemId);
                     if (material != null && material.IsActive && material.StockQuantity >= quantity)
                    {
                        cart.Add(new CartItemViewModel { ItemId = material.Id, ItemType = "Material", Name = material.Name, UnitPrice = material.PricePerUnit, Quantity = quantity, ImageUrl = material.ImageUrl });
                    }
                    else { TempData["ErrorMessage"] = "Material not available or insufficient stock."; return Redirect(Request.Headers["Referer"].ToString() ?? "/"); }
                }
            }
            SaveCart(cart);
            TempData["SuccessMessage"] = "Item added to cart!";
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Browse/Products"); 
        }
        
        
        public IActionResult Cart()
        {
            var cart = GetCart();
            var model = new CheckoutViewModel 
            {
                CartItems = cart,
                CartTotal = cart.Sum(item => item.TotalPrice)
            };
            return View(model);
        }

        
        [HttpPost]
        public IActionResult UpdateCartQuantity(int itemId, string itemType, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(ci => ci.ItemId == itemId && ci.ItemType == itemType);
            if (item != null)
            {
                if (quantity > 0) item.Quantity = quantity;
                else cart.Remove(item); 
            }
            SaveCart(cart);
            return RedirectToAction(nameof(Cart));
        }

        
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId, string itemType)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(ci => ci.ItemId == itemId && ci.ItemType == itemType);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction(nameof(Cart));
        }


        
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }
            var model = new CheckoutViewModel
            {
                CartItems = cart,
                CartTotal = cart.Sum(item => item.TotalPrice)
            };
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            model.CartItems = cart; 
            model.CartTotal = cart.Sum(item => item.TotalPrice);

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty. Please add items before checking out.");
            }

            if (ModelState.IsValid)
            {
                
                
                bool paymentSuccessful = SimulatePayment(model.CardNumber);

                if (paymentSuccessful)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var order = new Order
                    {
                        BuyerUserId = currentUser.Id,
                        OrderDate = DateTime.UtcNow,
                        ShippingAddress = model.ShippingAddress,
                        ShippingCity = model.ShippingCity,
                        ShippingPostalCode = model.ShippingPostalCode,
                        TotalAmount = model.CartTotal,
                        Status = OrderStatus.Processing, 
                        OrderItems = new List<OrderItem>()
                    };

                    foreach (var cartItem in cart)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = cartItem.ItemType == "Product" ? (int?)cartItem.ItemId : null,
                            MaterialId = cartItem.ItemType == "Material" ? (int?)cartItem.ItemId : null,
                            Quantity = cartItem.Quantity,
                            UnitPriceAtPurchase = cartItem.UnitPrice
                        });
                        
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(CartSessionKey); 
                    TempData["SuccessMessage"] = $"Order #{order.Id} placed successfully! Thank you for your purchase.";
                    return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Payment declined. Please check your card details or use a different card.");
                }
            }
            return View(model); 
        }

        private bool SimulatePayment(string cardNumber)
        {
            
            
            if (cardNumber.EndsWith("0000")) return true; 
            if (cardNumber.EndsWith("1111")) return false; 
            return true; 
        }
        
        
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                                .Include(o => o.OrderItems)
                                    .ThenInclude(oi => oi.Product)
                                .Include(o => o.OrderItems)
                                    .ThenInclude(oi => oi.Material)
                                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerUserId == currentUser.Id);

            if (order == null) return NotFound();
            return View(order);
        }

         
        public async Task<IActionResult> MyOrders()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                                .Where(o => o.BuyerUserId == currentUser.Id)
                                .OrderByDescending(o => o.OrderDate)
                                .ToListAsync();
            return View(orders);
        }
    }
}