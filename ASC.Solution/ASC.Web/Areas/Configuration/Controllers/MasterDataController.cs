﻿using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;
        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);
            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);
            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeys)
        {
            masterKeys.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");
            if (!ModelState.IsValid)
            {
                return View(masterKeys);
            }
            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeys.MasterKeyInContext);
            if (masterKeys.IsEdit)
            {
                // Update Master Key
                await _masterData.UpdateMasterKeyAsync(masterKeys.MasterKeyInContext.PartitionKey, masterKey);
            }
            else
            {
                // Insert Master Key
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }
            return RedirectToAction("MasterKeys");
        }
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get All Master Keys and hold them in ViewBag for Select tag
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }
        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // get Master values based on master key.
            return Json(new { data = await _masterData.GetAllMasterValuesByKeyAsync(key) } );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
                //return View();
            }
            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
            if (isEdit)
            {
                // Update Master Value
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Insert Master Value
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                masterDataValue.CreateBy = HttpContext.User.GetCurrentUserDetails().Name;
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }
            return Json(true);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(); // Return the view with the model if it is invalid
        //    }

        //    var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
        //    if (isEdit)
        //    {
        //        // Update Master Value
        //        await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
        //    }
        //    else
        //    {
        //        // Insert Master Value
        //        masterDataValue.RowKey = Guid.NewGuid().ToString();
        //        masterDataValue.CreateBy = HttpContext.User.GetCurrentUserDetails().Name;
        //        await _masterData.InsertMasterValueAsync(masterDataValue);
        //    }

        //    return RedirectToAction("Index"); // Redirect to another action on success
        //}

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();
            using (var memoryStream = new MemoryStream())
            {
                // Get MemoryStream from Excel file
                await excelFile.CopyToAsync(memoryStream);
                // Create a ExcelPackage object from MemoryStream
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    // Get the first Excel sheet from the Workbook
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    // Iterate all the rows and create the List of MasterDataValue
                    // Ignore first row as it is header
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var masterDataValue = new MasterDataValue();
                        masterDataValue.RowKey = Guid.NewGuid().ToString();
                        masterDataValue.PartitionKey = worksheet.Cells[row, 1].Value.ToString();
                        masterDataValue.Name = worksheet.Cells[row, 2].Value.ToString();
                        masterDataValue.IsActive = Boolean.Parse(worksheet.Cells[row, 3].Value.ToString());
                        masterValueList.Add(masterDataValue);
                    }
                }
            }
            return masterValueList;
        }
        //private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        //{
        //    var masterValueList = new List<MasterDataValue>();
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await excelFile.CopyToAsync(memoryStream);
        //        using (ExcelPackage package = new ExcelPackage(memoryStream))
        //        {
        //            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
        //            int rowCount = worksheet.Dimension.Rows;

        //            var existingEntries = new HashSet<string>();

        //            for (int row = 2; row <= rowCount; row++)
        //            {
        //                var partitionKey = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
        //                var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
        //                var isActiveStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

        //                // Validate input
        //                if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(isActiveStr))
        //                    continue;

        //                // Check for duplicates using a composite key
        //                string compositeKey = $"{partitionKey}|{name}";
        //                if (existingEntries.Contains(compositeKey))
        //                    continue;

        //                bool isActive = bool.TryParse(isActiveStr, out var parsedIsActive) && parsedIsActive;

        //                masterValueList.Add(new MasterDataValue
        //                {
        //                    RowKey = Guid.NewGuid().ToString(),
        //                    PartitionKey = partitionKey,
        //                    Name = name,
        //                    IsActive = isActive
        //                });

        //                existingEntries.Add(compositeKey);
        //            }
        //        }
        //    }
        //    return masterValueList;
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;
            //var uploadedFile = HttpContext.Session.GetSession<IFormFile>("UploadedExcelFile");
            //if (uploadedFile != null)
            //{
            //    return Json(new { Error = true, Text = "File đã được tải lên và xử lý trước đó." });
            //}
            // Validations
            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }
            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }
            //HttpContext.Session.SetSession("UploadedExcelFile", excelFile);
            // Parse Excel Data
            var masterData = await ParseMasterDataExcel(excelFile);
            var result = await _masterData.UploadBulkMasterData(masterData);
            return Json(new { Success = result });
        }
    }
}
