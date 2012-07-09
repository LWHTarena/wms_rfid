﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THOK.RfidWms.DBModel.Ef.Models.Wms
{
    public class Supplier
    {
        public Supplier()
        { 

        }
        public string SupplierCode { get; set; }
        public string UniformCode { get; set; }
        public string CustomCode { get; set; }
        public string SupplierName { get; set; }
        public string ProvinceName { get; set; }
        public string IsActive { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}