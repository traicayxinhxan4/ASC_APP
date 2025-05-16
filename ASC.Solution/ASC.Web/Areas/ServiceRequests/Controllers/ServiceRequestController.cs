﻿using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(IServiceRequestOperations serviceRequestOperations, IMapper mapper, IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _mapper = mapper;
            _masterData = masterData;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());

        }
        //[HttpGet]
        //public async Task<IActionResult> ServiceRequest()
        //{
        //    var masterData = await _masterData.GetMasterDataCacheAsync();
        //    var vehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
        //    var vehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();

        //    ViewBag.VehicleTypes = vehicleTypes;
        //    ViewBag.VehicleNames = vehicleNames;

        //    // 📄 Ghi log ra file
        //    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        //    Directory.CreateDirectory(logPath); // tạo thư mục nếu chưa có
        //    var logFile = Path.Combine(logPath, "service-request-log.txt");

        //    var logBuilder = new StringBuilder();
        //    logBuilder.AppendLine(">>> VehicleNames Count: " + vehicleNames.Count);
        //    foreach (var v in vehicleNames)
        //    {
        //        logBuilder.AppendLine($"[VehicleName] RowKey: {v.RowKey}, Name: {v.Name}, PartitionKey: {v.PartitionKey}");
        //    }

        //    logBuilder.AppendLine(">>> VehicleTypes Count: " + vehicleTypes.Count);
        //    foreach (var v in vehicleTypes)
        //    {
        //        logBuilder.AppendLine($"[VehicleType] RowKey: {v.RowKey}, Name: {v.Name}, PartitionKey: {v.PartitionKey}");
        //    }

        //    await System.IO.File.WriteAllTextAsync(logFile, logBuilder.ToString());

        //    return View(new NewServiceRequestViewModel());
        //}
        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(request);
            }
            // Map the view model to Azure model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);
            // Set RowKey, PartitionKey, RequestedDate, Status properties
            serviceRequest.PartitionKey = HttpContext.User.GetCurrentUserDetails().Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();
            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);
            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }
    }
}
