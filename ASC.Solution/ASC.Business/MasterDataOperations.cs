using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterKeys.ToList();
        }
        public async Task<List<MasterDataKey>> GetMasterKeyByNameAsync(string name)
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionKeyAsync(name);
            return masterKeys.ToList();
        }
        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
                _unitOfWork.ConmitTransaction();
                return true;
            }
        }
        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            try
            {
                var masterKeys = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionKeyAsync(key);
                return masterKeys.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }
        public async Task<MasterDataValue> GetMasterValueByNameAsync(string key, string name)
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAsync(key, name);
            return masterValues;
        }
        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                _unitOfWork.ConmitTransaction();
                return true;
            }
        }
        public async Task<bool> UpdateMasterKeyAsync(string originalPartitionKey, MasterDataKey key)
        {
            using (_unitOfWork)
            {
                var masterKey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(originalPartitionKey, key.RowKey);
                masterKey.IsActive = key.IsActive;
                masterKey.IsDeleted = key.IsDeleted;
                masterKey.Name = key.Name;
                _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
                _unitOfWork.ConmitTransaction();
                return true;
            }
        }
        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            using ( _unitOfWork)
            {
                var masterValue = await _unitOfWork.Repository<MasterDataValue>().FindAsync(originalPartitionKey, originalRowKey);
                masterValue.IsActive = value.IsActive;
                masterValue.IsDeleted = value.IsDeleted;
                masterValue.Name = value.Name;
                _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                _unitOfWork.ConmitTransaction();
                return true;
            }
        }
        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return masterValues.ToList();
        }
        //public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        //{
        //    using (_unitOfWork)
        //    {
        //        foreach (var value in values)
        //        {
        //            // Find, if null insert MasterKey
        //            var masterKey = await GetMasterKeyByNameAsync(value.PartitionKey);
        //            if (!masterKey.Any())
        //            {
        //                await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey()
        //                {
        //                    Name = value.PartitionKey,
        //                    RowKey = Guid.NewGuid().ToString(),
        //                    PartitionKey = value.PartitionKey
        //                });
        //            }
        //            //Find, if null Insert MasterValue
        //            var masterValuesByKey = await GetAllMasterValuesByKeyAsync(value.PartitionKey);
        //            var masterValue = masterValuesByKey.FirstOrDefault(p => p.Name == value.Name);
        //            if (masterValue == null)
        //            {
        //                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
        //            }
        //            else
        //            {
        //                masterValue.IsActive = value.IsActive;
        //                masterValue.IsDeleted = value.IsDeleted;
        //                masterValue.Name = value.Name;
        //                _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
        //            }
        //        }
        //        _unitOfWork.ConmitTransaction();
        //        return true;
        //    }
        //}
        public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        {
            using (_unitOfWork)
            {
                var existingKeys = new HashSet<string>();
                var valuesByKey = new Dictionary<string, List<MasterDataValue>>();

                foreach (var value in values)
                {
                    // Chỉ truy xuất 1 lần cho mỗi PartitionKey
                    if (!existingKeys.Contains(value.PartitionKey))
                    {
                        var masterKey = await GetMasterKeyByNameAsync(value.PartitionKey);
                        if (!masterKey.Any())
                        {
                            await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey()
                            {
                                Name = value.PartitionKey,
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = value.PartitionKey
                            });
                        }

                        // Cache danh sách MasterValue theo PartitionKey
                        var allValues = await GetAllMasterValuesByKeyAsync(value.PartitionKey);
                        valuesByKey[value.PartitionKey] = allValues;
                        existingKeys.Add(value.PartitionKey);
                    }

                    var existingList = valuesByKey[value.PartitionKey];
                    var matchedValue = existingList.FirstOrDefault(v => v.Name == value.Name);

                    if (matchedValue == null)
                    {
                        // Gán mới rowkey
                        value.RowKey = Guid.NewGuid().ToString();
                        await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                        valuesByKey[value.PartitionKey].Add(value); // Cập nhật cache
                    }
                    else
                    {
                        matchedValue.IsActive = value.IsActive;
                        matchedValue.IsDeleted = value.IsDeleted;
                        matchedValue.Name = value.Name;
                        _unitOfWork.Repository<MasterDataValue>().Update(matchedValue);
                    }
                }

                _unitOfWork.ConmitTransaction();
                return true;
            }
        }
    }
}
