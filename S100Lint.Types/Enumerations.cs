namespace S100Lint.Types
{
    public class Enumerations
    {
        public enum Level
        {
            Info = 1,
            Critical = 10,
            Error = 20,
            Warning = 30
        } 

        public enum Chapter
        {
            SimpleTypes = 10,
            ComplexTypes = 20,
            FeatureCatalogueBaseCheck = 30
        }

        public enum Type
        {
            Info = 1,
            SimpleType = 10,
            ComplexType = 20,
            Role = 30,
            InformationAssociation = 40,
            FeatureAssociation = 50,
            InformationType = 60,
            FeatureType = 70, 
            MetaFeatureType = 80,
            FeatureMemberType = 90,
            ComplexAttribute = 100,
            SimpleAttribute = 110
        }
    }
}
