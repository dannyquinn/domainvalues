namespace DomainValues.Shared.Common
{
    public static class Errors
    {
        public const string DuplicateValue = "{0} '{1}' is a duplicate value.";
        public const string EndOfFile = "Unexpected end of file.";
        public const string EnumDuplicate = "Already found a parameter that looks like the enum '{0}'.";
        public const string EnumNoName = "No name provided for enumeration.";
        public const string ExpectsParam = "'{0}' expects a parameter.";
        public const string Invalid = "Invalid text.";
        public const string KeyMapsToNonDBColumn = "Key value '{0}' is marked as non db in the column row.Cannot be used as a key.";
        public const string NameAlreadyUsed = "{0} named '{1}' already used in this file.";
        public const string NoParams = "'{0}' does not have a parameter.";
        public const string NotFoundInColumns = "{0} value '{1}' not found in the column row.";
        public const string NullAsSpaceAs = "Null as and space as cannot be set to the same value.";
        public const string RowCountMismatch = "Row count doesn't match header.";
        public const string TemplatePatternNotRecognised = "Cannot determine meaning from string.";
        public const string UnexpectedKeyword = "'{0}' was unexpected.Expected '{1}'.";
    }
}
