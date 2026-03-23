using GameStore.Shared.Enums;

namespace BusinessLogicLayer.Models.BanModels;

public class BanModel
{
    public string User { get; set; }

    public BanDurationOption Duration { get; set; }
}
