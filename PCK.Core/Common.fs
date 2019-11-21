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

    type Result = {
        CalculatedPrice : DecimalTwoDigits
        TaxAmount : decimal
        DiscountAmount : decimal
    }

    let private calculateDiscountAmount : decimal -> Discount list -> decimal = 
        fun price discounts ->
        discounts |> List.fold(fun result discount -> result + match discount with
                                                                        | NoDiscount -> 0M
                                                                        | Discount d -> DecimalTwoDigits.value d.Value * price / 100M) 0M

    let calculate : Product -> Tax -> Discount -> Discount -> Result =
        fun product tax discount upcDiscount ->
        let priceValue = DecimalTwoDigits.value product.Price
        let (Tax taxV) = tax
        let taxValue = DecimalTwoDigits.value taxV
        let discountAmountRulesBefore = calculateDiscountAmount priceValue ([discount; upcDiscount] |> List.filter (fun disc -> 
                                                                                                                        match disc with
                                                                                                                        | NoDiscount -> false
                                                                                                                        | Discount d ->
                                                                                                                            match d.ApplyRule with 
                                                                                                                            | Before -> true
                                                                                                                            | After -> false))
        let currentPriceValue = priceValue - discountAmountRulesBefore
        let discountAmountRulesAfter = calculateDiscountAmount currentPriceValue ([discount; upcDiscount] |> List.filter (fun disc -> 
                                                                                                                        match disc with
                                                                                                                        | NoDiscount -> false
                                                                                                                        | Discount d ->
                                                                                                                            match d.ApplyRule with 
                                                                                                                            | Before -> false
                                                                                                                            | After -> true))
        let discountValue = discountAmountRulesBefore + discountAmountRulesAfter
        let taxAmountTD = DecimalTwoDigits.create(currentPriceValue * taxValue / 100M)
        let discountAmountTD = DecimalTwoDigits.create(discountValue)
        let taxAmount = DecimalTwoDigits.value taxAmountTD
        let discountAmount = DecimalTwoDigits.value discountAmountTD
        { CalculatedPrice = DecimalTwoDigits.create (priceValue + taxAmount - discountAmount)
          TaxAmount = taxAmount
          DiscountAmount = discountAmount }





