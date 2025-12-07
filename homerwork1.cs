using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Book
{
public int Id { get; set; }
public string Title { get; set; }
public string Author { get; set; }
public int YearPublished { get; set; }
}

public class LibraryContext : DbContext
{
public DbSet<Book> Books { get; set; }

```
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(
        @"Server=YOUR_SERVER;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;");
}
```

}

class Program
{
static void Main()
{
while (true)
{
Console.WriteLine("\n====== Библиотека (EF Core) ======");
Console.WriteLine("1. Добавить книгу");
Console.WriteLine("2. Показать все книги");
Console.WriteLine("3. Посчитать количество книг");
Console.WriteLine("4. Найти книги по автору");
Console.WriteLine("5. Удалить книгу");
Console.WriteLine("0. Выход");
Console.Write("Выберите действие: ");

```
        string choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1": AddBook(); break;
            case "2": ShowAllBooks(); break;
            case "3": CountBooks(); break;
            case "4": FindByAuthor(); break;
            case "5": DeleteBook(); break;
            case "0": return;
            default:
                Console.WriteLine("Неверный пункт меню.");
                break;
        }
    }
}

static void AddBook()
{
    Console.Write("Название: ");
    string title = Console.ReadLine();

    Console.Write("Автор: ");
    string author = Console.ReadLine();

    Console.Write("Год публикации: ");
    int year = int.Parse(Console.ReadLine());

    using (var db = new LibraryContext())
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            YearPublished = year
        };

        db.Books.Add(book);
        db.SaveChanges();
    }

    Console.WriteLine("Книга успешно добавлена.");
}

static void ShowAllBooks()
{
    using (var db = new LibraryContext())
    {
        var list = db.Books.ToList();

        if (!list.Any())
        {
            Console.WriteLine("Список пуст.");
            return;
        }

        foreach (var b in list)
        {
            Console.WriteLine($"{b.Id}. {b.Title} — {b.Author}, {b.YearPublished}");
        }
    }
}

static void CountBooks()
{
    using (var db = new LibraryContext())
    {
        int count = db.Books.Count();
        Console.WriteLine($"Количество книг: {count}");
    }
}

static void FindByAuthor()
{
    Console.Write("Введите автора: ");
    string author = Console.ReadLine();

    using (var db = new LibraryContext())
    {
        var books = db.Books
            .Where(b => b.Author.Contains(author))
            .ToList();

        if (!books.Any())
        {
            Console.WriteLine("Книги не найдены.");
            return;
        }

        foreach (var b in books)
        {
            Console.WriteLine($"{b.Id}. {b.Title} — {b.Author}, {b.YearPublished}");
        }
    }
}

static void DeleteBook()
{
    Console.Write("Введите Id для удаления: ");
    int id = int.Parse(Console.ReadLine());

    using (var db = new LibraryContext())
    {
        var book = db.Books.FirstOrDefault(b => b.Id == id);

        if (book == null)
        {
            Console.WriteLine("Книга не найдена.");
            return;
        }

        db.Books.Remove(book);
        db.SaveChanges();
    }

    Console.WriteLine("Книга удалена.");
}
```

}
