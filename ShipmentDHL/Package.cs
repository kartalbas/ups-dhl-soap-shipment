using ShipmentLib;
using System.Globalization;

namespace ShipmentDHL
{
    public class Package
    {
        public Package() : this(3, SettingController.Paket_Length, SettingController.Paket_Width, SettingController.Paket_Height, "PK")
        {
        }

        public Package(double? dWeightInKG, string strLengthInCM, string strWidthInCM, string strHeightInCM, string strPackageType)
        {
            PackageType = strPackageType;

            string strLocale = SettingController.DatabaseCulture;
            WeightInKG = dWeightInKG < 1 ? "1" : dWeightInKG.ToString();

            if (strLengthInCM.Length > 0)
                LengthInCM = int.Parse(strLengthInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
            else
                LengthInCM = int.Parse(SettingController.Paket_Length, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);

            if (strWidthInCM.Length > 0)
                WidthInCM = int.Parse(strWidthInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
            else
                WidthInCM = int.Parse(SettingController.Paket_Width, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);

            if (strHeightInCM.Length > 0)
                HeightInCM = int.Parse(strHeightInCM, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
            else
                HeightInCM = int.Parse(SettingController.Paket_Height, new CultureInfo(strLocale)).ToString(CultureInfo.InvariantCulture);
        }

        public string WeightInKG { get; set; }
        public string LengthInCM { get; set; }
        public string WidthInCM { get; set; }
        public string HeightInCM { get; set; }
        public string PackageType { get; set; }
    }
}
