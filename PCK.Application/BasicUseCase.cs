using System.Collections.Generic;
using System.Linq;

namespace PCK.Application
{
    public class BasicUseCase
    {
        private readonly List<Core.Common.Product> products = new List<Core.Common.Product>();
        private readonly decimal discount;

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

            var result = Core.Common.calculate(product, Core.Common.Tax.NewTax(Core.DecimalTwoDigits.create(taxValue)), Core.Common.Discount.NewDiscount(Core.DecimalTwoDigits.create(discount)));

            return new Result(Core.DecimalTwoDigits.value(product.Price), Core.DecimalTwoDigits.value(result.CalculatedPrice), taxValue, discount, result.TaxAmount, result.DiscountAmount);
        }
    }

    public class Result
    {
        public Result(decimal initPrice, decimal calculatedPrice, decimal taxValue, decimal discountValue, decimal taxValueAmount, decimal discountValueAmount)
        {
            InitPrice = initPrice;
            CalculatedPrice = calculatedPrice;
            TaxValue = taxValue;
            DiscountValue = discountValue;
            TaxValueAmount = taxValueAmount;
            DiscountValueAmount = discountValueAmount;
        }

        public decimal InitPrice { get; }
        public decimal CalculatedPrice { get; }
        public decimal TaxValue { get; }
        public decimal DiscountValue { get; }
        public decimal TaxValueAmount { get; }
        public decimal DiscountValueAmount { get; }

        public override string ToString()
        {
            return $"Product price reported as ${InitPrice} before tax and ${CalculatedPrice.ToString("0.00")} after {TaxValue} % tax and {DiscountValue} % discount. Tax amount: ${TaxValueAmount}, discount amount: ${DiscountValueAmount}";
        }
    }
}
