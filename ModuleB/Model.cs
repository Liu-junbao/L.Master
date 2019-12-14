using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    [GenericName("实体模型")]
    public class Model
    {
        [GenericName("Id")]
        public int Id { get; set; }
        [GenericName("名称")]
        public string Name { get; set; }
        [GenericName("年龄")]
        public int Age { get; set; }
        [GenericName("描述")]
        public string Descripton { get; set; }
        public string Text { get; set; }
    }
}
