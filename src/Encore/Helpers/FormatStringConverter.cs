using Storage;
using System.ComponentModel;

namespace Encore.Helpers;
public class FormatStringConverter : StringConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;
    public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) => new StandardValuesCollection(UserFileSystem.PCDriveList);
}

