using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    [Description("人员信息")]
    public class Model
    {
        [Key]
        [Description("名称")]
        public string Name { get; set; }
        [Description("年龄")]
        public int Age { get; set; }
        public string Descripton { get; set; }
        public string Text { get; set; }
    }
}
