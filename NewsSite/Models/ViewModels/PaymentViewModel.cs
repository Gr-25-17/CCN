using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Name of card owner")]
        public required string CardName { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits.")]
        public required string CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiration Date")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry date must be in MM/YY format.")]
        public required string ExpirationDate { get; set; }

        [Required]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be 3 digits.")]
        public required string CVV { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
