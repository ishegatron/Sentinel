namespace Sentinel.Support.Converters
{
    using Images;

    public class TypeToSmallImageConverter : TypeToImageConverter
    {
        public TypeToSmallImageConverter()
        {
            quality = ImageQuality.Small;
        }
    }
}