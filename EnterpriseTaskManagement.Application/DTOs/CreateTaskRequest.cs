using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
