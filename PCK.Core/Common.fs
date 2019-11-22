namespace PCK.Core

module Common =

    open System
    open DecimalTwoDigits
    open DecimalFourDigits
    
    type UPC = UPC of int
    type Name = Name of string
    
    type Currency = Currency of string

    type Price = {
        Value: DecimalFourDigits
        Currency: Currency
    }

    type Product = {
        UPC: UPC
        Name: Name  
        Price: Price
    }

    type DiscountApplyRule =
    | Before
    | After

    type DiscountValue ={
        Value: decimal
        ApplyRule: DiscountApplyRule
    }

    type Tax = Tax of decimal
    type Discount =
    | Discount of DiscountValue
    | NoDiscount
    
    type Ammount =
    | AbsoluteValue of Price
    | Percentage of decimal

    type AdditionalCost = {
        Description : string
        Ammount : Ammount
    }
    
    type ResultPrice = {
        Value: DecimalTwoDigits
        Currency: Currency
    }

    type AdditionalCostResult = {
        Description : string
        Ammount : ResultPrice
    }
    
    type CombiningDiscountsMethod =
    | Additive
    | Multiplicative
    
    type DiscountCap =
    | AbsoluteValueCup of Price
    | PercentageCup of decimal
    | NoDiscountCap
    
    type ValidationError = {
        Name : string
        Description : string
    }

    type Result = {
        InitialPrice : ResultPrice
        CalculatedPrice : ResultPrice
        TaxAmount : ResultPrice
        DiscountAmount : ResultPrice
        AdditionalCostsResult : AdditionalCostResult list
    }

    let private calculateDiscountAmount : decimal -> Discount list -> decimal = 
        fun price discounts ->
        discounts |> List.fold(fun result discount -> result + match discount with
                                                                        | NoDiscount -> 0M
                                                                        | Discount d -> d.Value * price / 100M) 0M

    let private calculateAdditionalCost : Price -> AdditionalCost -> AdditionalCostResult =
        fun prpductPrice additionalCost ->
        match additionalCost.Ammount with
        | AbsoluteValue av -> { Description = additionalCost.Description
                                Ammount = { Value = DecimalTwoDigits.create (DecimalFourDigits.value av.Value)
                                            Currency = av.Currency } }
        | Percentage p -> { Description = additionalCost.Description
                            Ammount = { Value = DecimalTwoDigits.create ((DecimalFourDigits.value prpductPrice.Value) * p / 100M)
                                        Currency = prpductPrice.Currency } }

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

    let private calculateValid : Product -> Tax -> Discount -> Discount -> AdditionalCost list -> CombiningDiscountsMethod -> DiscountCap -> Result =
        fun product (Tax taxValue) discount upcDiscount additionalCosts combiningDiscountsMethod discountCap ->
        let priceValue = DecimalFourDigits.value product.Price.Value

        let beforeRuleDiscountAmmount = calculateDiscountAmount priceValue ([discount] |> List.filter matchDiscountApplyRuleBefore)
        let beforeRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod priceValue beforeRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleBefore)
        let discountAmountRulesBefore = beforeRuleDiscountAmmount + beforeRuleUpcDiscountAmmount
        let currentPriceValue = priceValue - discountAmountRulesBefore
        
        let afterRuleDiscountAmmount = calculateDiscountAmount currentPriceValue ([discount] |> List.filter matchDiscountApplyRuleAfter)
        let afterRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod currentPriceValue afterRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleAfter)
        let discountAmountRulesAfter = afterRuleDiscountAmmount + afterRuleUpcDiscountAmmount

        let discountValue = discountAmountRulesBefore + discountAmountRulesAfter
        let taxAmountTD = DecimalFourDigits.create(currentPriceValue * taxValue / 100M)
         
        let discountCapValue = 
            match discountCap with
            | NoDiscountCap -> Decimal.MaxValue
            | PercentageCup pc -> pc * priceValue / 100M
            | AbsoluteValueCup avc -> DecimalFourDigits.value avc.Value
        
        let discountAmountTD = DecimalFourDigits.create(min discountValue discountCapValue)
        let taxAmount = DecimalFourDigits.value taxAmountTD
        let discountAmount = DecimalFourDigits.value discountAmountTD
        let totalAdditionalCosts = additionalCosts |> List.map(fun ac -> calculateAdditionalCost product.Price ac)
        { InitialPrice = { Value = DecimalTwoDigits.create priceValue
                           Currency = product.Price.Currency}
          CalculatedPrice = { Value = DecimalTwoDigits.create (priceValue + taxAmount - discountAmount + (totalAdditionalCosts |> List.sumBy(fun ac -> DecimalTwoDigits.value ac.Ammount.Value)))
                              Currency = product.Price.Currency }
          TaxAmount = { Value = DecimalTwoDigits.create taxAmount
                        Currency = product.Price.Currency }
          DiscountAmount = { Value = DecimalTwoDigits.create discountAmount
                             Currency = product.Price.Currency }
          AdditionalCostsResult = totalAdditionalCosts }

    let private validateAdditionalCosts: Product -> AdditionalCost list -> ValidationError list -> ValidationError list =
           fun product additionalCosts validationErrors ->
               additionalCosts |> List.fold (fun validationErrors additionalCost -> 
                   match additionalCost.Ammount with
                   | Percentage p -> validationErrors
                   | AbsoluteValue av -> if (av.Currency = product.Price.Currency) then
                                            validationErrors
                                         else { Name = "AdditionalCost"; Description = "Currency mismatch. " + av.Currency.ToString() + " -> " + product.Price.Currency.ToString() + ". Description: " + additionalCost.Description } :: validationErrors) validationErrors

    let private validateDiscountCap: Product -> DiscountCap -> ValidationError list -> ValidationError list =
        fun product discountCap validationErrors ->
            match discountCap with
            | NoDiscountCap -> validationErrors
            | PercentageCup _ -> validationErrors
            | AbsoluteValueCup avc -> if (avc.Currency = product.Price.Currency) then
                                        validationErrors
                                      else {Name = "DiscountCap"; Description = "Currency mismatch. " + avc.Currency.ToString() + " -> " + product.Price.Currency.ToString() } :: validationErrors

    let private validate  : Product -> AdditionalCost list -> DiscountCap -> ValidationError list =
        fun product additionalCosts discountCap ->
        validateAdditionalCosts product additionalCosts []
                 |> validateDiscountCap product discountCap

    let calculate : Product -> Tax -> Discount -> Discount -> AdditionalCost list -> CombiningDiscountsMethod -> DiscountCap -> Result<Result, ValidationError list> =
        fun product tax discount upcDiscount additionalCosts combiningDiscountsMethod discountCap ->
        match validate product additionalCosts discountCap with
        | [] -> Ok (calculateValid product tax discount upcDiscount additionalCosts combiningDiscountsMethod discountCap)
        | validationErrors -> Error validationErrors
        


