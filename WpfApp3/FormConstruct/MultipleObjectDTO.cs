using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBerryProject.FormConstruct.MultipleObject;

namespace BlueBerryProject.FormConstruct
{
    public class MultipleObjectsDTO
    {
        public class MultipleObjectDTO
        {
            public MultipleObjectDTO() 
            { }
            [JsonRequired]
            public string AnswerType { get; set; }
            [JsonRequired]
            public AnswerState State { get; set; }
            [JsonRequired]
            public string AnswerText { get; set; }
        }
    }
}
