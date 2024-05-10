namespace BookStoreApp.Models;

public class GetAuthorsDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<AutorDto> authors { get; set; }
}
public class AutorDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}