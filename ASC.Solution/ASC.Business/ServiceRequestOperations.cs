﻿using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
                _unitOfWork.ConmitTransaction();
            }
        }
        public ServiceRequest UpdateServiceRequest(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                _unitOfWork.Repository<ServiceRequest>().Update(request);
                _unitOfWork.ConmitTransaction();
                return request;
            }
        }
        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            using (_unitOfWork)
            {
                var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);
                if (serviceRequest == null)
                    throw new NullReferenceException();
                serviceRequest.Status = status;
                _unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
                _unitOfWork.ConmitTransaction();
                return serviceRequest;
            }
        }
    }
}
