using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Domain.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message, 404) { }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id '{key}' was not found.", 404) { }
    }
}
