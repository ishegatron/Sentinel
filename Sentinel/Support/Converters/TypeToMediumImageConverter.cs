namespace Sentinel.Support.Converters
{
    using Images;

    public class TypeToMediumImageConverter : TypeToImageConverter
    {
        public TypeToMediumImageConverter()
        {
            quality = ImageQuality.Medium;
        }
    }
}