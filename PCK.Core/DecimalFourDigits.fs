namespace PCK.Core

module DecimalFourDigits =
    open System

    type DecimalFourDigits = private DecimalFourDigits of decimal
    let create (value:decimal) = 
        DecimalFourDigits (Math.Round(value,4))
    let private apply f (DecimalFourDigits d) = f d
    let value d = apply id d
