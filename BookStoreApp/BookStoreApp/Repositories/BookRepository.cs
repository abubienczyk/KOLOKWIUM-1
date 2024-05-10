using BookStoreApp.Models;
using Microsoft.Data.SqlClient;

namespace BookStoreApp.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async  Task<bool> DoesBookExist(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM BOOKS WHERE PK=@ID;";
        command.Parameters.AddWithValue("@ID", id);
        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<GetAuthorsDTO> GetAuthors(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "select b.PK as bookPk, b.title, a.first_name, a.last_name from books b join books_authors ba on b.PK=ba.FK_book join authors a on ba.FK_author=a.PK where b.PK=@PK";
        command.Parameters.AddWithValue("@PK", id);
        
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        var bookIdOrdinal = reader.GetOrdinal("bookPk");
        var bookTitleOrdinal = reader.GetOrdinal("title");
        var firstOrdinal = reader.GetOrdinal("first_name");
        var lastOrdinal = reader.GetOrdinal("last_name");

        GetAuthorsDTO dto = null;
        
        while (await reader.ReadAsync())
        {
            if (dto is not null)
            {
                dto.authors.Add(new AutorDto()
                {
                    FirstName = reader.GetString(firstOrdinal),
                    LastName    = reader.GetString(lastOrdinal)
                });
            }
            else
            {
                dto = new GetAuthorsDTO()
                {
                    Id = reader.GetInt32(bookIdOrdinal),
                    Title = reader.GetString(bookTitleOrdinal),
                    authors = new List<AutorDto>()
                    {
                        new AutorDto()
                        {
                            FirstName = reader.GetString(firstOrdinal),
                            LastName = reader.GetString(lastOrdinal)
                        }
                    }
                };
            }
        }

        if (dto is null) throw new Exception();
        return dto;
    }

    public async Task<bool> DoesAuthorExist(string fName, string lName)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM AUTHORS WHERE FIRST_NAME=@FIRST AND LAST_NAME=@LAST;";
        command.Parameters.AddWithValue("@FIRST", fName);
        command.Parameters.AddWithValue("@LAST", lName);
        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<GetAuthorsDTO> PostBook(AddBookDTO data)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO BOOKS VALUES(@TITLE); SELECT @@IDENTITY AS ID";
        command.Parameters.AddWithValue("@TITLE", data.Title);
        await connection.OpenAsync();
        
        int ID = 0;
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            var tmp=await command.ExecuteScalarAsync();
            ID = Convert.ToInt32(tmp);
            foreach (var author in data.authors)
            {
                
                //pobranie id
                command.Parameters.Clear();
                command.CommandText = "SELECT PK FROM AUTHORS WHERE FIRST_NAME=@FIRST AND LAST_NAME=@LAST;";
                command.Parameters.AddWithValue("@FIRST", author.FirstName);
                command.Parameters.AddWithValue("@LAST", author.LastName);

                var authorID = await command.ExecuteScalarAsync();
                
                // wstawienie do asocjacyjnej 
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO BOOKS_AUTHORS VALUES(@IDBOOK, @IDAUTHOR)";
                command.Parameters.AddWithValue("@IDBOOK", ID);
                command.Parameters.AddWithValue("@IDAUTHOR", authorID);
                
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
        GetAuthorsDTO dto=new GetAuthorsDTO()
       {
           Id = ID,
           Title = data.Title,
           authors = data.authors
       };

       if (dto is null) throw new Exception();
       return dto;
    }
}