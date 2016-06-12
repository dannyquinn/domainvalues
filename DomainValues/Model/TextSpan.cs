namespace DomainValues.Model
{
    internal class TextSpan
    {
        public TextSpan(int start,string text)
        {
            Start = start;
            Text = text;
        }
        public int Start { get; }
        public string Text { get; }
    }
}
