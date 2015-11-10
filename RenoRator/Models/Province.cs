using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(ProvinceModel))]
    public partial class Province
    {
        public class ProvinceModel
        {
        }

        public List<Province> getAllProvinces()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return db.Provinces.ToList();
        }
    }
}