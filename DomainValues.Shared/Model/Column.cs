namespace DomainValues.Shared.Model
{
    internal class Column
    {
        public Column(string text, bool isKey, bool isDbColumn)
        {
            Text = text.ToLower();
            IsKey = isKey;
            IsDbColumn = isDbColumn;
        }

        public string Text { get; }
        public bool IsKey { get; }
        public bool IsDbColumn { get; }

        public override bool Equals(object obj)
        {
            return obj is Column c && c.Text.Equals(Text);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
