using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace LibraryManagementService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void AddBook(Book book);

        [OperationContract]
        void RemoveBook(Book book);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void UpdateBook(Book book, Book newBook);

        [OperationContract]
        List<Book> GetAllBooks();

        [OperationContract]
        Book LookupBookByISBN(string ISBN);

        [OperationContract]
        List<Book> LookupBooksByTitle(string Title);

        [OperationContract]
        List<Book> LookupBooksByAuthor(string Author);

        [OperationContract]
        bool CheckBookAvailability(Book book);

        [OperationContract]
        void UpdateBookAvailability(Book book, bool isAvailable);

        [OperationContract]
        List<Book> GetBooksByAvailability(bool isAvailable);
    }

    [DataContract]
    public class Book
    {
        private string _isbn;

        [DataMember]
        public string ISBN
        {
            get { return _isbn; }
            set
            {
                _isbn = value;
                FormattedISBN = FormatISBN(value);
            }
        }

        public string FormattedISBN { get; set; }

        private string _title;

        [DataMember]
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                FormattedTitle = _title.Trim().ToLower();
            }
        }

        public string FormattedTitle { get; set; }

        private string _author;

        [DataMember]
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                FormattedAuthor = _author.Trim().ToLower();
            }
        }

        public string FormattedAuthor { get; set; }

        [DataMember]
        public bool IsAvailable { get; set; }

        private string FormatISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return string.Empty;

            return isbn.Trim().Replace("-", "").Replace(" ", "");
        }
    }

    // custom validation fault to allow list
    [DataContract]
    public class ValidationFault
    {
        [DataMember]
        public List<string> Errors { get; set; } = new List<string>();

        public bool HasErrors => Errors.Count > 0;
    }

}
