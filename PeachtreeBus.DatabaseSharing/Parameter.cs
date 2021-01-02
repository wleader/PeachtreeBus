namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// A Parameter that can be converted into the approriated underlying DB Specific Parameter for Parameterized Queries and statements.
    /// </summary>
    public class Parameter
    {
        public string Name { get; set; }
        public System.Data.DbType Type { get; set; }
        public object Value { get; set; }
        public Parameter(string name, System.Data.DbType type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
