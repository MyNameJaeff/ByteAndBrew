using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Dtos.Customer
{
    public class CustomerUpdateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required, Phone, StringLength(20)]
        public string PhoneNumber { get; set; } = default!;
    }
}