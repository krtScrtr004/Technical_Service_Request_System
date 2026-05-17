using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Services
{
    public class EquipmentService
    {
        public string NormalizeAssetTag(string assetTag)
        {
            if (string.IsNullOrWhiteSpace(assetTag))
            {
                return null;
            }

            // Remove non-alphanumeric characters
            return Regex.Replace(assetTag, @"[^a-zA-Z0-9]", "").Trim().ToUpper();
        }

        public Equipment GetEquipmentByAssetTag(string assetTag)
        {
            using (var db = new ApplicationDbContext())
            {
                var normalizedAssetTag = NormalizeAssetTag(assetTag);
                return db.Equipments
                    .FirstOrDefault(e => e.AssetTag == normalizedAssetTag);
            }
        }

        public List<Equipment> GetListEquipmentByAssetTag(string assetTag, bool exactMatch = true)
        {
            using (var db = new ApplicationDbContext())
            {
                var normalizedAssetTag = NormalizeAssetTag(assetTag);
                if (string.IsNullOrEmpty(normalizedAssetTag))
                {
                    return new List<Equipment>();
                }

                var query = db.Equipments.AsQueryable();
                query = exactMatch
                    ? query.Where(e => e.AssetTag == normalizedAssetTag)
                    : query.Where(e => e.AssetTag.Contains(normalizedAssetTag));

                return query
                    .OrderBy(e => e.AssetTag)
                    .Take(10)
                    .ToList();
            }
        }

        public EquipmentFormViewModel ToEquipmentFormViewModel(EquipmentDetailsViewModel equipment)
        {
            if (equipment == null)
            {
                return null;
            }

            return new EquipmentFormViewModel
            {
                Id = equipment.Id,
                Model = equipment.Model,
                AssetTag = equipment.AssetTag,

                TypeId = equipment.TypeId,
                StatusId = equipment.StatusId,

                BuildingNumber = equipment.BuildingNumber,
                FloorNumber = equipment.FloorNumber,
                Office = equipment.Office
            };
        }

    }
}