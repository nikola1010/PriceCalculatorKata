using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCK.Application
{
    public class BasicUseCase
    {
        private readonly List<Core.Common.Product> products = new List<Core.Common.Product>();
        private Discount discount;
        private Dictionary<int, Discount> productUpcDiscounts = new Dictionary<int, Discount>();
        private DiscountCup discountCup;

        public BasicUseCase()
        {
            products.Add(new Core.Common.Product(Core.Common.UPC.NewUPC(12345),
                                                 Core.Common.Name.NewName("The Little Prince"),
                                                 Core.DecimalTwoDigits.create(20.25M)));
        }

        public Result Execute(int id, decimal taxValue, List<AdditionalCost> additionalCosts, CombiningDiscountsMethod combiningDiscountsMethod)
        {
            if (discountCup == null) throw new ArgumentNullException(nameof(discountCup));

            var product = products.First(x => x.UPC.Item == id);
            Discount upcDiscount;
            productUpcDiscounts.TryGetValue(id, out upcDiscount);

            var result = Core.Common.calculate(product,
                                                Core.Common.Tax.NewTax(taxValue),
                                                discount == null ? Core.Common.Discount.NoDiscount :
                                                                Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(discount.Value, discount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)),
                                                upcDiscount == null ? Core.Common.Discount.NoDiscount :
                                                                    Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(upcDiscount.Value, upcDiscount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)),
                                                ListModule.OfSeq(additionalCosts.Select(ac => new Core.Common.AdditionalCost(ac.Description,
                                                                                                                                ac.Type == AdditionalCostType.Absolute ? Core.Common.Ammount.NewAbsoluteValue(Core.DecimalTwoDigits.create(ac.Value)) :
                                                                                                                                                                            Core.Common.Ammount.NewPercentage(ac.Value)))),
                                                combiningDiscountsMethod == CombiningDiscountsMethod.Additive ? Core.Common.CombiningDiscountsMethod.Additive : Core.Common.CombiningDiscountsMethod.Multiplicative,
                                                discountCup.Type == DiscountCupType.Absolute ? Core.Common.DiscountCap.NewAbsoluteValueCup(Core.DecimalTwoDigits.create(discountCup.Value)) :
                                                                                                            Core.Common.DiscountCap.NewPercentageCup(discountCup.Value));

            return new Result(Core.DecimalTwoDigits.value(product.Price), Core.DecimalTwoDigits.value(result.CalculatedPrice), taxValue, discount, result.TaxAmount, result.DiscountAmount, upcDiscount, result.AdditionalCostsResult.Select(ac => new AdditionalCostResult(ac.Description, Core.DecimalTwoDigits.value(ac.Ammount))));
        }

        public void SetDiscountCup(DiscountCup discountCup)
        {
            this.discountCup = discountCup;
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

    public enum AdditionalCostType
    {
        Absolute,
        Percentage
    }

    public enum CombiningDiscountsMethod
    {
        Additive,
        Multiplicative
    }

    public enum DiscountCupType
    {
        Absolute,
        Percentage
    }

    public class DiscountCup
    {
        public DiscountCup(DiscountCupType type, decimal value)
        {
            Type = type;
            Value = value;
        }

        public DiscountCupType Type { get; }
        public decimal Value { get; }
    }

    public class AdditionalCost
    {
        public AdditionalCost(string description, AdditionalCostType type, decimal value)
        {
            Description = description;
            Type = type;
            Value = value;
        }

        public string Description { get; }
        public AdditionalCostType Type { get; }
        public decimal Value { get; }
    }

    public class AdditionalCostResult
    {
        public AdditionalCostResult(string description, decimal value)
        {
            Description = description;
            Value = value;
        }

        public string Description { get; }
        public decimal Value { get; }
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
        public Result(decimal initPrice, decimal calculatedPrice, decimal taxValue, Discount discountValue, decimal taxValueAmount, decimal discountValueAmount, Discount upcDiscountValue, IEnumerable<AdditionalCostResult> additionalCosts)
        {
            InitPrice = initPrice;
            CalculatedPrice = calculatedPrice;
            TaxValue = taxValue;
            DiscountValue = discountValue;
            TaxValueAmount = taxValueAmount;
            DiscountValueAmount = discountValueAmount;
            UpcDiscountValue = upcDiscountValue;
            AdditionalCosts = additionalCosts;
        }

        public decimal InitPrice { get; }
        public decimal CalculatedPrice { get; }
        public decimal TaxValue { get; }
        public Discount DiscountValue { get; }
        public decimal TaxValueAmount { get; }
        public decimal DiscountValueAmount { get; }
        public Discount UpcDiscountValue { get; }
        public IEnumerable<AdditionalCostResult> AdditionalCosts { get; }

        public override string ToString()
        {
            var discountValueText = string.Empty;
            var discountAmountText = string.Empty;

            if (DiscountValue != null)
            {
                discountValueText = $" and {DiscountValue.Value} % ({DiscountValue.Rule}) discount";
                discountAmountText = $", discount amount: ${DiscountValueAmount.ToString("0.00")}";
            }

            if (UpcDiscountValue != null)
            {
                discountValueText += $" and {UpcDiscountValue.Value} % ({UpcDiscountValue.Rule}) UPC discount";
            }

            return $"Product price reported as ${InitPrice} before tax and ${CalculatedPrice.ToString("0.00")} after {TaxValue} % tax{discountValueText}. Tax amount: ${TaxValueAmount}{discountAmountText}.\n{string.Join("\n", AdditionalCosts.Select(x => $"{x.Description} - ${x.Value}"))}";
        }
    }
}
