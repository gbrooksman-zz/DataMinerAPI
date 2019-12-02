using System;

namespace DataMinerAPI.Models
{
    public class MissingApplicationException : Exception
    {
        public MissingApplicationException() { }

        public MissingApplicationException(string message)
            : base(message)  { }

        public MissingApplicationException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class MissingRequestGuidException : Exception
    {
        public MissingRequestGuidException() { }

        public MissingRequestGuidException(string message)
            : base(message)  { }

        public MissingRequestGuidException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class MissingDocumentContentException : Exception
    {
        public MissingDocumentContentException() { }

        public MissingDocumentContentException(string message)
            : base(message)  { }

        public MissingDocumentContentException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class MissingKeywordsException : Exception
    {
        public MissingKeywordsException() { }

        public MissingKeywordsException(string message)
            : base(message)  { }

        public MissingKeywordsException(string message, Exception inner)
            : base(message, inner) { }
    }

}