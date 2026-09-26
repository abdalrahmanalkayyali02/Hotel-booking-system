using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Languages : BaseAuditLogModel
{
    public required string LanguageName { get; set; }
    public required string Code { get; set; }

    public ICollection<RoleTranslation> RoleTranslations { get; set; } = new List<RoleTranslation>();
    public ICollection<PermissionsTranslation> PermissionTranslations { get; set; } = new List<PermissionsTranslation>();
    public ICollection<RegionsTranslation> RegionTranslations { get; set; } = new List<RegionsTranslation>();
    public ICollection<SubRegionsTranslation> SubRegionsTranslations { get; set; } = new List<SubRegionsTranslation>();
    public ICollection<StatesTranslation> StateTranslations { get; set; } = new List<StatesTranslation>();
    public ICollection<CountriesTranslation> CountryTranslations { get; set; } = new List<CountriesTranslation>();
    public ICollection<CitiesTranslation> CityTranslations { get; set; } = new List<CitiesTranslation>();
    public ICollection<HotelsTranslation>  HotelTranslations { get; set; } = new List<HotelsTranslation>();
    public ICollection<AmenitiesTranslation> AmenityTranslations { get; set; } = new List<AmenitiesTranslation>();
    public ICollection<ServicesTranslation> ServicesTranslations { get; set; } = new List<ServicesTranslation>();
    public ICollection<RoomTypesTranslation>  RoomTypeTranslations { get; set; } = new List<RoomTypesTranslation>();
}