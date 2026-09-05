using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EnterpriseTaskManagement.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Role()
        {

        }

        public Role(string name)
        {
            Name = name;
        }
    }
}
