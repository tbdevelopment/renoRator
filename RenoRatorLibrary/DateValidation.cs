using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace RenoRatorLibrary
{
    public class DateValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult r = new ValidationResult("Invalid Date!");
            try
            {
                DateTime d = (DateTime)value;
                DateTime dateTime = DateTime.ParseExact(d.Date.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception ex)
            {
                return r;
            }
            return null;
        }
    }
}
