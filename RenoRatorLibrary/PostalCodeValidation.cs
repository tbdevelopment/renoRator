using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace RenoRatorLibrary
{

    public class PostalCodeValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return null;
            ValidationResult r = new ValidationResult("Invalid Postal Code!");
            if(!ValidateFunctions.validPostalCode(((String)value).ToUpper()))
                return r;
            return null;
        }
    }
}
