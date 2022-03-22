//using System;
//using System.Collections.Generic;

//namespace DomainValues.Model
//{
//    internal class DataBlock
//    {
//        public DataBlock(string table)
//        {
//            Table = table;
//            Data = new Dictionary<Column, List<string>>();
//        }
//        public string Table { get; }
//        public Dictionary<Column,List<string>> Data { get; }
//        public bool IsEnumInternal { get; set; }
//        public string EnumBaseType { get; set; }
//        public bool EnumHasFlagsAttribute { get; set; }
//        public string EnumName { get; set; }
//        public string EnumDescField { get; set; }
//        public string EnumMemberField { get; set; }
//        public string EnumInitField { get; set; }

//        public Type GetBaseType()
//        {
//            switch (EnumBaseType.ToLower())
//            {
//                case "int":
//                case "int32":
//                default:
//                    return typeof(int);
//                case "short":
//                case "int16":
//                    return typeof(short);
//                case "long":
//                case "int64":
//                    return typeof(long);
//                case "byte":
//                    return typeof(byte);
//                case "sbyte":
//                    return typeof(sbyte);
//                case "ushort":
//                    return typeof(ushort);
//                case "uint":
//                    return typeof(uint);
//                case "ulong":
//                    return typeof(ulong);
//            }
//        }
//    }
//}
