namespace PCK.Core

module Common =

    open DecimalTwoDigits
    
    type UPC = UPC of int
    type Name = Name of string
    
    type Currency = Currency of string

    type Price = {
        Value: DecimalTwoDigits
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

    type AdditionalCostResult = {
        Description : string
        Ammount : Price
    }
    
    type CombiningDiscountsMethod =
    | Additive
    | Multiplicative
    
    type DiscountCap =
    | AbsoluteValueCup of Price
    | PercentageCup of decimal
    
    type ValidationError = {
        Name : string
        Description : string
    }

    type Result = {
        CalculatedPrice : Price
        TaxAmount : decimal
        DiscountAmount : decimal
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
        | AbsoluteValue av -> {
                                Description = additionalCost.Description
                                Ammount = av
                              }
        | Percentage p -> let priceValue = DecimalTwoDigits.value prpductPrice.Value
                          {
                            Description = additionalCost.Description
                            Ammount = { Value = DecimalTwoDigits.create (priceValue * p / 100M)
                                        Currency = prpductPrice.Currency }
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

    let private calculateValid : Product -> Tax -> Discount -> Discount -> AdditionalCost list -> CombiningDiscountsMethod -> DiscountCap -> Result =
        fun product (Tax taxValue) discount upcDiscount additionalCosts combiningDiscountsMethod discountCap ->
        let priceValue = DecimalTwoDigits.value product.Price.Value

        let beforeRuleDiscountAmmount = calculateDiscountAmount priceValue ([discount] |> List.filter matchDiscountApplyRuleBefore)
        let beforeRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod priceValue beforeRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleBefore)
        let discountAmountRulesBefore = beforeRuleDiscountAmmount + beforeRuleUpcDiscountAmmount
        let currentPriceValue = priceValue - discountAmountRulesBefore
        
        let afterRuleDiscountAmmount = calculateDiscountAmount currentPriceValue ([discount] |> List.filter matchDiscountApplyRuleAfter)
        let afterRuleUpcDiscountAmmount = calculateDiscountAmount (getPriceByCombiningDiscountsMethod combiningDiscountsMethod currentPriceValue afterRuleDiscountAmmount) ([upcDiscount] |> List.filter matchDiscountApplyRuleAfter)
        let discountAmountRulesAfter = afterRuleDiscountAmmount + afterRuleUpcDiscountAmmount

        let discountValue = discountAmountRulesBefore + discountAmountRulesAfter
        let taxAmountTD = DecimalTwoDigits.create(currentPriceValue * taxValue / 100M)
         
        let discountCapValue = 
            match discountCap with
            | PercentageCup pc -> pc * priceValue / 100M
            | AbsoluteValueCup avc -> DecimalTwoDigits.value avc.Value
        
        let discountAmountTD = DecimalTwoDigits.create(min discountValue discountCapValue)
        let taxAmount = DecimalTwoDigits.value taxAmountTD
        let discountAmount = DecimalTwoDigits.value discountAmountTD
        let totalAdditionalCosts = additionalCosts |> List.map(fun ac -> calculateAdditionalCost product.Price ac)
        { CalculatedPrice = { Value = DecimalTwoDigits.create (priceValue + taxAmount - discountAmount + (totalAdditionalCosts |> List.sumBy(fun ac -> DecimalTwoDigits.value ac.Ammount.Value)))
                              Currency = product.Price.Currency }
          TaxAmount = taxAmount
          DiscountAmount = discountAmount
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
        


