namespace BookStoreApp.Models;

public class AddBookDTO
{
    public string Title { get; set; }
    public List<AutorDto> authors { get; set; }
}