using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(AddressMetaData))]
    public partial class Address
    {
        public class AddressMetaData
        {
            [Required(ErrorMessage="Postal Code required!")]
            [PostalCodeValidation()]
            [Display(Name="Postal Code")]
            public object postalCode;

            [Required(ErrorMessage="Address Line 1 required!")]
            [Display(Name="Address Line 1")]
            public object addressLine1;

            [Display(Name="Address Line 2")]
            public object addressLine2;

            [Required(ErrorMessage="City required!")]
            [Display(Name="City")]
            public object cityID;
        }
    }
}