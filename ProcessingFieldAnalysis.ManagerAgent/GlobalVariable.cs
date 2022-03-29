using System;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    /// <summary>
    /// Global variables
    /// </summary>
    public static class GlobalVariable
    {
        //Application
        public static readonly Guid PROCESSING_FIELD_APPLICATION_GUID = new Guid("E86BBEDE-3395-4E48-8194-713328B0D976");

        //Objects
        public static readonly Guid PROCESSING_FIELD_OBJECT = new Guid("9122C660-2ED7-4BF2-B44E-C168AC4F06AE");
        public static readonly Guid DOCUMENT_OBJECT = new Guid("15C36703-74EA-4FF8-9DFB-AD30ECE7530D");

        //Procesing Field Object Fields
        public static readonly Guid PROCESSING_FIELD_OBJECT_ARTIFACT_ID_FIELD = new Guid("00D9FBCD-8CE0-4A27-AA35-0ED714271B60");
        public static readonly Guid PROCESSING_FIELD_OBJECT_SOURCE_NAME_HASH_FIELD = new Guid("B58ECA21-6B7D-476D-AD9F-CBC673901F20");
        public static readonly Guid PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD = new Guid("0017BF05-8DAA-4381-B346-9EDC59E39F29");
        public static readonly Guid PROCESSING_FIELD_OBJECT_CATEGORY_FIELD = new Guid("D8F957D6-1FFA-45C3-91FA-F1212F8BAF1B");
        public static readonly Guid PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD = new Guid("96A096AD-3CA7-487C-B0EE-B5683E238E0F");
        public static readonly Guid PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD = new Guid("6DD10E5A-33B3-4F23-972C-E8D097CF7F88");
        public static readonly Guid PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD = new Guid("868238A4-45C8-4305-A5F0-6DAF263A5A17");
        public static readonly Guid PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD = new Guid("2581697D-D3E3-4DC5-8E3C-E9C5F05D1531");
        public static readonly Guid PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD = new Guid("1BDD02EF-E645-49D2-8D99-DB23676A9AE3");

        //Document Object Fields
        public static readonly Guid DOCUMENT_OBJECT_OTHER_METADATA_FIELD = new Guid("DCE19DA4-D470-4EFC-A932-AFF489911A37");
        public static readonly Guid DOCUMENT_OBJECT_UNMAPPED_METADATA_MULTI_OBJECT_FIELD = new Guid("867DBF19-A278-473C-8839-E04D382F3748");
    }
}
