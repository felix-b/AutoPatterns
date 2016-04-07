using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoPatterns.Tests.Examples
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
