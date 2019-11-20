namespace PCK.Core

module DecimalTwoDigits =
    open System

    type DecimalTwoDigits = private DecimalTwoDigits of decimal
    let create (value:decimal) = 
        DecimalTwoDigits (Math.Round(value,2))
    let private apply f (DecimalTwoDigits d) = f d
    let value d = apply id d
