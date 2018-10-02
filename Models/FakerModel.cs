using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Dummy_Data_Generator.Models
{
    public class FakerModel
    {
        #region Name
        public int Id { get; set; }
        public bool IsMale { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string JobTitle { get; set; }
        public string JobDescriptor { get; set; }
        public string JobArea { get; set; }
        public string JobType { get; set; }
        #endregion
    }
}
