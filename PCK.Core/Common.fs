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

    type Tax = Tax of DecimalTwoDigits
    type Discount = Discount of DecimalTwoDigits

    type Result = {
        CalculatedPrice : DecimalTwoDigits
        TaxAmount : decimal
        DiscountAmount : decimal
    }

    let calculate : Product -> Tax -> Discount -> Result =
        fun product tax discount ->
        let priceValue = DecimalTwoDigits.value product.Price
        let (Tax taxV) = tax
        let taxValue = DecimalTwoDigits.value taxV
        let (Discount discountV) = discount
        let discountValue = DecimalTwoDigits.value discountV
        let taxAmountTD = DecimalTwoDigits.create(priceValue * taxValue / 100M)
        let discountAmountTD = DecimalTwoDigits.create(priceValue * discountValue / 100M)
        let taxAmount = DecimalTwoDigits.value taxAmountTD
        let discountAmount = DecimalTwoDigits.value discountAmountTD
        { CalculatedPrice = DecimalTwoDigits.create (priceValue + taxAmount - discountAmount)
          TaxAmount = taxAmount
          DiscountAmount = discountAmount }





