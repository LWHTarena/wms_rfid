﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI;
using NPOI.HPSF;
using NPOI.HSSF;
using NPOI.HSSF.UserModel;
using System.Data;
using System.IO;

namespace THOK.Common
{
    public  class ExportExcel_test
    {
        /// <summary>
        /// 导出列名
        /// </summary>
        public static System.Collections.SortedList ListColumnsName;
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="filePath"></param>
        public static void ExportExcel(DataTable dtSource, string filePath)
        {
            if (ListColumnsName == null || ListColumnsName.Count == 0)
                throw (new Exception("请对ListColumnsName设置要导出的列明！"));

            HSSFWorkbook excelWorkbook = CreateExcelFile();
            InsertRow(dtSource, excelWorkbook);
            SaveExcelFile(excelWorkbook, filePath);
        }
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="filePath"></param>
        public static void ExportExcel(DataTable dtSource, Stream excelStream)
        {
            if (ListColumnsName == null || ListColumnsName.Count == 0)
                throw (new Exception("请对ListColumnsName设置要导出的列明！"));

            HSSFWorkbook excelWorkbook = CreateExcelFile();
            InsertRow(dtSource, excelWorkbook);
            SaveExcelFile(excelWorkbook, excelStream);
        }
        /// <summary>
        /// 保存Excel文件
        /// </summary>
        /// <param name="excelWorkBook"></param>
        /// <param name="filePath"></param>
        protected static void SaveExcelFile(HSSFWorkbook excelWorkBook, string filePath)
        {
            FileStream file = null;
            try
            {
                file = new FileStream(filePath, FileMode.Create);
                excelWorkBook.Write(file);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
        /// <summary>
        /// 保存Excel文件
        /// </summary>
        /// <param name="excelWorkBook"></param>
        /// <param name="filePath"></param>
        protected static void SaveExcelFile(HSSFWorkbook excelWorkBook, Stream excelStream)
        {
            try
            {
                excelWorkBook.Write(excelStream);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 创建Excel文件
        /// </summary>
        /// <param name="filePath"></param>
        protected static HSSFWorkbook CreateExcelFile()
        {
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            return hssfworkbook;
        }
        /// <summary>
        /// 创建excel表头
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="excelSheet"></param>
        protected static void CreateHeader(HSSFSheet excelSheet)
        {
            int cellIndex = 0;
            //循环导出列
            foreach (System.Collections.DictionaryEntry de in ListColumnsName)
            {
                HSSFRow newRow = (HSSFRow)excelSheet.CreateRow(0);
                HSSFCell newCell = (HSSFCell)newRow.CreateCell(cellIndex);
                newCell.SetCellValue(de.Value.ToString());
                cellIndex++;
            }
        }
        /// <summary>
        /// 插入数据行
        /// </summary>
        protected static void InsertRow(DataTable dtSource, HSSFWorkbook excelWorkbook)
        {
            int rowCount = 0;
            int sheetCount = 1;
            HSSFSheet newsheet = null;

            //循环数据源导出数据集
            newsheet = (HSSFSheet)excelWorkbook.CreateSheet("Sheet" + sheetCount);
            CreateHeader(newsheet);
            foreach (DataRow dr in dtSource.Rows)
            {
                rowCount++;
                //超出10000条数据 创建新的工作簿
                if (rowCount == 10000)
                {
                    rowCount = 1;
                    sheetCount++;
                    newsheet = (HSSFSheet)excelWorkbook.CreateSheet("Sheet" + sheetCount);
                    CreateHeader(newsheet);
                }

                HSSFRow newRow = (HSSFRow)newsheet.CreateRow(rowCount);
                InsertCell(dtSource, dr, newRow, newsheet, excelWorkbook);
            }
        }
        /// <summary>
        /// 导出数据行
        /// </summary>
        /// <param name="dtSource"></param>
        /// <param name="drSource"></param>
        /// <param name="currentExcelRow"></param>
        /// <param name="excelSheet"></param>
        /// <param name="excelWorkBook"></param>
        protected static void InsertCell(DataTable dtSource, DataRow drSource, HSSFRow currentExcelRow, HSSFSheet excelSheet, HSSFWorkbook excelWorkBook)
        {
            for (int cellIndex = 0; cellIndex < ListColumnsName.Count; cellIndex++)
            {
                //列名称
                string columnsName = ListColumnsName.GetKey(cellIndex).ToString();
                HSSFCell newCell = null;
                System.Type rowType = drSource[columnsName].GetType();
                string drValue = drSource[columnsName].ToString().Trim();
                switch (rowType.ToString())
                {
                    case "System.String"://字符串类型
                        drValue = drValue.Replace("&", "&");
                        drValue = drValue.Replace(">", ">");
                        drValue = drValue.Replace("<", "<");
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue(drValue);
                        break;
                    case "System.DateTime"://日期类型
                        DateTime dateV;
                        DateTime.TryParse(drValue, out dateV);
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue(dateV);

                        //格式化显示
                        HSSFCellStyle cellStyle = (HSSFCellStyle)excelWorkBook.CreateCellStyle();
                        HSSFDataFormat format = (HSSFDataFormat)excelWorkBook.CreateDataFormat();
                        cellStyle.DataFormat = format.GetFormat("yyyy-mm-dd hh:mm:ss");
                        newCell.CellStyle = cellStyle;

                        break;
                    case "System.Boolean"://布尔型
                        bool boolV = false;
                        bool.TryParse(drValue, out boolV);
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue(boolV);
                        break;
                    case "System.Int16"://整型
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Byte":
                        int intV = 0;
                        int.TryParse(drValue, out intV);
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue(intV.ToString());
                        break;
                    case "System.Decimal"://浮点型
                    case "System.Double":
                        double doubV = 0;
                        double.TryParse(drValue, out doubV);
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue(doubV);
                        break;
                    case "System.DBNull"://空值处理
                        newCell = (HSSFCell)currentExcelRow.CreateCell(cellIndex);
                        newCell.SetCellValue("");
                        break;
                    default:
                        throw (new Exception(rowType.ToString() + "：类型数据无法处理!"));
                }
            }
        }
    }
    //排序实现接口 不进行排序 根据添加顺序导出
    public class NoSort : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            return -1;
        }
    }
}
