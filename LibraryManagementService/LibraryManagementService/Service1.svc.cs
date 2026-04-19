using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace LibraryManagementService
{
    public class Service1 : IService1
    {
        private static List<Book> books = new List<Book>();

        public void AddBook(Book book)
        {
            var fault = ValidateBook(book);
            if (fault.HasErrors)
                throw new FaultException<ValidationFault>(fault, "Validation failed.");

            books.Add(book);
        }

        public void RemoveBook(Book book)
        {
            Book bookToRemove = books.FirstOrDefault(b => b.FormattedISBN == book.FormattedISBN);

            if (bookToRemove == null)
                throw new FaultException("Book selected for deletion does not exist.");

            books.Remove(bookToRemove);
        }

        public void UpdateBook(Book book, Book newBook)
        {
            Book bookToUpdate = books.FirstOrDefault(b => b.FormattedISBN == book.FormattedISBN);

            var fault = ValidateBook(newBook, bookToUpdate.FormattedISBN);
            if (fault.HasErrors)
                throw new FaultException<ValidationFault>(fault, "Validation failed.");

            if (bookToUpdate == null)
                throw new FaultException("Book selected for update does not exist.");
            
            books.Remove(bookToUpdate);
            books.Add(newBook);
        }

        public List<Book> GetAllBooks()
        {
            if (books.Count == 0)
                throw new FaultException("No books currently in the database.");
            return books;
        }

        public Book LookupBookByISBN(string isbn)
        {
            string formattedISBN = isbn.Trim().Replace("-", "").Replace(" ", "");

            if (string.IsNullOrWhiteSpace(formattedISBN))
                throw new FaultException("Unable to search without a provided ISBN.");

            if (formattedISBN.Length != 13)
                throw new FaultException("Provided ISBN is the incorrect length. (Only ISBN-13 format accepted.)");

            if (!long.TryParse(formattedISBN, out _))
                throw new FaultException("Only digits, spaces, and dashes accepted in ISBN entry.");

            Book foundBook = books.FirstOrDefault(b => b.FormattedISBN == formattedISBN);

            if (foundBook == null)
                throw new FaultException($"Book not found with ISBN: {isbn}");

            return foundBook;
        }

        public List<Book> LookupBooksByTitle(string title)
        {
            title = title.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(title))
                throw new FaultException("Unable to search without a provided title.");

            List<Book> foundBooks = books.Where(b => b.FormattedTitle == title).ToList();

            if (foundBooks.Count == 0)
                throw new FaultException($"No books found with Title: {title}");

            return foundBooks;
        }

        public List<Book> LookupBooksByAuthor(string author)
        {
            List<Book> foundBooks = books.Where(b => b.FormattedAuthor == author).ToList();

            if (string.IsNullOrWhiteSpace(author))
                throw new FaultException("Unable to search without a provided author.");

            if (foundBooks.Count == 0)
                throw new FaultException($"No books found with Author: {author}");

            return foundBooks;
        }

        public bool CheckBookAvailability(Book book)
        {
            Book foundBook = books.FirstOrDefault(b => b.FormattedISBN == book.FormattedISBN);

            if (foundBook == null)
                throw new FaultException("Book not found.");

            return foundBook.IsAvailable;
        }

        public void UpdateBookAvailability(Book book, bool isAvailable)
        {
            Book bookToUpdate = books.FirstOrDefault(b => b.FormattedISBN == book.FormattedISBN);

            if (bookToUpdate == null)
                throw new FaultException("Book not found.");

            bookToUpdate.IsAvailable = isAvailable;
        }

        public List<Book> GetBooksByAvailability(bool isAvailable)
        {
            List<Book> foundBooks = books.Where(b => b.IsAvailable == isAvailable).ToList();
            string isAvailableString = isAvailable ? "available" : "unavailable";

            if (foundBooks.Count == 0)
                throw new FaultException($"No {isAvailableString} books found.");

            return foundBooks;
        }
        

        // -- helper methods --
        private ValidationFault ValidateBook(Book book, string excludeISBN = null)
        {
            ValidationFault fault = new ValidationFault();

            if (excludeISBN != null)
                excludeISBN = excludeISBN.Trim().ToLower();

            // null/empty checks
            if (book == null)
            {
                fault.Errors.Add("Book cannot be null.");
                return fault;   // do not continue if the book is null
            }

            fault.Errors.AddRange(ValidateISBN(book));     // also checks ISBN length/digits at the same time

            if (string.IsNullOrWhiteSpace(book.Title))
                fault.Errors.Add("Book must have a title.");

            if (string.IsNullOrWhiteSpace(book.Author))
                fault.Errors.Add("Book must have an author.");

            // check for duplicate books
            if (books.Any(b => b.FormattedISBN == book.FormattedISBN &&  b.FormattedISBN != excludeISBN))
                fault.Errors.Add("Book with this ISBN already exists in the database.");

            return fault;
        }
        
        private List<string> ValidateISBN(Book book)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(book.ISBN))
                errors.Add("Book must have an ISBN.");
            else
            {
                if (book.FormattedISBN.Length != 13)
                    errors.Add("ISBN must have 13 characters (ISBN-10 format not accepted.)");

                if (!long.TryParse(book.FormattedISBN, out _))
                {
                    errors.Add("ISBN can only contain digits, spaces, or dashes.");
                }
            }
            return errors;
        }
    }
}
