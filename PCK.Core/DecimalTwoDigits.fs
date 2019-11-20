namespace PCK.Core

module DecimalTwoDigits =

    type DecimalTwoDigits = private DecimalTwoDigits of decimal
    let create (value:decimal) = 
        let integralValue = truncate value
        let fraction = value - integralValue
        let truncatedFraction = truncate (fraction * 100M) / 100M
        DecimalTwoDigits (integralValue + truncatedFraction)
    let private apply f (DecimalTwoDigits d) = f d
    let value d = apply id d
