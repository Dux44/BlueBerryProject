using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBerryProject.FormConstruct
{
    public class ResponseDTO
    {
        public ResponseDTO() { }

        public int MaxValue { get; set; }
        public double LoverValue { get; set; }
        public double UpperValue { get; set; }
        public bool IsChecked { get; set; }
        public string RedZoneText { get; set; }
        public string YellowZoneText { get; set; }
        public string GreenZoneText { get;set; }
    }
}
