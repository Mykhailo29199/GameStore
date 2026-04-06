namespace BusinessLogicLayer.Models.GameModels;

public class FilteredGamesResponseModel
{
    public List<GameModel> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}
