using System.Collections.Generic;
using System.Linq;

namespace PCK.Application
{
    public class BasicUseCase
    {
        private readonly List<Core.Common.Product> products = new List<Core.Common.Product>();
        private decimal discount;
        private Dictionary<int, decimal> productUpcDiscounts = new Dictionary<int, decimal>();

        public BasicUseCase()
        {
            products.Add(new Core.Common.Product(Core.Common.UPC.NewUPC(12345),
                                                 Core.Common.Name.NewName("The Little Prince"),
                                                 Core.DecimalTwoDigits.create(20.25M)));

            discount = 15;
        }

        public Result Execute(int id, decimal taxValue)
        {
            var product = products.First(x => x.UPC.Item == id);
            decimal upcDiscount = 0;
            productUpcDiscounts.TryGetValue(id, out upcDiscount);

            var result = Core.Common.calculate(product,
                                                Core.Common.Tax.NewTax(Core.DecimalTwoDigits.create(taxValue)),
                                                discount == 0 ? Core.Common.Discount.NoDiscount :
                                                                Core.Common.Discount.NewDiscount(Core.DecimalTwoDigits.create(discount)),
                                                upcDiscount == 0 ? Core.Common.Discount.NoDiscount :
                                                                    Core.Common.Discount.NewDiscount(Core.DecimalTwoDigits.create(upcDiscount)));

            return new Result(Core.DecimalTwoDigits.value(product.Price), Core.DecimalTwoDigits.value(result.CalculatedPrice), taxValue, discount, result.TaxAmount, result.DiscountAmount, upcDiscount);
        }

        public void SetDiscount(decimal discount)
        {
            this.discount = discount;
        }

        public void SetUPCDiscount(int ucd, decimal discount)
        {
            productUpcDiscounts.Add(ucd, discount);
        }

        public void ClearAllUPCDiscounts()
        {
            productUpcDiscounts = new Dictionary<int, decimal>();
        }
    }

    public class Result
    {
        public Result(decimal initPrice, decimal calculatedPrice, decimal taxValue, decimal discountValue, decimal taxValueAmount, decimal discountValueAmount, decimal upcDiscountValue)
        {
            InitPrice = initPrice;
            CalculatedPrice = calculatedPrice;
            TaxValue = taxValue;
            DiscountValue = discountValue;
            TaxValueAmount = taxValueAmount;
            DiscountValueAmount = discountValueAmount;
            UpcDiscountValue = upcDiscountValue;
        }

        public decimal InitPrice { get; }
        public decimal CalculatedPrice { get; }
        public decimal TaxValue { get; }
        public decimal DiscountValue { get; }
        public decimal TaxValueAmount { get; }
        public decimal DiscountValueAmount { get; }
        public decimal UpcDiscountValue { get; }

        public override string ToString()
        {
            var discountValueText = string.Empty;
            var discountAmountText = string.Empty;
            if (DiscountValue != 0)
            {
                discountValueText = $" and {DiscountValue} % discount";
                discountAmountText = $", discount amount: ${DiscountValueAmount}";
            }

            if (UpcDiscountValue != 0)
            {
                discountValueText += $" and {UpcDiscountValue} % UPC discount";
            }

            return $"Product price reported as ${InitPrice} before tax and ${CalculatedPrice.ToString("0.00")} after {TaxValue} % tax{discountValueText}. Tax amount: ${TaxValueAmount}{discountAmountText}.";
        }
    }
}
