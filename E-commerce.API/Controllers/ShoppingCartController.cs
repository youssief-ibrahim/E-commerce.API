using System.Security.Claims;
using AutoMapper;
using E_commerce.Core.Basic;
using E_commerce.Core.DTO.Category;
using E_commerce.Core.DTO.ShoppingCart;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using E_commerce.EF.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IGenericReposatory<ShoppingCart> GenShopingCart;
        private readonly IGenericReposatory<Product> GenProduct;
        private readonly IGenericReposatory<CartItem> GenCartItem;
        private readonly IMapper Mapper;
        private readonly IStringLocalizer<ShoppingCart> localizer;
        private readonly ResponseHandler responesHandler;
        private readonly ApplicationDbContext context;

        public ShoppingCartController(IGenericReposatory<ShoppingCart> GenShopingCart, IGenericReposatory<Product> GenProduct, IGenericReposatory<CartItem> GenCartItem, IMapper Mapper, IStringLocalizer<ShoppingCart> localizer, ResponseHandler responesHandler, ApplicationDbContext context)
        {
            this.GenShopingCart = GenShopingCart;
            this.GenProduct = GenProduct;
            this.GenCartItem = GenCartItem;
            this.Mapper = Mapper;
            this.localizer = localizer;
            this.responesHandler = responesHandler;
            this.context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {

            var carts = await context.ShoppingCarts
          .Include(c => c.User)
           .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .ToListAsync();

            var data = Mapper.Map<List<AllShoppingCartDTO>>(carts);

            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var cart = await context.ShoppingCarts
          .Include(c => c.User)
           .Include(c => c.CartItems)
          .ThenInclude(ci => ci.Product)
          .FirstOrDefaultAsync(c => c.Id == id);
            if (cart == null)
            {
                return NotFound(responesHandler.BadRequestt<AllShoppingCartDTO>(localizer["NotFound"]));
            }
            var data = Mapper.Map<AllShoppingCartDTO>(cart);
            return Ok(responesHandler.Success(data));
        }
        [HttpPost]
        [Authorize]

        public async Task<IActionResult> AddToCart( int productId, int quantity)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var userfount = await context.Users.FindAsync(userId);
            if (userfount == null)
            {
                return NotFound(localizer["usernotfound"].Value);
            }
            // find the cart for the user
            var cart = await context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
               .FirstOrDefaultAsync(c => c.UserId == userId);

            // if no cart, create one
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };

                await GenShopingCart.Create(cart);
                GenShopingCart.Save();
            }

            // Check if product exists in DB
            var product = await GenProduct.GetById(p => p.Id == productId);
            if (product == null)
                return NotFound(localizer["productNotFound"].Value);

            // check if product already in cart
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
           
            int existingQty = cartItem?.Quantity ?? 0; 
            int totalRequested = existingQty + quantity;

            if (product.Quantity < totalRequested) 
            {
                int canAdd = product.Quantity - existingQty;

                if (canAdd <= 0)
                    return BadRequest(localizer["outOfStockMaxAdded"].Value);

                return BadRequest($"{localizer["onlyItemsAllowed"].Value} {canAdd}");
            }

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    ShoppingCartId = cart.Id
                };

                await GenCartItem.Create(cartItem);
                GenCartItem.Save();    
            }
            else
            {
                cartItem.Quantity += quantity;
                GenCartItem.update(cartItem);
                GenCartItem.Save();
            }

            var data = Mapper.Map<AllShoppingCartDTO>(cart);

            return Ok(responesHandler.Success(data));
        }

        [HttpPut("Update/{productid}")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem(int productid, int quantity)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await context.ShoppingCarts
        .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
       .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(localizer["shoppingCartNotFound"].Value);
            }
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productid);
            if (cartItem == null)
            {
                return NotFound(localizer["productNotFoundInCart"].Value);
            }
            var product = await GenProduct.GetById(p => p.Id == productid);
            if (product == null)
                return NotFound(localizer["productNotFound"].Value);
            if (product.Quantity < quantity)
            {
                return BadRequest(localizer["requestedQuantityExceedsStock"].Value);
            }
            cartItem.Quantity = quantity;
            GenCartItem.update(cartItem);
            GenCartItem.Save();
            var data = Mapper.Map<AllShoppingCartDTO>(cart);
            return Ok(responesHandler.Success(data));
        }
        [HttpDelete("Remove/{productid}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int productid)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var cart = await context.ShoppingCarts
       .Include(c => c.CartItems)
       .ThenInclude(ci => ci.Product)
      .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(localizer["shoppingCartNotFound"].Value);
            }
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productid);
            if (cartItem == null)
            {
                return NotFound(localizer["productNotFoundInCart"].Value);
            }
            GenCartItem.delete(cartItem);
            GenCartItem.Save();
            var data = Mapper.Map<AllShoppingCartDTO>(cart);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Summary")]
        [Authorize]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(localizer["shoppingCartNotFound"].Value);
            }
            var totalItems = cart.CartItems.Sum(ci => ci.Quantity);
            var totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            var items = cart.CartItems.Select(ci => new
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                TotalQuantity = ci.Quantity,
                UnitePrice = ci.Product.Price,
                TotalPrice = ci.Quantity * ci.Product.Price
            });

            var summary = new
            {
                Items = items,
                TotalItems = totalItems,
                AllTotalPrice = totalPrice,
            };
            return Ok(responesHandler.Success(summary));
        }
        [HttpDelete("ClearCart")]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(localizer["shoppingCartNotFound"].Value);
            }
            context.CartItems.RemoveRange(cart.CartItems);
            await context.SaveChangesAsync();
            return Ok(responesHandler.Success(localizer["cartClearedSuccessfully"].Value));
        }

    }
}
