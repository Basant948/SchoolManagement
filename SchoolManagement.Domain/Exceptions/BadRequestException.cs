using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Domain.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message) : base(message, 400) { }
    }
}
