using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Exceptions;

namespace Domain.Exceptions
{
    public class ReglasdeNegocioException : ApiException
    {
        public ReglasdeNegocioException(string message) : base(message)
        {

        }
    }
}
