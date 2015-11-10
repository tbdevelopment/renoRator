using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(CityMetaData))]
    public partial class City
    {
        public class CityMetaData
        {
            [Required(ErrorMessage = "Province required!")]
            [Display(Name = "Province")]
            public object provinceID;
        }

        public List<City> getAllCities(){
            renoRatorDBEntities db = new renoRatorDBEntities();
            return db.Cities.ToList();
        }
    }
}