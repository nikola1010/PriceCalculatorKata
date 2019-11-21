using System.Collections.Generic;
using System.Linq;

namespace PCK.Application
{
    public class BasicUseCase
    {
        private readonly List<Core.Common.Product> products = new List<Core.Common.Product>();
        private Discount discount;
        private Dictionary<int, Discount> productUpcDiscounts = new Dictionary<int, Discount>();

        public BasicUseCase()
        {
            products.Add(new Core.Common.Product(Core.Common.UPC.NewUPC(12345),
                                                 Core.Common.Name.NewName("The Little Prince"),
                                                 Core.DecimalTwoDigits.create(20.25M)));
        }

        public Result Execute(int id, decimal taxValue)
        {
            var product = products.First(x => x.UPC.Item == id);
            Discount upcDiscount;
            productUpcDiscounts.TryGetValue(id, out upcDiscount);

            var result = Core.Common.calculate(product,
                                                Core.Common.Tax.NewTax(Core.DecimalTwoDigits.create(taxValue)),
                                                discount == null ? Core.Common.Discount.NoDiscount :
                                                                Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(Core.DecimalTwoDigits.create(discount.Value), discount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)),
                                                upcDiscount == null ? Core.Common.Discount.NoDiscount :
                                                                    Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(Core.DecimalTwoDigits.create(upcDiscount.Value), upcDiscount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)));

            return new Result(Core.DecimalTwoDigits.value(product.Price), Core.DecimalTwoDigits.value(result.CalculatedPrice), taxValue, discount, result.TaxAmount, result.DiscountAmount, upcDiscount);
        }

        public void SetDiscount(Discount discount)
        {
            this.discount = discount;
        }

        public void SetUPCDiscount(int ucd, Discount discount)
        {
            productUpcDiscounts.Add(ucd, discount);
        }

        public void ClearAllUPCDiscounts()
        {
            productUpcDiscounts = new Dictionary<int, Discount>();
        }
    }

    public enum DiscountRule
    {
        Before,
        After
    }

    public class Discount
    {
        public Discount(decimal value, DiscountRule rule)
        {
            Value = value;
            Rule = rule;
        }

        public decimal Value { get; }
        public DiscountRule Rule { get; }
    }

    public class Result
    {
        public Result(decimal initPrice, decimal calculatedPrice, decimal taxValue, Discount discountValue, decimal taxValueAmount, decimal discountValueAmount, Discount upcDiscountValue)
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
        public Discount DiscountValue { get; }
        public decimal TaxValueAmount { get; }
        public decimal DiscountValueAmount { get; }
        public Discount UpcDiscountValue { get; }

        public override string ToString()
        {
            var discountValueText = string.Empty;
            var discountAmountText = string.Empty;

            if (DiscountValue != null)
            {
                discountValueText = $" and {DiscountValue.Value} % ({DiscountValue.Rule}) discount";
                discountAmountText = $", discount amount: ${DiscountValueAmount}";
            }

            if (UpcDiscountValue != null)
            {
                discountValueText += $" and {UpcDiscountValue.Value} % ({UpcDiscountValue.Rule}) UPC discount";
            }

            return $"Product price reported as ${InitPrice} before tax and ${CalculatedPrice.ToString("0.00")} after {TaxValue} % tax{discountValueText}. Tax amount: ${TaxValueAmount}{discountAmountText}.";
        }
    }
}
