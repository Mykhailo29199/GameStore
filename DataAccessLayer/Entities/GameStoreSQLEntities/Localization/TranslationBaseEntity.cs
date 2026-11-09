using System.ComponentModel.DataAnnotations;
using GameStore.Shared.Localization;

namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

public abstract class TranslationBaseEntity
{
    [Required]
    [MaxLength(Languages.CodeMaxLength)]
    public string LanguageCode { get; set; }
}
