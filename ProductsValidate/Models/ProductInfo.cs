using System.ComponentModel.DataAnnotations;

namespace ProductsValidate.Models
{
    public class ProductInfo
    {
        [Display(Name = "客户编号")]
        public string CustomId { get; set; }

        [Display(Name = "门店编号")]
        public string StoreId { get; set; }

        [Display(Name = "产品编号")]
        public string ProductId { get; set; }

        [Display(Name = "唯一码")]
        public string QRCode { get; set; }
    }
}