﻿using ASC.DataAccess.Interfaces;
using ASC.Model.BaseTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private DbContext dbContext;
        public Repository(DbContext _dbContext)
        {
            this.dbContext = _dbContext;
        }
        public async Task<T> AddAsync(T entity)
        {
            var entityToInsert = entity as BaseEntity;
            entityToInsert.CreateDate = DateTime.UtcNow;
            entityToInsert.UpdateDate = DateTime.UtcNow;
            var result = dbContext.Set<T>().AddAsync(entity).Result;
            return result as T;
        }
        public void Update(T entity)
        {
            var entityToUpdate = entity as BaseEntity;
            entityToUpdate.CreateDate = DateTime.UtcNow;
            entityToUpdate.UpdateDate = DateTime.UtcNow;
            var result = dbContext.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            var entityToDelete = entity as BaseEntity;
            entityToDelete.UpdateDate = DateTime.UtcNow;
            entityToDelete.IsDeleted = true;
            dbContext.Set<T>().Remove(entity);
        }
        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            var result = dbContext.Set<T>().FindAsync(partitionKey, rowKey).Result;
            return result as T;
        }
        public async Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey)
        {
            var result = dbContext.Set<T>().Where(t => t.PartitionKey == partitionKey).ToListAsync().Result;
            return result as IEnumerable<T>;
        }
        public async Task<IEnumerable<T>> FindAllAsync()
        {
            var result = dbContext.Set<T>().ToListAsync().Result;
            return result as IEnumerable<T>;
        }
    }
}
