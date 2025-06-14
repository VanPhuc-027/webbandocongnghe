using System.ComponentModel.DataAnnotations;

namespace _2280613193_webdocongnghe.Models
{
    public class CheckoutViewModel
    {
        [Required]
        public string ShippingAddress { get; set; }

        public string Notes { get; set; }

        public decimal TotalPrice { get; set; }

        public List<CartItem> CartItems { get; set; }
    }
}
