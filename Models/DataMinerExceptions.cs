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

    public class ProcessDocumentContentException : Exception
    {
        public ProcessDocumentContentException() { }

        public ProcessDocumentContentException(string message)
            : base(message)  { }

        public ProcessDocumentContentException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class SaveToAzureException : Exception
    {
        public SaveToAzureException() { }

        public SaveToAzureException(string message)
            : base(message)  { }

        public SaveToAzureException(string message, Exception inner)
            : base(message, inner) { }
    }


    public class SaveToLogException : Exception
    {
        public SaveToLogException() { }

        public SaveToLogException(string message)
            : base(message)  { }

        public SaveToLogException(string message, Exception inner)
            : base(message, inner) { }
    }



}