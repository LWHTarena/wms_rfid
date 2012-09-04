﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using THOK.Wms.Bll.Interfaces;
using THOK.WebUtil;


namespace Wms.Controllers.Wms.Inventory
{
    public class CellHistoricalController : Controller
    {
        //
        // GET: /CellHistorical/

        [Dependency]
        public ICellHistoricalService CellHistoricalService { get; set; }

        public ActionResult Index(string moduleID)
        {
            ViewBag.hasSearch = true;
            ViewBag.hasPrint = true;
            ViewBag.hasHelp = true;
            ViewBag.ModuleID = moduleID;
            return View();
        }

        //
        // GET: /CellHistorical/Details/

        public ActionResult Details(int page, int rows, string beginDate, string endDate, string type, string id)
        {
            var storage = CellHistoricalService.GetCellDetails(page, rows, beginDate, endDate, type, id);

            return Json(storage, "text", JsonRequestBehavior.AllowGet);
        }

    }
}
