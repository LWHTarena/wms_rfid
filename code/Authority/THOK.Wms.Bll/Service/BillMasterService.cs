﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using THOK.Wms.DbModel;
using THOK.Wms.Dal.Interfaces;
using THOK.Wms.Bll.Interfaces;
using System.Transactions;

namespace THOK.Wms.Bll.Service
{
    public class BillMasterService : ServiceBase<BillMaster>, IBillMasterService
    {
        [Dependency]
        public IBillMasterRepository BillMasterRepository { get; set; }
        [Dependency]
        public IBillDetailRepository BillDetailRepository { get; set; }
        [Dependency]
        public IContractRepository ContractRepository { get; set; }
        [Dependency]
        public IContractDetailRepository ContractDetailRepository { get; set; }
        [Dependency]
        public INavicertRepository NavicertRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        public bool Add(BillMaster billMaster, out string strResult)
        {
            strResult = string.Empty;
            bool result = false;
            var b = new BillMaster();
            
            if (b != null)
            {
                try
                {
                    b.ID = billMaster.ID;
                    b.UUID = billMaster.UUID;
                    b.BillType = billMaster.BillType;
                    b.BillDate = billMaster.BillDate;
                    b.MakerName = billMaster.MakerName;
                    if (billMaster.OperateDate > Convert.ToDateTime("0002-1-1"))
                    {
                        b.OperateDate = billMaster.OperateDate;
                    }
                    b.CigaretteType = billMaster.CigaretteType;
                    b.BillCompanyCode = billMaster.BillCompanyCode;
                    b.SupplierCode = billMaster.SupplierCode;
                    b.SupplierType = billMaster.SupplierType;
                    b.State = billMaster.State;

                    BillMasterRepository.Add(b);
                    BillMasterRepository.SaveChanges();
                    result = true;
                }
                catch (Exception ex)
                {
                    strResult = "原因：" + ex.ToString();
                    result = false;
                }
            }
            else
            {
                strResult = "原因：该编号已存在！";
                result = false;
            }
            return result;
        }
        public bool Save(BillMaster billMaster, out string strResult)
        {
            strResult = string.Empty;
            bool result = false;
            var b = BillMasterRepository.GetQueryable().FirstOrDefault(c => c.UUID == billMaster.UUID);
            if (b != null)
            {
                try
                {
                    b.UUID = billMaster.UUID;
                    b.BillType = billMaster.BillType;
                    b.BillDate = billMaster.BillDate;
                    b.MakerName = billMaster.MakerName;
                    if (b.OperateDate > Convert.ToDateTime("0002-1-1"))
                    {
                        b.OperateDate = billMaster.OperateDate;
                    }
                    b.CigaretteType = billMaster.CigaretteType;
                    b.BillCompanyCode = billMaster.BillCompanyCode;
                    b.SupplierCode = billMaster.SupplierCode;
                    b.SupplierType = billMaster.SupplierType;
                    b.State = billMaster.State;

                    BillMasterRepository.SaveChanges();
                    result = true;
                }
                catch (Exception ex)
                {
                    strResult = "原因：" + ex.Message;
                }
            }
            return result;
        }
        public bool Delete(string contractCode, string uuid, out string strResult)
        {
            bool result = false;
            strResult = string.Empty;

            var navicert = NavicertRepository.GetQueryable().Where(i => i.ContractCode == contractCode);
            var contract = ContractRepository.GetQueryable().Where(i => i.ContractCode == contractCode);
            var billMaster = BillMasterRepository.GetQueryable().Where(i => i.UUID == uuid);

            try
            {
                if (navicert != null)
                {
                    foreach (var item1 in navicert.ToList())
                    {
                        NavicertRepository.Delete(item1);
                        result = true;
                    }
                }
                if (contract != null)
                {
                    foreach (var item2 in contract.ToList())
                    {
                        Del(ContractDetailRepository, item2.ContractDetails);
                        ContractRepository.Delete(item2);
                        result = true;
                    }
                }
                if (billMaster != null)
                {
                    foreach (var item3 in billMaster.ToList())
                    {
                        Del(BillDetailRepository, item3.BillDetails);
                        BillMasterRepository.Delete(item3);
                        result = true;
                    }
                }
                BillMasterRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                strResult = "原因：" + ex.Message;
                result = false;
            }
            return result;
        }
    }
}
