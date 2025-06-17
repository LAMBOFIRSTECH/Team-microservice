using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams.CORE.Layer.Models;

public class ValidationErrorResponse
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public List<ValidationError> Errors { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
}