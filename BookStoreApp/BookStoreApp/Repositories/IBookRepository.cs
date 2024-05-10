using BookStoreApp.Models;

namespace BookStoreApp.Repositories;

public interface IBookRepository
{
    public Task<bool> DoesBookExist(int id);
    public Task<GetAuthorsDTO> GetAuthors(int id);

    public Task<bool> DoesAuthorExist(string fName, string lName);

    public Task<GetAuthorsDTO> PostBook(AddBookDTO data);
}