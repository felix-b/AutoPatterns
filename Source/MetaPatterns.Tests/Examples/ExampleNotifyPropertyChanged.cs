using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public class ExampleNotifyPropertyChanged : INotifyPropertyChanged
    {
        [MetaProgram.Annotation.MetaMember]
        public object AProperty
        {
            get
            {
                return MetaProgram.Proceed<object>();
            }
            set
            {
                MetaProgram.Proceed(value);
                OnPropertyChanged();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MetaProgram.Annotation.DeclaredMember]
        public event PropertyChangedEventHandler PropertyChanged;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MetaProgram.Annotation.NewMember]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
