using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Enumerables;

namespace TECHNICAL_SERVICE_REQUEST.Services
{
    public class RequestQueueService
    {
        private readonly ApplicationDbContext _db;

        public RequestQueueService()
        {
            _db = new ApplicationDbContext();
        }

        public void Push(int technicalServiceRequestId)
        {
            _db.RequestQueues.Add(new RequestQueue()
            {
                RequestId = technicalServiceRequestId,
                QueuedAt = DateTime.Now
            });
            _db.SaveChanges();
        }

        public Request Pop()
        {
            var lastItem = _db.RequestQueues
                .Where(q => 
                    !q.IsProcessed &&
                    q.Request.StatusId == (int)RequestStatusEnum.PENDING)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            // Mark the item as processed
            if (lastItem != null)
            {
                lastItem.IsProcessed = true;
                lastItem.Request.StatusId = (int)RequestStatusEnum.ONGOING;

                _db.Entry(lastItem.Request).State = EntityState.Modified;
                _db.Entry(lastItem).State = EntityState.Modified;
                _db.SaveChanges();

                return _db.Requests
                    .Where(i => i.Id == lastItem.RequestId)
                    .First();
            }

            return null;
        }

        public Request Top()
        {
            var lastItem = _db.RequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            return (lastItem != null)
                 ? _db.Requests.Where(i => i.Id == lastItem.RequestId).First()
                 : null;
        }

        public Request Peek()
        {
            var lastItem = _db.RequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .FirstOrDefault();

            return (lastItem != null)
                ? _db.Requests.Where(i => i.Id == lastItem.RequestId).First()
                : null;
        }

        public Request[] GetAll()
        {
            var items = _db.RequestQueues
                .Where(q => !q.IsProcessed)
                .OrderBy(q => q.QueuedAt)
                .ToList();

            return items.Select(i => _db.Requests.Where(r => r.Id == i.RequestId).First()).ToArray();
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