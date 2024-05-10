using BookStoreApp.Models;
using BookStoreApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetAuthors(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
            return NotFound("BOOK NOT FOUND");

        var result = await _bookRepository.GetAuthors(id);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostBook(AddBookDTO data)
    {
        foreach (var author in data.authors)
        {
            if (!await _bookRepository.DoesAuthorExist(author.FirstName, author.LastName))
                return NotFound("AUTHOR NOT FOUND");
        }

        var result = await _bookRepository.PostBook(data);

        return Created("", result);
    }
}