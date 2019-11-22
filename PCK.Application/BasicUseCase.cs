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
        private decimal tax;
        private CombiningDiscountsMethod combiningDiscountsMethod;
        private List<AdditionalCost> additionalCosts = new List<AdditionalCost>();

        public BasicUseCase()
        {
            products.Add(new Core.Common.Product(Core.Common.UPC.NewUPC(12345),
                                                 Core.Common.Name.NewName("The Little Prince"),
                                                 new Core.Common.Price(Core.DecimalFourDigits.create(20.25M), Core.Common.Currency.NewCurrency("USD"))));
        }

        public Result Execute(int id)
        {
            var product = products.First(x => x.UPC.Item == id);
            Discount upcDiscount;
            productUpcDiscounts.TryGetValue(id, out upcDiscount);

            var result = Core.Common.calculate(product,
                                                Core.Common.Tax.NewTax(tax),
                                                discount == null ? Core.Common.Discount.NoDiscount :
                                                                Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(discount.Value, discount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)),
                                                upcDiscount == null ? Core.Common.Discount.NoDiscount :
                                                                    Core.Common.Discount.NewDiscount(new Core.Common.DiscountValue(upcDiscount.Value, upcDiscount.Rule == DiscountRule.After ? Core.Common.DiscountApplyRule.After : Core.Common.DiscountApplyRule.Before)),
                                                ListModule.OfSeq(additionalCosts.Select(ac => new Core.Common.AdditionalCost(ac.Description,
                                                                                                                                ac.Type == AdditionalCostType.Absolute ? Core.Common.Ammount.NewAbsoluteValue(new Core.Common.Price(Core.DecimalFourDigits.create(ac.Value), Core.Common.Currency.NewCurrency(ac.Currency))) :
                                                                                                                                                                            Core.Common.Ammount.NewPercentage(ac.Value)))),
                                                combiningDiscountsMethod == CombiningDiscountsMethod.Additive ? Core.Common.CombiningDiscountsMethod.Additive : Core.Common.CombiningDiscountsMethod.Multiplicative,
                                                discountCup == null ? Core.Common.DiscountCap.NoDiscountCap :
                                                                        discountCup.Type == DiscountCupType.Absolute ? Core.Common.DiscountCap.NewAbsoluteValueCup(new Core.Common.Price(Core.DecimalFourDigits.create(discountCup.Value), Core.Common.Currency.NewCurrency(discountCup.Currency))) :
                                                                                                                        Core.Common.DiscountCap.NewPercentageCup(discountCup.Value));

            if (result.IsOk)
            {
                return new Result(new Price(Core.DecimalTwoDigits.value(result.ResultValue.InitialPrice.Value), result.ResultValue.InitialPrice.Currency.Item),
                                    new Price(Core.DecimalTwoDigits.value(result.ResultValue.CalculatedPrice.Value), result.ResultValue.CalculatedPrice.Currency.Item),
                                    tax,
                                    discount,
                                    new Price(Core.DecimalTwoDigits.value(result.ResultValue.TaxAmount.Value), result.ResultValue.TaxAmount.Currency.Item),
                                    new Price(Core.DecimalTwoDigits.value(result.ResultValue.DiscountAmount.Value), result.ResultValue.DiscountAmount.Currency.Item),
                                    upcDiscount,
                                    result.ResultValue.AdditionalCostsResult.Select(ac => new AdditionalCostResult(ac.Description, Core.DecimalTwoDigits.value(ac.Ammount.Value), ac.Ammount.Currency.Item)));
            }
            else
            {
                throw new InvalidOperationException($"Errors: {string.Join(";\n", result.ErrorValue.Select(x => $"{x.Name} - {x.Description}"))}");
            }
        }

        public void AddAdditionalCost(AdditionalCost additionalCost) => additionalCosts.Add(additionalCost);

        public void ClearAddAdditionalCosts() => additionalCosts = new List<AdditionalCost>();

        public void SetCombiningDiscountsMethod(CombiningDiscountsMethod combiningDiscountsMethod) => this.combiningDiscountsMethod = combiningDiscountsMethod;

        public void SetTax(decimal tax) => this.tax = tax;

        public void SetDiscountCup(DiscountCup discountCup) => this.discountCup = discountCup;

        public void ClearDiscountCup() => discountCup = null;

        public void SetDiscount(Discount discount) => this.discount = discount;

        public void SetUPCDiscount(int ucd, Discount discount) => productUpcDiscounts.Add(ucd, discount);

        public void ClearAllUPCDiscounts() => productUpcDiscounts = new Dictionary<int, Discount>();
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
        public AdditionalCostResult(string description, decimal value, string currency)
        {
            Description = description;
            Value = value;
            Currency = currency;
        }

        public string Description { get; }
        public decimal Value { get; }
        public string Currency { get; }
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
        public Result(Price initPrice, Price calculatedPrice, decimal tax, Discount discountValue, Price taxAmount, Price discountValueAmount, Discount upcDiscountValue, IEnumerable<AdditionalCostResult> additionalCosts)
        {
            InitPrice = initPrice;
            CalculatedPrice = calculatedPrice;
            Tax = tax;
            DiscountValue = discountValue;
            TaxAmount = taxAmount;
            DiscountValueAmount = discountValueAmount;
            UpcDiscountValue = upcDiscountValue;
            AdditionalCosts = additionalCosts;
        }

        public Price InitPrice { get; }
        public Price CalculatedPrice { get; }
        public decimal Tax { get; }
        public Discount DiscountValue { get; }
        public Price TaxAmount { get; }
        public Price DiscountValueAmount { get; }
        public Discount UpcDiscountValue { get; }
        public IEnumerable<AdditionalCostResult> AdditionalCosts { get; }

        public override string ToString()
        {
            var decimalOutputFormat = "0.00";

            var discountValueText = string.Empty;
            var discountAmountText = string.Empty;

            if (DiscountValue != null)
            {
                discountValueText = $" and {DiscountValue.Value} % ({DiscountValue.Rule}) discount";
                discountAmountText = $", discount amount: {DiscountValueAmount.Value.ToString(decimalOutputFormat)} {DiscountValueAmount.Currency}";
            }

            if (UpcDiscountValue != null)
            {
                discountValueText += $" and {UpcDiscountValue.Value} % ({UpcDiscountValue.Rule}) UPC discount";
            }

            return $"Product price reported as {InitPrice.Value} {InitPrice.Currency} before tax and {CalculatedPrice.Value.ToString(decimalOutputFormat)} {CalculatedPrice.Currency} after {Tax} % tax{discountValueText}. Tax amount: {TaxAmount.Value.ToString(decimalOutputFormat)} {TaxAmount.Currency}{discountAmountText}.\n{string.Join("\n", AdditionalCosts.Select(x => $"{x.Description} - {x.Value.ToString(decimalOutputFormat)} {x.Currency}"))}";
        }
    }
}
