namespace PCK.Core

module Common =

    //type DecimalTwoDigits = DecimalTwoDigits of decimal
    //let create (value:decimal) = 
    //    let integralValue = truncate value
    //    let fraction = value - integralValue
    //    let truncatedFraction = truncate (fraction * 100M) / 100M
    //    DecimalTwoDigits (integralValue + truncatedFraction)
    ////let apply f (DecimalTwoDigits d) = f d
    ////let value d = apply id d
    open DecimalTwoDigits
    
    type UPC = UPC of int
    type Name = Name of string

    type Product = {
        UPC: UPC
        Name: Name  
        Price: DecimalTwoDigits
    }

    type Tax = Tax of decimal

    type Result = {
        InitialPrice : DecimalTwoDigits
        CalculatedPrice : DecimalTwoDigits
        Tax : Tax
    }

    let calculate : Product -> Tax -> DecimalTwoDigits =
        fun product tax ->
        let priceValue = DecimalTwoDigits.value product.Price
        let (Tax taxValue) = tax
        DecimalTwoDigits.create (priceValue + priceValue * taxValue / 100M)





