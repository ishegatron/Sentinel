namespace Sentinel.Filters
{
    #region Using directives

    using Interfaces;
    using Sentinel.Interfaces;
    using System.Runtime.Serialization;

    #endregion Using directives

    [DataContract]
    public class SearchFilter
        : Filter, IDefaultInitialisation, ISearchFilter
    {
        public SearchFilter()
        {
        }
      
        public void Initialise()
        {
            base.Name = "SearchFilter";
            base.Field = LogEntryField.System;
            base.Pattern = string.Empty;
        }     
    }
}