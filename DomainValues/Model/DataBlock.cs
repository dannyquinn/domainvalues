using System.Collections.Generic;

namespace DomainValues.Model
{
    internal class DataBlock
    {
        public DataBlock(string table)
        {
            Table = table;
            Data = new Dictionary<Column, List<string>>();
        }
        public string Table { get; }
        public Dictionary<Column,List<string>> Data { get; }
    }
}
