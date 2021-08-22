﻿using Storage;
using System.ComponentModel;

namespace Encore.Helpers
{
    public class FormatStringConverter : StringConverter
    {
        
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            //List<string> list = UserFileSystem.Drives?.Keys?.ToList();
            return new StandardValuesCollection(UserFileSystem.PCDriveList);
        }
    }
}
