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
                                                 new Core.Common.Price(Core.DecimalTwoDigits.create(20.25M), Core.Common.Currency.NewCurrency("USD"))));

            products.Add(new Core.Common.Product(Core.Common.UPC.NewUPC(123),
                                                 Core.Common.Name.NewName("Mali Princ"),
                                                 new Core.Common.Price(Core.DecimalTwoDigits.create(20.25M), Core.Common.Currency.NewCurrency("RSD"))));
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
                                                                                                                                ac.Type == AdditionalCostType.Absolute ? Core.Common.Ammount.NewAbsoluteValue(new Core.Common.Price(Core.DecimalTwoDigits.create(ac.Value), Core.Common.Currency.NewCurrency(ac.Currency))) :
                                                                                                                                                                            Core.Common.Ammount.NewPercentage(ac.Value)))),
                                                combiningDiscountsMethod == CombiningDiscountsMethod.Additive ? Core.Common.CombiningDiscountsMethod.Additive : Core.Common.CombiningDiscountsMethod.Multiplicative,
                                                discountCup.Type == DiscountCupType.Absolute ? Core.Common.DiscountCap.NewAbsoluteValueCup(new Core.Common.Price(Core.DecimalTwoDigits.create(discountCup.Value), Core.Common.Currency.NewCurrency(discountCup.Currency))) :
                                                                                                            Core.Common.DiscountCap.NewPercentageCup(discountCup.Value));

            if (result.IsOk)
            {
                return new Result(new Price(Core.DecimalTwoDigits.value(product.Price.Value), product.Price.Currency.Item),
                                    Core.DecimalTwoDigits.value(result.ResultValue.CalculatedPrice.Value),
                                    taxValue,
                                    discount,
                                    result.ResultValue.TaxAmount,
                                    result.ResultValue.DiscountAmount,
                                    upcDiscount,
                                    result.ResultValue.AdditionalCostsResult.Select(ac => new AdditionalCostResult(ac.Description, Core.DecimalTwoDigits.value(ac.Ammount.Value))));
            }
            else
            {
                throw new InvalidOperationException($"Errors: {string.Join(";\n", result.ErrorValue.Select(x => $"{x.Name} - {x.Description}"))}");
            }
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

    public class Price
    {
        public Price(decimal value, string currency)
        {
            Value = value;
            Currency = currency;
        }

        public decimal Value { get; }
        public string Currency { get; }
    }

    public class DiscountCup
    {
        public DiscountCup(DiscountCupType type, decimal value, string currency) //TODO: no need for currency for Percentage type 
        {
            Type = type;
            Value = value;
            Currency = currency;
        }

        public DiscountCupType Type { get; }
        public decimal Value { get; }
        public string Currency { get; }
    }

    public class AdditionalCost
    {
        public AdditionalCost(string description, AdditionalCostType type, decimal value, string currency) //TODO: no need for currency for Percentage type 
        {
            Description = description;
            Type = type;
            Value = value;
            Currency = currency;
        }

        public string Description { get; }
        public AdditionalCostType Type { get; }
        public decimal Value { get; }
        public string Currency { get; }
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
        public Result(Price initPrice, decimal calculatedPrice, decimal taxValue, Discount discountValue, decimal taxValueAmount, decimal discountValueAmount, Discount upcDiscountValue, IEnumerable<AdditionalCostResult> additionalCosts)
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

        public Price InitPrice { get; }
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
                discountAmountText = $", discount amount: {DiscountValueAmount.ToString("0.00")} {InitPrice.Currency}";
            }

            if (UpcDiscountValue != null)
            {
                discountValueText += $" and {UpcDiscountValue.Value} % ({UpcDiscountValue.Rule}) UPC discount";
            }

            return $"Product price reported as {InitPrice.Value} {InitPrice.Currency} before tax and {CalculatedPrice.ToString("0.00")} {InitPrice.Currency} after {TaxValue} % tax{discountValueText}. Tax amount: {TaxValueAmount} {InitPrice.Currency}{discountAmountText}.\n{string.Join("\n", AdditionalCosts.Select(x => $"{x.Description} - {x.Value} {InitPrice.Currency}"))}";
        }
    }
}
