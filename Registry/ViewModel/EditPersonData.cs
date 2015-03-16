using DataLib;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry
{
    class EditPersonData : ObservableObject
    {

        private Person person;

        public EditPersonData()
        {

        }


        //TODO: rework into loading photo from base. Probably worth using IsAsync binding property
        public string PhotoSource
        {
            get
            {
                switch (person.GenderId)
                {
                    case 3:
                        return "pack://application:,,,/Resources;component/Images/Woman48x48.png";
                    case 2:
                        return "pack://application:,,,/Resources;component/Images/Man48x48.png";
                    default:
                        return "pack://application:,,,/Resources;component/Images/Question48x48.png";
                }
            }
        }
    }
}
