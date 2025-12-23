namespace OnlineStore.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public string? SessionId { get; set; }  // ← nullable
        public string? UserId { get; set; }     // ← тоже nullable
    }
}