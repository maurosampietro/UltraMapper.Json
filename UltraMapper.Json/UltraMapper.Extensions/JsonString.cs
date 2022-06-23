using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace UltraMapper.Json
{
    //Encapsulating data in this ad-hoc class is a neat way 
    //to inform the mapper to use a specific UltraMapper extension
    //to perform its operations.
    //
    //Only one instance of this class will be created per parser instance.
    //This one instance will be reused over and over again to pass data to the ExpressionBuilder.
    public class JsonString
    {
        public StringBuilder Json = new StringBuilder();

        public string IndentationString { get; private set; }

        private int _indentation = 0;
        public int Indentation
        {
            get => _indentation;
            set
            {
                _indentation = value;

                if( _indentation < _indentStrs.Count )
                    this.IndentationString = _indentStrs[ _indentation ];
                else
                {
                    this.IndentationString = new string( '\t', _indentation );
                    _indentStrs.Add( this.IndentationString );
                }
            }
        }

        //No need to be static since every parser should use 1 instance of JsonString.
        //Locking or ConcurrentCollections are actually slower techniques than
        //creating the indentationstring every single time
        private readonly List<string> _indentStrs = new List<string>();
    }
}
