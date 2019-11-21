namespace PCK.Core

module Common =

    open DecimalTwoDigits
    
    type UPC = UPC of int
    type Name = Name of string

    type Product = {
        UPC: UPC
        Name: Name  
        Price: DecimalTwoDigits
    }

    type DiscountApplyRule =
    | Before
    | After

    type DiscountValue ={
        Value: DecimalTwoDigits
        ApplyRule: DiscountApplyRule
    }

    type Tax = Tax of DecimalTwoDigits
    type Discount =
    | Discount of DiscountValue
    | NoDiscount
    
    type Ammount =
    | AbsoluteValue of DecimalTwoDigits
    | Percentage of decimal

    type AdditionalCost = {
        Description : string
        Ammount : Ammount
    }

    type AdditionalCostResult = {
        Description : string
        Ammount : DecimalTwoDigits
    }
    
    type CombiningDiscountsMethod =
    | Additive
    | Multiplicative

    type Result = {
        CalculatedPrice : DecimalTwoDigits
        TaxAmount : decimal
        DiscountAmount : decimal
        AdditionalCostsResult : AdditionalCostResult list
    }

    let private calculateDiscountAmount : decimal -> Discount list -> decimal = 
        fun price discounts ->
        discounts |> List.fold(fun result discount -> result + match discount with
                                                                        | NoDiscount -> 0M
                                                                        | Discount d -> DecimalTwoDigits.value d.Value * price / 100M) 0M

    let private calculateAdditionalCost : DecimalTwoDigits -> AdditionalCost -> AdditionalCostResult =
        fun prpductPrice additionalCost ->
        match additionalCost.Ammount with
        | AbsoluteValue av -> {
                                Description = additionalCost.Description
                                Ammount = av
                              }
        | Percentage p -> let priceValue = DecimalTwoDigits.value prpductPrice
                          {
                            Description = additionalCost.Description
                            Ammount = DecimalTwoDigits.create (priceValue * p / 100M)
                          }

    let private matchDiscountApplyRuleBefore : Discount -> bool =
        fun discount ->
        match discount with
        | NoDiscount -> false
        | Discount d ->
            match d.ApplyRule with 
            | Before -> true
            | After -> false

    let private matchDiscountApplyRuleAfter : Discount -> bool =
        fun discount ->
        match discount with
        | NoDiscount -> false
        | Discount d ->
            match d.ApplyRule with 
            | Before -> false
            | After -> true

    let private getPriceByCombiningDiscountsMethod : CombiningDiscountsMethod -> decimal -> decimal -> decimal =
        fun combiningDiscountsMethod currentPrice currentDiscountValue ->
        match combiningDiscountsMethod with
                            | Additive -> currentPrice
                            | Multiplicative -> currentPrice - currentDiscountValue

    let calculate : Product -> Tax -> Discount -> Discount -> AdditionalCost list -> CombiningDiscountsMethod-> Result =
        fun product (Tax tax) discount upcDiscount additionalCosts combiningDiscountsMethod ->
        let priceValue = DecimalTwoDigits.value product.Price
        let taxValue = DecimalTwoDigits.value tax

        let beforeRuleDiscountAmmount = calculateDiscountAmount priceValue ([discount] |> List.filter matchDiscountApplyRuleBefore)
        let beforeRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod priceValue beforeRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleBefore)
        let discountAmountRulesBefore = beforeRuleDiscountAmmount + beforeRuleUpcDiscountAmmount
        let currentPriceValue = priceValue - discountAmountRulesBefore
        
        let afterRuleDiscountAmmount = calculateDiscountAmount currentPriceValue ([discount] |> List.filter matchDiscountApplyRuleAfter)
        let afterRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod currentPriceValue afterRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleAfter)
        let discountAmountRulesAfter = afterRuleDiscountAmmount + afterRuleUpcDiscountAmmount

        let discountValue = discountAmountRulesBefore + discountAmountRulesAfter
        let taxAmountTD = DecimalTwoDigits.create(currentPriceValue * taxValue / 100M)
        let discountAmountTD = DecimalTwoDigits.create(discountValue)
        let taxAmount = DecimalTwoDigits.value taxAmountTD
        let discountAmount = DecimalTwoDigits.value discountAmountTD
        let totalAdditionalCosts = additionalCosts |> List.map(fun ac -> calculateAdditionalCost product.Price ac)
        { CalculatedPrice = DecimalTwoDigits.create (priceValue + taxAmount - discountAmount + (totalAdditionalCosts |> List.sumBy(fun ac -> DecimalTwoDigits.value ac.Ammount)))
          TaxAmount = taxAmount
          DiscountAmount = discountAmount
          AdditionalCostsResult = totalAdditionalCosts }





