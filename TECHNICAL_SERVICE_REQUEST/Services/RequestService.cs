using System;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Services
{
    public class RequestService
    {
        public Request ToRequest(RequestCreateViewModel technicalServiceRequest)
        {
            var equipment = new Equipment
            {
                Model = technicalServiceRequest.EquipmentModel,
                AssetTag = technicalServiceRequest.EquipmentAssetTag,
                TypeId = technicalServiceRequest.EquipmentTypeId
            };

            var scheduledControlProcessDetail = new ScheduledControlProcessDetail
            {
                // For the scheduled date, combine the date and time properties from the view model into a single DateTime property in the model
                ScheduledDate = technicalServiceRequest.ScheduledDate.HasValue && technicalServiceRequest.ScheduledStartTime.HasValue
                    ? (DateTime?)technicalServiceRequest.ScheduledDate.Value.Date + technicalServiceRequest.ScheduledStartTime.Value
                    : null,
                ScheduledStartTime = technicalServiceRequest.ScheduledStartTime.HasValue
                    ? technicalServiceRequest.ScheduledStartTime.Value
                    : (TimeSpan?)null,
                ScheduledEndTime = technicalServiceRequest.ScheduledEndTime.HasValue
                    ? technicalServiceRequest.ScheduledEndTime.Value
                    : (TimeSpan?)null,
            };

            return new Request
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,

                ClientId = technicalServiceRequest.ClientId,

                TypeId = technicalServiceRequest.TypeId,
                SeverityId = technicalServiceRequest.SeverityId,
                Others = technicalServiceRequest.Others,
                Description = technicalServiceRequest.Description,

                EquipmentId = equipment != null ? equipment.Id : (int?)null,
                Equipment = equipment ?? null,

                ScheduledControlProcessDetailId = scheduledControlProcessDetail != null ? scheduledControlProcessDetail.Id : (int?)null,
                ScheduledControlProcessDetail = scheduledControlProcessDetail ?? null
            };
        }

        public RequestDetailsViewModel ToRequestDetailsViewModel(Request technicalServiceRequest)
        {
            var _db = new ApplicationDbContext();

            return new RequestDetailsViewModel
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,

                ClientId = technicalServiceRequest.ClientId,
                ClientFirstName = technicalServiceRequest.Client.FirstName,
                ClientMiddleName = technicalServiceRequest.Client.MiddleName,
                ClientLastName = technicalServiceRequest.Client.LastName,
                ClientExtensionName = technicalServiceRequest.Client.ExtensionName,
                ClientEmailAddress = technicalServiceRequest.Client.Email,
                ClientContactNumber = technicalServiceRequest.Client.ContactNumber,
                ClientOffice = technicalServiceRequest.Client.Office,
                ClientPosition = technicalServiceRequest.Client.Position,

                TypeId = technicalServiceRequest.TypeId.GetValueOrDefault(),
                Type = technicalServiceRequest.Type,
                SeverityId = technicalServiceRequest.SeverityId.GetValueOrDefault(),
                Severity = technicalServiceRequest.Severity,
                Severities = new SelectList(
                    RequestSeverityEnum.GetSelectListItems(),
                    "Value", "Text"
                ),
                StatusId = technicalServiceRequest.StatusId.GetValueOrDefault(),
                Status = technicalServiceRequest.Status,
                Statuses = new SelectList(
                    RequestStatusEnum.GetSelectListItems(),
                    "Value", "Text"
                ),
                Others = technicalServiceRequest.Others,
                Description = technicalServiceRequest.Description,

                DateRequest = technicalServiceRequest.DateRequest,
                DateRecieved = technicalServiceRequest.DateReceived,

                EquipmentModel = technicalServiceRequest.Equipment?.Model,
                EquipmentAssetTag = technicalServiceRequest.Equipment?.AssetTag,
                EquipmentTypeName = technicalServiceRequest.Equipment?.Type.Name,

                ScheduledDate = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledDate,
                ScheduledStartTime = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledStartTime,
                ScheduledEndTime = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledEndTime,

                RequestHistories = technicalServiceRequest.Histories,
            };
        }
    }

}