using LibraryManagementClient.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Service1Client client = new Service1Client();

            Console.WriteLine("\n           Library Management Service");
            Console.WriteLine("================================================");

            int selectedOperation = 0;

            do
            {
                do
                {
                    Console.WriteLine("\n---- Available Operations ----");
                    Console.WriteLine("1. View all books");
                    Console.WriteLine("2. Add a new book");
                    Console.WriteLine("3. Search for book(s)");
                    Console.WriteLine("4. Exit the application");
                    Console.Write("\nPlease enter the number of your selection: ");

                    int.TryParse(Console.ReadLine(), out selectedOperation);

                    if (selectedOperation < 0 || selectedOperation > 4)
                        selectedOperation = 0;

                    if (selectedOperation == 0)
                        Console.WriteLine("Invalid selection.\n");

                } while (selectedOperation == 0);

                switch (selectedOperation)
                {
                    case 1:
                        DisplayBookList(() => client.GetAllBooks(), client, "Would you like to return to the menu?", "Returning to menu", "List of All Books");
                        break;

                    case 2:
                        Console.WriteLine("\n---- Enter New Book Information ----");
                        Book book = GetBookDataInput();

                        try
                        {
                            client.AddBook(book);
                            Console.WriteLine("\nBook added successfully!\n");
                        }
                        catch (FaultException<ValidationFault> ex)
                        {
                            Console.WriteLine("\nError: Unable to add book!");
                            foreach (string error in ex.Detail.Errors)
                                Console.WriteLine($" - {error}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\nError: Unable to add book!");
                            Console.WriteLine($" - {ex.Message}");
                        }

                        break;

                    case 3:
                        int secondarySelectOption = 0;
                        bool returnToMenu = false;

                        do
                        {
                            do
                            {
                                Console.WriteLine("\n---- Search Options ----");
                                Console.WriteLine("1. Search by ISBN");
                                Console.WriteLine("2. Search by Title");
                                Console.WriteLine("3. Search by Author");
                                Console.WriteLine("4. Search by Availability");
                                Console.WriteLine("5. Return to Menu");

                                Console.Write("Enter the number of your selection: ");
                                string response = Console.ReadLine();

                                int.TryParse(response, out secondarySelectOption);

                                if (secondarySelectOption < 0 || secondarySelectOption > 5)
                                    secondarySelectOption = 0;

                                if (secondarySelectOption == 0)
                                    Console.WriteLine("Invalid selection. \n");

                            } while (secondarySelectOption == 0);

                            bool continueSearching = true;

                            switch (secondarySelectOption)
                            {
                                case 1:
                                    do
                                    {
                                        Console.Write("Enter the ISBN to search for: ");
                                        string response = Console.ReadLine().Trim().ToLower();

                                        try
                                        {
                                            Book foundBook = client.LookupBookByISBN(response);
                                            ViewEditBook(foundBook, client);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Error: Unable to locate book!");
                                            Console.WriteLine($" - {ex.Message}");
                                        }
                                        finally
                                        {
                                            continueSearching = ValidateYesNo("Would you like to search for another ISBN?");
                                        }

                                    } while (continueSearching);
                                    break;

                                case 2:
                                    do
                                    {
                                        Console.Write("Enter the title to search for: ");
                                        string response = Console.ReadLine();
                                        string formattedResponse = response.Trim().ToLower();

                                        string displayTitleString = $"with the Title '{response}'";

                                        DisplayBookList(() => client.LookupBooksByTitle(formattedResponse), client,
                                            "Would you like to return to the search menu?", "Returning to search...", displayTitleString);

                                        continueSearching = ValidateYesNo("Would you like to search for another title?");

                                    } while (continueSearching);
                                    break;

                                case 3:
                                    do
                                    {
                                        Console.Write("Enter the author to search for: ");
                                        string response = Console.ReadLine();
                                        string formattedResponse = response.Trim().ToLower();

                                        string displayTitleString = $"by Author {response}";

                                        DisplayBookList(() => client.LookupBooksByAuthor(formattedResponse), client,
                                            "Would you like to return to the search menu?", "Returning to search...", displayTitleString);

                                        continueSearching = ValidateYesNo("\nWould you like to search for another author?");

                                    } while (continueSearching);
                                    break;

                                case 4:
                                    do
                                    {
                                        var responses = new Dictionary<string, bool>
                                        {
                                            { "available", true },
                                            { "unavailable", false },
                                            { "true", true },
                                            { "false", false },
                                            { "yes", true },
                                            { "no", false },
                                            { "y", true },
                                            { "n", false }
                                        };

                                        bool? responseBool = null;

                                        do
                                        {
                                            Console.Write("Enter the availability status to search for (e.g. 'available'/'unavailable'): ");
                                            string response = Console.ReadLine();
                                            string formattedResponse = response.Trim().ToLower();

                                            if (responses.ContainsKey(formattedResponse))
                                                responseBool = responses[formattedResponse];

                                            else
                                                Console.WriteLine("Invalid response.\n");

                                        } while (responseBool == null);

                                        bool searchBool = responseBool ?? false;
                                        string status = searchBool ? "Available" : "Unavailable";

                                        string displayTitleString = $"with {status} Status";

                                        DisplayBookList(() => client.GetBooksByAvailability(searchBool), client, 
                                            "Would you like to return to the search menu?", "Returning to search...", displayTitleString);

                                        continueSearching = ValidateYesNo("\nWould you like to search by availability again?");

                                    } while (continueSearching);
                                    break;

                                case 5:
                                    Console.WriteLine("Returning to menu...");
                                    returnToMenu = true;
                                    break;
                            }

                        } while (!returnToMenu);
                        break;

                    case 4:
                        Console.WriteLine("Shutting down the application...");
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                        client.Close();
                        break;
                }

            } while (selectedOperation != 4);
        }

        // -- helper methods --
        private static bool ValidateYesNo(string printString)
        {
            var validYesNo = new Dictionary<string, bool>
            {
                { "yes", true },
                { "no", false },
                { "y", true },
                { "n", false },
                { "true", true },
                { "false", false }
            };
            bool? value = null;

            do
            {
                Console.Write($"{printString} (y/n) ");
                string response = Console.ReadLine();

                response = response.Trim().ToLower();

                if (validYesNo.ContainsKey(response))
                    value = validYesNo[response];
                else
                    Console.WriteLine("Invalid response.\n");
            } while (value == null);

            bool returnValue = value ?? false;
            return returnValue;
        }

        private static Book GetBookDataInput()
        {
            Console.Write("Enter the ISBN: ");
            string isbn = Console.ReadLine();
            Console.Write("Enter the title: ");
            string title = Console.ReadLine();
            Console.Write("Enter the author: ");
            string author = Console.ReadLine();
            bool isAvailable = ValidateYesNo("Is this book available?");

            bool userApproval = false;

            Book book = new Book
            {
                ISBN = isbn,
                Title = title,
                Author = author,
                IsAvailable = isAvailable
            };

            do
            {
                string availabilityString = book.IsAvailable ? "Yes" : "No";

                Console.WriteLine("\n---- Book Information ----");
                Console.WriteLine($"     ISBN:   {book.ISBN}");
                Console.WriteLine($"    Title:   {book.Title}");
                Console.WriteLine($"   Author:   {book.Author}");
                Console.WriteLine($"Available?   {availabilityString}");

                Console.WriteLine();

                userApproval = ValidateYesNo("Does this look correct?");

                if (!userApproval)
                {
                    book = ChangeBookDetails(book, "Which of the values is incorrect?");
                }
            } while (!userApproval);

            return book;
        }

        private static Book ChangeBookDetails(Book book, string prompt)
        {
            int fieldToUpdate = 0;
            Book updatedBook = book;

            do
            {
                Console.WriteLine($"\n{prompt}");
                Console.WriteLine("1. ISBN");
                Console.WriteLine("2. Title");
                Console.WriteLine("3. Author");
                Console.WriteLine("4. Availability");
                Console.Write("Enter the number of the field you'd like to change: ");

                int.TryParse(Console.ReadLine(), out fieldToUpdate);

                if (fieldToUpdate < 0 || fieldToUpdate > 4)
                    fieldToUpdate = 0;

                if (fieldToUpdate == 0)
                    Console.WriteLine("Invalid selection.\n");
            } while (fieldToUpdate == 0);

            switch (fieldToUpdate)
            {
                case 1:
                    Console.Write("Enter the updated ISBN: ");
                    updatedBook.ISBN = Console.ReadLine();
                    break;

                case 2:
                    Console.Write("Enter the updated title: ");
                    updatedBook.Title = Console.ReadLine();
                    break;

                case 3:
                    Console.Write("Enter the updated author: ");
                    updatedBook.Author = Console.ReadLine();
                    break;

                case 4:
                    updatedBook.IsAvailable = ValidateYesNo("Should this book be available?");
                    break;
            }

            return updatedBook;
        }

        private static void ViewEditBook(Book book, Service1Client client)
        {
            bool continueViewing = true;
            do
            {
                DisplayBookDetails(book, client);

                Console.WriteLine("\n---- Options ----");
                Console.WriteLine("1. Check out book");
                Console.WriteLine("2. Return book");
                Console.WriteLine("3. View book availability");
                Console.WriteLine("4. Edit book details");
                Console.WriteLine("5. Delete this book");
                Console.WriteLine("6. Cancel");

                int bookOperation = 0;
                do
                {
                    Console.Write("Please enter the number of your selection: ");
                    string response = Console.ReadLine();

                    int.TryParse(response, out bookOperation);

                    if (bookOperation < 0 || bookOperation > 6)
                        bookOperation = 0;

                    if (bookOperation == 0)
                        Console.WriteLine("Invalid selection.\n");
                } while (bookOperation == 0);

                switch (bookOperation)
                {
                    case 1:
                        if (!book.IsAvailable)
                        {
                            Console.WriteLine("Cannot check out this book!");
                            Console.WriteLine(" - Book already checked out (unavailable)");
                            break;
                        }
                        try
                        {
                            client.UpdateBookAvailability(book, false);
                            book.IsAvailable = false;    // update local object
                            Console.WriteLine($"'{book.Title}' successfully checked out!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: Unable to update book availability!");
                            Console.WriteLine($" - {ex.Message}");
                        }
                        break;

                    case 2:
                        if (book.IsAvailable)
                        {
                            Console.WriteLine("Cannot return this book!");
                            Console.WriteLine(" - Book already checked in (available)");
                            break;
                        }
                        try
                        {
                            client.UpdateBookAvailability(book, true);
                            book.IsAvailable = true;   // update local object
                            Console.WriteLine($"'{book.Title}' successfully returned!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: Unable to update book availability!");
                            Console.WriteLine($" - {ex.Message}");
                        }
                        break;

                    case 3:
                        try
                        {
                            bool availability = client.CheckBookAvailability(book);
                            string result = availability ? "\nThis book is currently *available*! :D" : "\nThis book is currently *unavailable*! :(";
                            Console.WriteLine(result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: Unable to check book availability!");
                            Console.WriteLine($" - {ex.Message}");
                        }
                        break;

                    case 4:
                        bool continueEditing = true;
                        Book updatedBook = new Book
                        {
                            ISBN = book.ISBN,
                            Title = book.Title,
                            Author = book.Author,
                            IsAvailable = book.IsAvailable
                        };

                        do
                        {
                            updatedBook = ChangeBookDetails(updatedBook, "\nWhich of the following would you like to update?");

                            continueEditing = ValidateYesNo("Would you like to update another field?");

                        } while (continueEditing);

                        try
                        {
                            client.UpdateBook(book, updatedBook);
                            book = updatedBook;
                            Console.WriteLine("Book updated successfully!");
                        }
                        catch (FaultException<ValidationFault> ex)
                        {
                            Console.WriteLine("Error: Unable to save book details!");
                            foreach (string error in ex.Detail.Errors)
                                Console.WriteLine($" - {error}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: Unable to save book details!");
                            Console.WriteLine($" - {ex.Message}");
                        }

                        break;

                    case 5:
                        Console.WriteLine("\nAre you absolutely certain you would like to delete this book?");
                        Console.WriteLine("!!!! This action cannot be undone !!!!");
                        Console.Write("Enter the word 'delete' to confirm (enter anything else to cancel): ");
                        string response = Console.ReadLine().Trim().ToLower();
                        if (response == "delete")
                        {
                            try
                            {
                                client.RemoveBook(book);
                                Console.WriteLine("Book successfully deleted.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Unable to delete book!");
                                Console.WriteLine($" - {ex.Message}");
                            }
                        }
                        continueViewing = false;
                        break;

                    case 6:
                        Console.WriteLine("Cancelling...");
                        continueViewing = false;
                        break;
                }
            } while (continueViewing);
        }

        private static void DisplayBookDetails(Book book, Service1Client client)
        {
            string availabilityString = "";
            
            Console.WriteLine($"\n---- {book.Title} ----");
            Console.WriteLine($"     ISBN: {book.ISBN}");
            Console.WriteLine($"   Author: {book.Author}");

            try
            {
                bool isAvailable = client.CheckBookAvailability(book);
                availabilityString = isAvailable ? "Yes" : "No";
            } 
            catch
            {
                availabilityString = "Error Checking Availability";
            }
            finally
            {
                Console.WriteLine($"Available? {availabilityString}");
            }
            
        }

        private static void DisplayBookList(Func<List<Book>> getBooks, Service1Client client, string exitPrompt, string exitString, string displayTitleString)
        {
            bool returnToPrevious = false;

            do
            {
                List<Book> books = null;

                string errorString = "";

                try
                {
                    books = getBooks();
                }
                catch (Exception ex)
                {
                    errorString += "Error searching for books!\n";
                    errorString += $" - {ex.Message}";
                }

                string plural = "";

                if (books != null)
                    plural = books.Count > 1 ? "Books" : "Book";
                else
                    plural = "Books";

                string displayTitle = $"{plural} {displayTitleString}";
                Console.WriteLine($"\n---- {displayTitle} ----");
                Console.WriteLine(errorString);

                if (books != null)
                {
                    for (int i = 0; i < books.Count; i++)
                    {
                        string availabilityString = "";

                        try
                        {
                            bool isAvailable = client.CheckBookAvailability(books[i]);
                            availabilityString = isAvailable ? "Available" : "Unavailable";
                        }
                        catch
                        {
                            availabilityString = "Error Checking Availability";
                        }

                        Console.WriteLine($"  {i + 1}.  Title: {books[i].Title} | Author: {books[i].Author} | {availabilityString} | ISBN: {books[i].ISBN} ");
                    }
                }

                if (books != null && ValidateYesNo("\nWould you like to view a specific book?"))
                {
                    int selectedBookIndex = -1;
                    do
                    {
                        Console.Write("Please enter the number corresponding to the book you'd like to view: ");
                        string response = Console.ReadLine();
                        int.TryParse(response, out selectedBookIndex);

                        if (selectedBookIndex < 0 || selectedBookIndex > books.Count)
                            selectedBookIndex = -1;

                        if (selectedBookIndex == -1)
                            Console.WriteLine("Invalid selection.\n");

                    } while (selectedBookIndex == -1);

                    ViewEditBook(books[selectedBookIndex - 1], client);

                    returnToPrevious = ValidateYesNo(exitPrompt);

                    if (returnToPrevious)
                        Console.WriteLine($"{exitString}...");
                }
                else
                {
                    returnToPrevious = true;
                }
            } while (!returnToPrevious);
            
        }
    }
}
