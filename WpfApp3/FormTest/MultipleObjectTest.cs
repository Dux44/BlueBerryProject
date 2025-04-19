using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BlueBerryProject.FormConstruct;

namespace BlueBerryProject.FormTest
{
    internal class MultipleObjectTest
    {
        public Control Selector { get; set; }
        public string AnswerText { get; set; }
        public MultipleObject.AnswerState State { get; set; }
    }
}
