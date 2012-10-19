﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using THOK.Wms.Allot.Interfaces;
using Microsoft.Practices.Unity;
using THOK.WebUtil;
using THOK.Wms.Bll.Interfaces;

namespace Wms.Controllers.Wms.VehicleMounted
{
    public class StockInTaskController : Controller
    {
        //
        // GET: /StockInTask/

        [Dependency]
        public IInBillAllotService InBillAllotService { get; set; }
        [Dependency]
        public IInBillMasterService InBillMasterService { get; set; }

        public ActionResult Index(string moduleID)
        {
            ViewBag.hasSearch = true;
            ViewBag.hasApply = true;
            ViewBag.hasCancel = true;
            ViewBag.hasFinish = true;
            ViewBag.hasBatch = true;
            ViewBag.ModuleID = moduleID;
            return View();
        }
        //GO: /StockInTask/Search/
        public ActionResult Search(string billNo, int page, int rows)
        {
            var result = InBillAllotService.SearchInBillAllot(billNo, page, rows);
            return Json(result, "text", JsonRequestBehavior.AllowGet);
        }
        //GO: /StockInTask/GetBillNo/
        public ActionResult GetBillNo()
        {
            var result = InBillMasterService.GetInBillMaster();
            return Json(result, "text", JsonRequestBehavior.AllowGet);
        }
        //GO: /StockInTask/Operate/
        public ActionResult Operate(int id, string status)
        {
            string strResult = string.Empty;
            string operator1 = string.Empty;
            string msg = string.Empty;
            if (status != "0") operator1 = "admin";
            if (status == "0") operator1 = "";
            bool bResult = InBillAllotService.EditAllot(id, status, operator1, out strResult);
            if (status == "0") msg = bResult ? "取消成功" : "取消失败";
            if (status == "1") msg = bResult ? "申请成功" : "申请失败";
            if (status == "2") msg = bResult ? "操作成功" : "操作失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, strResult), "text", JsonRequestBehavior.AllowGet);
        }
    }
}
