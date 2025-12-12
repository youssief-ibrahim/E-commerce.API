using AutoMapper;
using E_commerce.Core.Basic;
using E_commerce.Core.DTO.Category;
using E_commerce.Core.DTO.OrderC;
using E_commerce.Core.DTO.OrderItem;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IGenericReposatory<Order> GenOrder;
        private readonly IGenericReposatory<CartItem> GenCartItem;
        private readonly IGenericReposatory<OrderItem> GenOrderItem;
        private readonly IGenericReposatory<Product> GenProduct;
        private readonly IMapper Mapper;
        private readonly IStringLocalizer<Order> localizer;
        private readonly ResponseHandler responesHandler;

        public OrderController(IGenericReposatory<Order> GenOrder, IGenericReposatory<CartItem> GenCartItem, IGenericReposatory<OrderItem> GenOrderItem, IGenericReposatory<Product> GenProduct, IMapper Mapper, IStringLocalizer<Order> localizer, ResponseHandler responesHandler)
        {
            this.GenOrder = GenOrder;
            this.GenCartItem = GenCartItem;
            this.GenOrderItem = GenOrderItem;
            this.GenProduct = GenProduct;
            this.Mapper = Mapper;
            this.localizer = localizer;
            this.responesHandler = responesHandler;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await GenOrder.GetAll(r => r.User);
            var data = Mapper.Map<List<AllOrderDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (res == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
            var data = Mapper.Map<AllOrderDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("User/{userid}")]
        public async Task<IActionResult> Getorderofusers(int userid)
        {
            var res = await GenOrder.FindAll(s => s.UserId == userid, r => r.User);
            if (res.Count == 0) return NotFound(responesHandler.NotFound<Order>($"Order with id {userid} not found"));
            var data = Mapper.Map<List<AllOrderDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpPost("CheckOut")]
        public async Task<IActionResult> CheckOut([FromBody] CreateOrderDTO createOrder)
        {
            var cartitems = await GenCartItem.FindAll(c => c.ShoppingCart.UserId == createOrder.UserId,r=>r.Product,r=>r.ShoppingCart);
            
            if (cartitems.Count == 0) return BadRequest(localizer["cartEmpty"].Value);
            // check stock
            foreach (var item in cartitems)
            {
                if (item.Product.Quantity < item.Quantity)
                    return BadRequest($"{localizer["insufficientStock"].Value} {item.Product.Name}");
            }

            var order = new Order
            {
                UserId = createOrder.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = cartitems.Sum(c => c.Quantity * c.Product.Price),
                Status = "Pending",
            };
            await GenOrder.Create(order);
            GenOrder.Save(); // to get order id

            foreach (var item in cartitems)
            {
                item.Product.Quantity -= item.Quantity;
                GenProduct.update(item.Product);

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };

                await GenOrderItem.Create(orderItem);

                GenCartItem.delete(item);
            }

            GenOrderItem.Save();
            GenProduct.Save();
            GenCartItem.Save();

            var data = Mapper.Map<AllOrderDTO>(order);
            return Ok(responesHandler.Success(data));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (res == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
            GenOrder.delete(res);
            GenOrder.Save();

            var data = Mapper.Map<AllOrderDTO>(res);
            return Ok(responesHandler.Deleted(data));
        }
        [HttpPut("Cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var res = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (res == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
            res.Status = "Cancelled";
            GenOrder.update(res);
            GenOrder.Save();
            var data = Mapper.Map<AllOrderDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpPost("Reorder/{id}")]
        public async Task<IActionResult> Reorder(int id)
        {
            var existingOrder = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (existingOrder == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
           
            var existingOrderItems = await GenOrderItem.FindAll(oi => oi.OrderId == existingOrder.Id);
            foreach (var item in existingOrderItems)
            {
                var product = await GenProduct.GetById(p => p.Id == item.ProductId);
                if (product.Quantity < item.Quantity)
                {
                    return BadRequest($"{localizer["insufficientStock"].Value} {product.Name}");
                }
                product.Quantity -= item.Quantity;
            }
            var newOrder = new Order
            {
                UserId = existingOrder.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = existingOrder.TotalAmount,
                Status = "Pending",
            };
            await GenOrder.Create(newOrder);
            GenOrder.Save();
            foreach (var item in existingOrderItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                await GenOrderItem.Create(orderItem);
            }
            GenOrderItem.Save();
            var data = Mapper.Map<AllOrderDTO>(newOrder);
            return Ok(responesHandler.Success(data));

        }

        [HttpGet("Status/{id}")]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var res = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (res == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
            return Ok(responesHandler.Success(new { res.Id, res.Status }));
        }
        [HttpPut("Status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var res = await GenOrder.GetById(s => s.Id == id, r => r.User);
            if (res == null) return NotFound(responesHandler.NotFound<Order>($"Order with id {id} not found"));
            res.Status = status;
            GenOrder.update(res);
            GenOrder.Save();
            var data = Mapper.Map<AllOrderDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Item/{id}")]
        public async Task<IActionResult> GetOrderItems(int id)
        {
            var orderItem = await GenOrderItem.GetById(oi => oi.Id == id);
            if (orderItem == null)
                return NotFound(responesHandler.NotFound<OrderItem>($"Order item with id {id} not found"));
            var data = Mapper.Map<AllOrderItemDTO>(orderItem);
            return Ok(responesHandler.Success(data));
        }
    }
}
