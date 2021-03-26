using System.Text;

namespace UltraMapper.Json
{
    //Encapsulating data in this ad-hoc class is a neat way 
    //to inform the mapper to use a specific UltraMapper extension
    //to perform its operations.
    //
    //Only one instance of this class will be created per parser instance.
    //This one instance will be reused over and over again to pass data to the ExpressionBuilder.
    //To further improve performance members inside this class are declared as fields.
    public class JsonString
    {
        public StringBuilder Json = new StringBuilder();

        private int _indentations = 0;

        public string IndentationString { get; private set; }

        public int Indentation
        {
            get => _indentations;
            set
            {
                _indentations = value;
                IndentationString = new string( '\t', value );
            }
        }

    }
}
