using System.Text;

namespace Utils
{
    public class CollectionStringBuilder
    {
        readonly StringBuilder _builder = new();

        bool _appended;

        readonly string _delimiter;

        const string OPENER = "[";
        const string CLOSER = "]";
        const string DEFAULT_DELIMITER = ", ";

        public CollectionStringBuilder(string delimeter = DEFAULT_DELIMITER)
        {
            _delimiter = delimeter;

            _builder.Append(OPENER);
        }

        public void Append(object obj)
        {
            if (_appended)
            {
                _builder.Append(_delimiter);
            }

            _builder.Append(obj);
            _appended = true;
        }

        public string Build()
        {
            _builder.Append(CLOSER);
            return _builder.ToString();
        }
    }
}
