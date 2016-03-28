using System.Reflection;

namespace MetaPatterns.Bases
{
    public abstract class MetaPatternsFactory
    {
        private readonly IMetaPatternsPlatform _platform;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MetaPatternsFactory(IMetaPatternsPlatform platform)
        {
            _platform = platform;
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //protected void CompileType(object key)
        //{

        //}


        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //protected object CreateInstanceOfType(object key)
        //{

        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //protected virtual string GetTypeFullName()
        //{

        //}
    }
}
