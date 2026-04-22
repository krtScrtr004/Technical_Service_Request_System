using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services
{
    public class TechnicalServiceRequestQueueService
    {
        private readonly ApplicationDbContext _db;

        public TechnicalServiceRequestQueueService()
        {
            _db = new ApplicationDbContext();
        }

        public void Push(int technicalServiceRequestId)
        {
            _db.TechnicalServiceRequestQueues.Add(new TechnicalServiceRequestQueue()
            {
                TechnicalServiceRequestId = technicalServiceRequestId,
                QueuedAt = DateTime.Now
            });
            _db.SaveChanges();
        }

        public TechnicalServiceRequest Pop()
        {
            var lastItem = _db.TechnicalServiceRequestQueues
                .Where(q => 
                    !q.IsProcessed &&
                    q.TechnicalServiceRequest.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.PENDING)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            // Mark the item as processed
            if (lastItem != null)
            {
                lastItem.IsProcessed = true;
                lastItem.TechnicalServiceRequest.TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.ONGOING;

                _db.Entry(lastItem.TechnicalServiceRequest).State = EntityState.Modified;
                _db.Entry(lastItem).State = EntityState.Modified;
                _db.SaveChanges();

                return _db.TechnicalServiceRequests
                    .Where(i => i.Id == lastItem.TechnicalServiceRequestId)
                    .First();
            }

            return null;
        }

        public TechnicalServiceRequest Top()
        {
            var lastItem = _db.TechnicalServiceRequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            return (lastItem != null)
                 ? _db.TechnicalServiceRequests.Where(i => i.Id == lastItem.TechnicalServiceRequestId).First()
                 : null;
        }

        public TechnicalServiceRequest Peek()
        {
            var lastItem = _db.TechnicalServiceRequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            return (lastItem != null)
                ? _db.TechnicalServiceRequests.Where(i => i.Id == lastItem.TechnicalServiceRequestId).First()
                : null;
        }

        public TechnicalServiceRequest[] GetAll()
        {
            var items = _db.TechnicalServiceRequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .ToList();

            return items.Select(i => _db.TechnicalServiceRequests.Where(r => r.Id == i.TechnicalServiceRequestId).First()).ToArray();
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
    }
}