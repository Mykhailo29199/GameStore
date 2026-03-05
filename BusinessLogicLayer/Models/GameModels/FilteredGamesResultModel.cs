namespace BusinessLogicLayer.Models.GameModels;

public class FilteredGamesResultModel
{
    public List<GameModel> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}
