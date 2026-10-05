using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Queue.Models
{
    [Table("Agent_EmployeeGroupsEmployee")]
    public class Agent_EmployeeGroupsEmployee
    {
        [Key]
        public Guid idAgent_EmployeeGroupsEmployee { get; set; }

        public Agent_Employee Agent_Employee { get; set; }
        public Agent_EmployeesGroups Agent_EmployeesGroups { get; set; }
    }
}